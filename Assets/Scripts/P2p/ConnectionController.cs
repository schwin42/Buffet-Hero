using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using GooglePlayGames.BasicApi.Nearby;
using System.Linq;
using GooglePlayGames.BasicApi.Multiplayer;

public class ConnectionController : MonoBehaviour, RealTimeMultiplayerListener
{

	private static ConnectionController _instance;

	public static ConnectionController Instance {
		get {
			if (_instance == null) {
				GameObject.FindObjectOfType<ConnectionController> ();
			}
			return _instance;
		}
	}

	public enum RemoteStatus
	{
		Uninitialized = -1,
		Idle = 0,
		Advertising = 1,
		Discovering = 2,
		EstablishedClient = 3,
		EstablishedHost = 4,
		StartingRoom = 5,
		EstablishedRealTimeMultiplayer = 6,
	}

	//Players
	public List<RemotePlayer> host_ConnectedClients;
	[System.NonSerialized]
	public RemotePlayer
		client_ConnectedHost;
	private List<RemotePlayer> _client_ConnectedClients;

	public List<RemotePlayer> client_ConnectedClients {
		get {
			return _client_ConnectedClients;
		}
		set {
			_client_ConnectedClients = value;
		}
	}

	public List<RemotePlayer> realtime_Participants;

	public List<RemotePlayer> AccessiblePlayers { //Other players in this session

		get {
			try {
				if (remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
					remoteStatus == ConnectionController.RemoteStatus.Advertising) {
					P2pInterfaceController.Instance.WriteToConsole ("AP as host");
					return host_ConnectedClients //Connected players
					.Union (new List<RemotePlayer> { new RemotePlayer (ConnectionController.localEndpointId, DeviceDatabase.Instance.ActiveProfile, false) }).ToList (); //Self
				} else if (remoteStatus == ConnectionController.RemoteStatus.EstablishedClient) {
					P2pInterfaceController.Instance.WriteToConsole ("Getting active players, connected clients: " + client_ConnectedClients.Count);
					List<RemotePlayer> output = client_ConnectedClients //Other connected clients
					.Union (new List<RemotePlayer> { client_ConnectedHost }) //Host
						.Union (new List<RemotePlayer> { new RemotePlayer(ConnectionController.localEndpointId, DeviceDatabase.Instance.ActiveProfile, false) }).ToList (); //Self
					return output;
				} else if (remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) {
					P2pInterfaceController.Instance.WriteToConsole ("Getting active players, realtime participants clients: " + realtime_Participants.Count);
					return realtime_Participants;
				} else {
					P2pInterfaceController.Instance.WriteToConsole ("No accessible players in unestablished remote state: " + remoteStatus);
					return new List<RemotePlayer> { new RemotePlayer(ConnectionController.localEndpointId, DeviceDatabase.Instance.ActiveProfile, false) }; //Self
				}
			} catch (Exception e) {
				P2pInterfaceController.Instance.WriteToConsole ("Exception in AccessiblePlayers: " + e.Message);
				return null;
			} 

		}
	}

	public static string serviceId;
	public RemoteStatus remoteStatus = RemoteStatus.Uninitialized;
	DiscoveryListener discoveryListener;
	public Nearby_MessageListener messageListener;
	public static string localEndpointId;

	public void Awake ()
	{
		_instance = this;
	}

	public void Start ()
	{
		InitializeGpgNearby ();

		InitializeGpgInstance ();

		serviceId = PlayGamesPlatform.Nearby.GetServiceId ();

		localEndpointId = PlayGamesPlatform.Nearby.LocalEndpointId ();

		remoteStatus = RemoteStatus.Idle;
	}
	
	#region remote event handlers
	
	
	public void Client_EnterLobby ()
	{
		P2pInterfaceController.Instance.SetScreenState (AppState.LobbyScreen);
	}
	
	public void Client_StartGame (GameSettings gameStartInfo)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Client_StartGame");
		try {
			remoteStatus = ConnectionController.RemoteStatus.EstablishedClient;
			P2pGameMaster.Instance.LoadGameSettings (gameStartInfo);
			P2pInterfaceController.Instance.SetScreenState (AppState.GameScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in Client_StartGame: " + e.Message);
		}
	}
	
	public void Client_OtherPlayerJoined ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Client: Player joined!");
		P2pInterfaceController.Instance.PlayersInLobby = ConnectionController.Instance.AccessiblePlayers;
	}
	
	public void Client_OtherPlayerLeft ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Client: Player left");
		P2pInterfaceController.Instance.PlayersInLobby = ConnectionController.Instance.AccessiblePlayers;
	}
	
	public void Host_PlayerJoined (RemotePlayer player)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Host: Player joined!");
		host_ConnectedClients.Add (player);
		P2pInterfaceController.Instance.PlayersInLobby = ConnectionController.Instance.AccessiblePlayers;
		P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive (true);
	}
	
	public void Host_PlayerLeft (string remoteEndpointId)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Player left");
		try {
			RemotePlayer playerToRemove = host_ConnectedClients.Single (rp => rp.gpgId == remoteEndpointId);
			host_ConnectedClients.Remove (playerToRemove);
			P2pInterfaceController.Instance.PlayersInLobby = AccessiblePlayers;
			if (host_ConnectedClients.Count == 0) {
				P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive (false);
				
				P2pInterfaceController.Instance.Result_SetPlayButtonInteractive (false); 
				
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Error in Host_PlayerLeft: " + e.Message);
		}
	}
	
	public void ReceiveGameResult (GameResult gameResult)
	{
		try {
			P2pInterfaceController.Instance.WriteToConsole ("Receiving game result: " + gameResult.profileId);
			P2pGameMaster.Instance.otherGameResults.Add (gameResult);
			P2pInterfaceController.Instance.WriteToConsole ("Remote status, game results, connected clients: " + remoteStatus + ", " + 
				P2pGameMaster.Instance.otherGameResults.Count + ", " + realtime_Participants.Count);
			if (remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) { //Broadcast display result event if all games have been received
				if (P2pGameMaster.Instance.otherGameResults.Count == host_ConnectedClients.Count) {
					P2pInterfaceController.Instance.DisplayResult ();
				}
			} else if (remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) {
	
				if (P2pGameMaster.Instance.otherGameResults.Count == realtime_Participants.Count - 1) { //Do not include self in comparison to otherGameResults
					P2pInterfaceController.Instance.DisplayResult ();
				}
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in ReceiveGameResult: " + e.Message);
		}
	}
	
	#endregion

	#region nearby generic

	private void InitializeGpgNearby ()
	{
		PlayGamesPlatform.InitializeNearby ((client) => {
			P2pInterfaceController.Instance.WriteToConsole ("Initiated GpgNearby." + client);
		});
	}

	public void TerminateAllConnections ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Attempting to terminate connections...");
		try {
//			if(PlayGamesPlatform.Instance != null) { //TODO Discover way to clean up Realtime that can be safely used in Nearby
//				PlayGamesPlatform.Instance.RealTime.LeaveRoom();
//			}
			PlayGamesPlatform.Nearby.StopAllConnections ();
			remoteStatus = RemoteStatus.Idle;
			discoveryListener = null;
			messageListener = null;
			client_ConnectedHost = null;
			host_ConnectedClients = null;
			client_ConnectedClients = null;
			P2pInterfaceController.Instance.WriteToConsole ("TerminateAllConnections completed successfully");
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in TerminateAllConnections: " + e.Message);
		}
	}

	public void BroadcastEvent (Payload payload)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Broadcasting event");
		try {
			if (remoteStatus == RemoteStatus.EstablishedClient) {
				if (client_ConnectedHost == null) {
					P2pInterfaceController.Instance.WriteToConsole ("Failed to broadcast event " + payload + " because host is null");
					return;
				}
				PlayGamesPlatform.Nearby.SendReliable (new List<string> { client_ConnectedHost.gpgId }, 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to host");
			} else if (remoteStatus == RemoteStatus.EstablishedHost) {
				if (host_ConnectedClients == null || host_ConnectedClients.Count == 0) {
					P2pInterfaceController.Instance.WriteToConsole ("Failed to broadcast event " + payload + " because clients list is empty");
					return;
				}
				PlayGamesPlatform.Nearby.SendReliable (host_ConnectedClients.Select (p => p.gpgId).ToList (), 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to " + host_ConnectedClients.Count + " clients");
			} else if (remoteStatus == RemoteStatus.EstablishedRealTimeMultiplayer) {
				P2pInterfaceController.Instance.WriteToConsole ("Sending payload to realtime peers");
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, Utility.PayloadToByteArray (payload));
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Failed to send message while in " + remoteStatus);
				return;
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole (e.Message);
		}
	}
	#endregion

	# region nearby host

	public void Host_BeginAdvertising ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("beginning advertising @" + DateTime.Now);
		host_ConnectedClients = new List<RemotePlayer> ();
		List<string> appIdentifiers = new List<string> ();
		appIdentifiers.Add (PlayGamesPlatform.Nearby.GetAppBundleId ());
		PlayGamesPlatform.Nearby.StartAdvertising (
			"DebugSession42", //TODO User determines session name
			appIdentifiers,
			TimeSpan.FromSeconds (0),
			(AdvertisingResult result) => {
			P2pInterfaceController.Instance.WriteToConsole ("advertising result: " + result.Status + ", " + result.Succeeded); //TODO Throw useful error to user
		},
		Host_HandleConnectionRequest);

		remoteStatus = RemoteStatus.Advertising;
	}

	public void Host_HandleConnectionRequest (ConnectionRequest request)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Received connection request: " +
			request.RemoteEndpoint.DeviceId + " " +
			request.RemoteEndpoint.EndpointId + " " +
			request.RemoteEndpoint.Name);

		messageListener = new Nearby_MessageListener (this, Nearby_MessageListener.ListenerMode.ListeningToClients);

		try {
			PlayGamesPlatform.Nearby.AcceptConnectionRequest (
				request.RemoteEndpoint.EndpointId,
				Utility.PayloadToByteArray (new WelcomePayload (new RemotePlayer (PlayGamesPlatform.Nearby.LocalEndpointId (), DeviceDatabase.Instance.ActiveProfile, false), host_ConnectedClients)),
				messageListener);

			RemotePlayer remotePlayer = ((RemotePlayerPayload)Utility.ByteArrayToPayload (request.Payload)).remotePlayer; //TODO better type validation/ error checking
			P2pInterfaceController.Instance.WriteToConsole ("Accepted connection request from " + request.RemoteEndpoint.EndpointId);
			Host_PlayerJoined (remotePlayer);
			Host_EchoMessageToOtherClients (request.RemoteEndpoint.EndpointId, request.Payload, true);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole (e.Message);
		}
	}

	public void Host_BeginSession ()
	{
		PlayGamesPlatform.Nearby.StopAdvertising ();
		remoteStatus = RemoteStatus.EstablishedHost;
		P2pInterfaceController.Instance.WriteToConsole ("Session started");
	}

	private void Host_EchoMessageToOtherClients (string sourceEndpointId, byte[] data, bool isReliableMessage)
	{
		//Send to every client but the source
		P2pInterfaceController.Instance.WriteToConsole ("Host beginning echo procedure");
	
		List<string> endpointsToMessage = host_ConnectedClients.Where (rp => rp.gpgId != sourceEndpointId).Select (rp => rp.gpgId).ToList ();

		if (isReliableMessage) {
			PlayGamesPlatform.Nearby.SendReliable (endpointsToMessage, data);
		} else {
			PlayGamesPlatform.Nearby.SendUnreliable (endpointsToMessage, data);
		}
		P2pInterfaceController.Instance.WriteToConsole ("Host echoed to: " + endpointsToMessage.Count + " endpoints");
	}

	#endregion

	#region nearby client

	public void Client_BeginDiscovery ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Beginning discovery");
		client_ConnectedHost = null;
		client_ConnectedClients = new List<RemotePlayer> ();
		discoveryListener = new DiscoveryListener (this);

		PlayGamesPlatform.Nearby.StartDiscovery (
			serviceId,
			TimeSpan.FromSeconds (0),
			discoveryListener);

		P2pInterfaceController.Instance.WriteToConsole ("Discovery in progress");
	}

	#endregion

	#region Nearby listeners

	public class DiscoveryListener : IDiscoveryListener
	{
		
		public void OnEndpointFound (EndpointDetails discoveredEndpoint)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Endpoint found, sending connection request @" + Time.time);
			
			Client_SendConnectionRequest (discoveredEndpoint);
			
		}
		
		public void OnEndpointLost (string lostEndpointId)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Endpoint lost");
		}

		public void Client_SendConnectionRequest (EndpointDetails discoveredEndpoint)
		{
			try {
				ConnectionController.Instance.messageListener = new Nearby_MessageListener (connectionController, Nearby_MessageListener.ListenerMode.ListeningToHost);
				PlayGamesPlatform.Nearby.SendConnectionRequest (
				"Marco Polo",
				discoveredEndpoint.EndpointId,
				Utility.PayloadToByteArray (new RemotePlayerPayload (new RemotePlayer (PlayGamesPlatform.Nearby.LocalEndpointId (), DeviceDatabase.Instance.ActiveProfile, false))),
				Client_HandleConnectionResponse,
				(IMessageListener)ConnectionController.Instance.messageListener);
				P2pInterfaceController.Instance.WriteToConsole ("Connection request sent to endpoint");
			} catch (Exception e) {
				P2pInterfaceController.Instance.WriteToConsole ("Error: " + e.Message);
				return;
			}
		}

		public void Client_HandleConnectionResponse (ConnectionResponse response)
		{
			if (response.ResponseStatus == ConnectionResponse.Status.Accepted) {
				Client_HandleSuccessfulConnectionToHost (response);
			} else {	
				P2pInterfaceController.Instance.WriteToConsole ("Connection failed: " + response.ResponseStatus); //TODO Connection failed, so retry or begin discovery again
			}
		}

		private void Client_HandleSuccessfulConnectionToHost (ConnectionResponse response)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Connection successful!");
			PlayGamesPlatform.Nearby.StopDiscovery (ConnectionController.serviceId);
			WelcomePayload welcomePayload = (WelcomePayload)Utility.ByteArrayToPayload (response.Payload);
			connectionController.client_ConnectedHost = welcomePayload.hostPlayer;
			connectionController.client_ConnectedClients = welcomePayload.fellowClients;
			connectionController.remoteStatus = RemoteStatus.EstablishedClient;
			connectionController.Client_EnterLobby ();
		}

		private ConnectionController connectionController;

		public DiscoveryListener (ConnectionController connectionController)
		{
			this.connectionController = connectionController;
		}
	}
	
	public class Nearby_MessageListener : IMessageListener
	{
		private ConnectionController connectionController;

		public enum ListenerMode
		{
			Inactive = -1,
			ListeningToHost = 0,
			ListeningToClients = 1,
		}

		public ListenerMode listenerMode = ListenerMode.Inactive;

		public Nearby_MessageListener (ConnectionController connectionController, ListenerMode listenerMode)
		{
			this.listenerMode = listenerMode;
			this.connectionController = connectionController;
		}

		public void OnMessageReceived (string remoteEndpointId, byte[] data, bool isReliableMessage)
		{
			Payload payload = Utility.ByteArrayToPayload (data);
			P2pInterfaceController.Instance.WriteToConsole ("Received message: " + payload);
			if (payload is StartGamePayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Received start game event");
				P2pInterfaceController.Instance.WriteToConsole ("connection controller = " + (connectionController == null ? "null" : "not null"));
				P2pInterfaceController.Instance.WriteToConsole ("payload start game form seed: " +
					((StartGamePayload)payload).gameStartInfo.formSeed);
				connectionController.Client_StartGame (((StartGamePayload)payload).gameStartInfo);
			} else if (payload is GameResultPayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Received game result");
				//If host, echo result to all other clients
				if (listenerMode == ListenerMode.ListeningToClients)
					ConnectionController.Instance.Host_EchoMessageToOtherClients (remoteEndpointId, data, isReliableMessage);
				connectionController.ReceiveGameResult (((GameResultPayload)payload).gameResult);
			} else if (payload is DisplayResultsEvent) {
				P2pInterfaceController.Instance.WriteToConsole ("Received display result event");
				P2pInterfaceController.Instance.DisplayResult ();
			} else if (payload is PlayerLeftPayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Recieved player left payload");
				if (connectionController.remoteStatus == RemoteStatus.EstablishedClient) {
					string droppedEndpointId = ((PlayerLeftPayload)payload).droppedEndpointId;
					connectionController.client_ConnectedClients.RemoveAll (rp => rp.gpgId == droppedEndpointId);
					connectionController.Client_OtherPlayerLeft ();
				} else {
					P2pInterfaceController.Instance.WriteToConsole ("Unexpected player left event outside of established client mode");
				}
			} else if (payload is RemotePlayerPayload) {
				if (connectionController.remoteStatus == RemoteStatus.EstablishedClient) {
					connectionController.client_ConnectedClients.Add (((RemotePlayerPayload)payload).remotePlayer);
					connectionController.Client_OtherPlayerJoined ();
				} else {
					P2pInterfaceController.Instance.WriteToConsole ("Unexpected player joined event outside of established client mode");
				}
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Unhandled message: " + payload);
			}
//			P2pInterfaceController.Instance.WriteToConsole ("Message received");
		}
		
		public void OnRemoteEndpointDisconnected (string remoteEndpointId)
		{
			if (listenerMode == ListenerMode.ListeningToHost) {
				//TODO Throw error
				P2pInterfaceController.Instance.ButtonHandler_DisconnectAndExit ();
			} else if (listenerMode == ListenerMode.ListeningToClients) {
				ConnectionController.Instance.Host_EchoMessageToOtherClients (remoteEndpointId, Utility.PayloadToByteArray (new PlayerLeftPayload (remoteEndpointId)), true);
				connectionController.Host_PlayerLeft (remoteEndpointId);

			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Error: Remote endpoint disconnected but listener is uninitialized!");
				return;
			}
			P2pInterfaceController.Instance.WriteToConsole ("Remote endpoint disconnected");
		}
	}

	#endregion

	#region GPG Online


	private void InitializeGpgInstance ()
	{
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder ()
		// enables saving game progress.
//				.EnableSavedGames()
		// registers a callback to handle game invitations received while the game is not running.
//					.WithInvitationDelegate (HandleInvitation)
		// registers a callback for turn based match notifications received while the
		// game is not running.
//					.WithMatchDelegate (HandleNotification)

					.Build ();
		PlayGamesPlatform.InitializeInstance (config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate ();
	}
	
	public void SignIn ()
	{
		try {
			// authenticate user:
			Social.localUser.Authenticate ((bool success) => {
				// handle success or failure
				if (success) {
					P2pInterfaceController.Instance.WriteToConsole ("Success!");
//				P2pInterfaceController.Instance.WriteToConsole("local user: " + Social.localUser.ToString());
//				P2pInterfaceController.Instance.WriteToConsole("user id: " + Social.localUser.id);
//				P2pInterfaceController.Instance.WriteToConsole("state:" + Social.localUser.state);
//				P2pInterfaceController.Instance.WriteToConsole ("Signed in with local user: " + Social.localUser.id + ", " + Social.localUser.state);
				} else {
					P2pInterfaceController.Instance.WriteToConsole ("GPG sign in failed for God knows why");
				}
			});
		} catch (Exception e) { 
			P2pInterfaceController.Instance.WriteToConsole ("Exception in SignIn: " + e.Message);
		}
	}

	public void CreateQuickGame ()
	{
		try {
			P2pInterfaceController.Instance.WriteToConsole("Beginning CreateQuickGame");
			if (remoteStatus != RemoteStatus.Idle) {
				P2pInterfaceController.Instance.WriteToConsole ("Error: Unable to create quick game from remote status: " + remoteStatus);
				return;
			}
//			P2pInterfaceController.Instance.WriteToConsole("connection controllers: " + this.ToString() + Instance.ToString());
			uint minPlayers = 1;
			uint maxPlayers = 1;
			uint variant = 0;
			P2pInterfaceController.Instance.WriteToConsole("Variables set");


//			P2pInterfaceController.Instance.WriteToConsole("realtime check: "+ PlayGamesPlatform.Instance.RealTime.GetSelf().DisplayName);
			PlayGamesPlatform.Instance.RealTime.CreateQuickGame (minPlayers, maxPlayers, variant, this);
			P2pInterfaceController.Instance.WriteToConsole ("Created quick game successfully");
			remoteStatus = RemoteStatus.StartingRoom;
			realtime_Participants = new List<RemotePlayer> ();
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in CreateQuickGame: " + e.Message);
		}
	}

	public void Realtime_StartGame (GameSettings gameSettings)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Sending start game payload to " + PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants ().Count + " clients");
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, Utility.PayloadToByteArray (new StartGamePayload (gameSettings)));
		P2pInterfaceController.Instance.WriteToConsole ("Start game payload sent");
	}

	#region RealTimeMultiplayerListener implementation

	public void OnRoomSetupProgress (float percent)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Received room setup progress: " + percent);
		P2pInterfaceController.Instance.Join_ReceivedRoomProgress (percent);
	}

	public void OnRoomConnected (bool success)
	{
		if (success) {
			P2pInterfaceController.Instance.WriteToConsole ("Room connected!");
			remoteStatus = RemoteStatus.EstablishedRealTimeMultiplayer;
			List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants ();
			Participant self = PlayGamesPlatform.Instance.RealTime.GetSelf ();
			RemotePlayer remoteSelf = null;
			foreach (Participant participant in participants) {
				RemotePlayer remotePlayer = new RemotePlayer (participant.ParticipantId);
				if (participant.ParticipantId == self.ParticipantId) {
					remotePlayer.profile = DeviceDatabase.Instance.ActiveProfile;
					remoteSelf = remotePlayer;
				}
				realtime_Participants.Add (remotePlayer);
			}
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, Utility.PayloadToByteArray (new RemotePlayerPayload (remoteSelf)));
			P2pInterfaceController.Instance.PlayersInLobby = AccessiblePlayers;
			P2pInterfaceController.Instance.SetScreenState (AppState.LobbyScreen);
		} else {
			P2pInterfaceController.Instance.WriteToConsole ("Room setup failed, boo"); //TODO Proper error dialogs
			TerminateAllConnections ();
			P2pInterfaceController.Instance.SetScreenState (AppState.TitleScreen);
		}

	}

	public void OnLeftRoom ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Leaving room");
		remoteStatus = RemoteStatus.Idle;

	}

	public void OnParticipantLeft (Participant participant) //TODO Abandon empty rooms?
	{
		P2pInterfaceController.Instance.WriteToConsole ("Participant left: " + participant.ParticipantId);
		try {
			RemotePlayer player = realtime_Participants.Where (remotePlayer => remotePlayer.gpgId == participant.ParticipantId).Single ();
			realtime_Participants.Remove (player);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in OnParticipantLeft: " + e.Message);
		}
	}

	public void OnPeersConnected (string[] participantIds)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Peers connected: " + participantIds.Length);
		foreach (string participantId in participantIds) {
			realtime_Participants.Add (new RemotePlayer (participantId));
			BroadcastEvent (new RemotePlayerPayload (new RemotePlayer (PlayGamesPlatform.Instance.RealTime.GetSelf ().ParticipantId, DeviceDatabase.Instance.ActiveProfile, true)));
			P2pInterfaceController.Instance.PlayersInLobby = AccessiblePlayers;
		}
	}

	public void OnPeersDisconnected (string[] participantIds) //TODO Leave empty room?
	{
		P2pInterfaceController.Instance.WriteToConsole ("Peers disconnected: " + participantIds.Length);
		try {
			foreach (string participantId in participantIds) {
				for (int i = 0; i < realtime_Participants.Count; i++) {
					if (realtime_Participants [i].gpgId == participantId) {
						realtime_Participants.RemoveAt (i);
						P2pInterfaceController.Instance.WriteToConsole ("Removed participant: " + participantId);
						break;
					}
				}
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in OnPeersConnected: " + e.Message);
		}
	}

	public void OnRealTimeMessageReceived (bool isReliable, string senderId, byte[] data)
	{
		try {
			P2pInterfaceController.Instance.WriteToConsole ("Realtime message received");
			Payload payload = Utility.ByteArrayToPayload (data);
			if (payload is RemotePlayerPayload) {
				P2pInterfaceController.Instance.WriteToConsole ("payload is remote player");
				RemotePlayer playerToUpdate = realtime_Participants.Single (remotePlayer => remotePlayer.gpgId == senderId);
				playerToUpdate.profile = ((RemotePlayerPayload)payload).remotePlayer.profile;
				P2pInterfaceController.Instance.PlayersInLobby = AccessiblePlayers;
				P2pInterfaceController.Instance.WriteToConsole ("Participant assigned online profile: " + senderId + ", " + playerToUpdate.profile.profileId);
			} else if (payload is StartGamePayload) {
				P2pGameMaster.Instance.LoadGameSettings (((StartGamePayload)payload).gameStartInfo);
				P2pInterfaceController.Instance.SetScreenState (AppState.GameScreen);
				P2pInterfaceController.Instance.WriteToConsole ("Start game message handled successfully");
			} else if (payload is GameResultPayload) {
				ReceiveGameResult (((GameResultPayload)payload).gameResult);
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Unhandled payload type in OnRealTimeMessageReceived: " + payload);
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in OnRealTimeMessageReceived: " + e.Message);
		}
	}

	#endregion

	
	
	//	private static void HandleInvitation (GooglePlayGames.BasicApi.Multiplayer.Invitation invitation, bool value)
	//	{
	//		print ("invitation received!");
	//	}
	
	//	private static void HandleNotification (GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch match, bool value)
	//	{
	//		print ("match notification received!");
	//	}
		#endregion

}
