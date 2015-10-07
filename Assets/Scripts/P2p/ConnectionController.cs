using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using GooglePlayGames.BasicApi.Nearby;
using System.Linq;

public class ConnectionController : MonoBehaviour
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
	}

	public static string serviceId;
	public static RemoteStatus remoteStatus = RemoteStatus.Uninitialized;
	DiscoveryListener discoveryListener;
	public MessageListener messageListener;

	public void Awake ()
	{
		_instance = this;
	}

	public void Start ()
	{
		InitializeGpgNearby ();

		serviceId = PlayGamesPlatform.Nearby.GetServiceId ();

		remoteStatus = RemoteStatus.Idle;
	}

	#region nearby generic

	private void InitializeGpgNearby ()
	{
		PlayGamesPlatform.InitializeNearby ((client) => {
			P2pInterfaceController.Instance.WriteToConsole ("Initiated GpgNearby." + client);
		});
	}

	public void TerminateAllConnections ()
	{
		PlayGamesPlatform.Nearby.StopAllConnections ();
		remoteStatus = RemoteStatus.Idle;
		discoveryListener = null;
		messageListener = null;
		StateController.Instance.client_ConnectedHost = null;
		StateController.Instance.host_ConnectedClients = null;
		StateController.Instance.client_ConnectedClients = null;
		P2pInterfaceController.Instance.WriteToConsole ("All connections stopped");
	}

	public void BroadcastEvent (Payload payload)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Broadcasting event");
		try {
			if (remoteStatus == RemoteStatus.EstablishedClient) {
				if(StateController.Instance.client_ConnectedHost == null) {
					P2pInterfaceController.Instance.WriteToConsole ("Failed to broadcast event " + payload + " because host is null");
					return;
				}
				PlayGamesPlatform.Nearby.SendReliable (new List<string> { StateController.Instance.client_ConnectedHost.remoteEndpointId }, 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to host");
			} else if (remoteStatus == RemoteStatus.EstablishedHost) {
				if(StateController.Instance.host_ConnectedClients == null || StateController.Instance.host_ConnectedClients.Count == 0) {
					P2pInterfaceController.Instance.WriteToConsole ("Failed to broadcast event " + payload + " because clients list is empty");
					return;
				}
				PlayGamesPlatform.Nearby.SendReliable (StateController.Instance.host_ConnectedClients.Select (p => p.remoteEndpointId).ToList (), 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to " + StateController.Instance.host_ConnectedClients.Count + " clients");
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
		StateController.Instance.host_ConnectedClients = new List<RemotePlayer> ();
		List<string> appIdentifiers = new List<string> ();
		appIdentifiers.Add (PlayGamesPlatform.Nearby.GetAppBundleId ());
		PlayGamesPlatform.Nearby.StartAdvertising (
			"First pancake",
			appIdentifiers,
			TimeSpan.FromSeconds (0),
			(AdvertisingResult result) => {
			P2pInterfaceController.Instance.WriteToConsole ("advertising result: " + result.Status + ", " + result.Succeeded);
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

		messageListener = new MessageListener (MessageListener.ListenerMode.ListeningToClients);

		try {
			PlayGamesPlatform.Nearby.AcceptConnectionRequest (
				request.RemoteEndpoint.EndpointId,
				Utility.PayloadToByteArray(new WelcomePayload(new RemotePlayer(PlayGamesPlatform.Nearby.LocalEndpointId(), DeviceDatabase.activeProfile), StateController.Instance.client_ConnectedClients)),
				messageListener);

			RemotePlayer remotePlayer = ((PlayerJoinedPayload)Utility.ByteArrayToPayload (request.Payload)).remotePlayer; //TODO better type validation/ error checking
			P2pInterfaceController.Instance.WriteToConsole ("Accepted connection request from " + request.RemoteEndpoint.EndpointId);
			StateController.Instance.Host_PlayerJoined (remotePlayer);
			Host_EchoMessageToOtherClients(request.RemoteEndpoint.EndpointId, request.Payload, true);
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

	private void Host_EchoMessageToOtherClients (string sourceEndpointId, byte[] data, bool isReliableMessage) {
		//Send to every client but the source
		P2pInterfaceController.Instance.WriteToConsole ("Host beginning echo procedure");
	
		List<string> endpointsToMessage = StateController.Instance.host_ConnectedClients.Where (rp => rp.remoteEndpointId != sourceEndpointId).Select(rp => rp.remoteEndpointId).ToList();

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
		StateController.Instance.client_ConnectedHost = null;
		StateController.Instance.client_ConnectedClients = new List<RemotePlayer> ();
		discoveryListener = new DiscoveryListener ();

		PlayGamesPlatform.Nearby.StartDiscovery (
			serviceId,
			TimeSpan.FromSeconds (0),
			discoveryListener);

		P2pInterfaceController.Instance.WriteToConsole ("Discovery in progress");
	}

	#endregion

//	#region online multiplayer
//
//	private void InitializeGpgInstance() {
//		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
//			// enables saving game progress.
//			.EnableSavedGames()
//				// registers a callback to handle game invitations received while the game is not running.
//				.WithInvitationDelegate(HandleInvitation)
//				// registers a callback for turn based match notifications received while the
//				// game is not running.
//				.WithMatchDelegate(HandleNotification)
//				.Build();
//		PlayGamesPlatform.InitializeInstance(config);
//		// recommended for debugging:
//		PlayGamesPlatform.DebugLogEnabled = true;
//		// Activate the Google Play Games platform
//		PlayGamesPlatform.Activate();
//	}
//
//	private void SignIn() {
//		// authenticate user:
//		Social.localUser.Authenticate((bool success) => {
//			// handle success or failure
//			if(success) {
//				P2pInterfaceController.Instance.WriteToConsole("success!");
//				_gameMaster.BeginNewGame();
//			} else {
//				P2pInterfaceController.Instance.WriteToConsole("failure!");
//			}
//		});
//	}
//
//	private static void HandleInvitation(GooglePlayGames.BasicApi.Multiplayer.Invitation invitation, bool value) {
//		print ("invitation received!");
//	}
//
//	private static void HandleNotification(GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch match, bool value) {
//		print ("match notification received!");
//	}
//
//	#endregion

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
				ConnectionController.Instance.messageListener = new MessageListener (MessageListener.ListenerMode.ListeningToHost);
				PlayGamesPlatform.Nearby.SendConnectionRequest (
				"Marco Polo",
				discoveredEndpoint.EndpointId,
				Utility.PayloadToByteArray (new PlayerJoinedPayload (new RemotePlayer(PlayGamesPlatform.Nearby.LocalEndpointId (), DeviceDatabase.activeProfile))),
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
				remoteStatus = RemoteStatus.EstablishedClient;
			} else {	
				P2pInterfaceController.Instance.WriteToConsole ("Connection failed: " + response.ResponseStatus);
			}
		}

		private void Client_HandleSuccessfulConnectionToHost (ConnectionResponse response)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Connection successful!");
			PlayGamesPlatform.Nearby.StopDiscovery (ConnectionController.serviceId);
			StateController.Instance.Client_EnterLobby ();
			WelcomePayload welcomePayload = (WelcomePayload)Utility.ByteArrayToPayload (response.Payload);
			StateController.Instance.client_ConnectedHost = welcomePayload.hostPlayer;
			StateController.Instance.client_ConnectedClients = welcomePayload.fellowClients;
		}
	}
	
	public class MessageListener : IMessageListener
	{

		public enum ListenerMode
		{
			Inactive = -1,
			ListeningToHost = 0,
			ListeningToClients = 1,
		}

		public ListenerMode listenerMode = ListenerMode.Inactive;

		public MessageListener (ListenerMode listenerMode) {
			this.listenerMode = listenerMode;
		}

		public void OnMessageReceived (string remoteEndpointId, byte[] data, bool isReliableMessage)
		{
			Payload payload = Utility.ByteArrayToPayload (data);
			P2pInterfaceController.Instance.WriteToConsole ("Received message: " + payload);
			if (payload is StartGamePayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Received start game event");
				StateController.Instance.Client_StartGame (((StartGamePayload)payload).gameStartInfo);
			} else if (payload is GameResultPayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Received game result");
				StateController.Instance.ReceiveGameResult (((GameResultPayload)payload).gameResult);
				//If host, echo result to all other clients
				if (listenerMode == ListenerMode.ListeningToClients) {
					ConnectionController.Instance.Host_EchoMessageToOtherClients (remoteEndpointId, data, isReliableMessage);
				}
			} else if (payload is DisplayResultsEvent) {
				P2pInterfaceController.Instance.WriteToConsole ("Received display result event");
				StateController.Instance.DisplayResult ();
			} else if (payload is PlayerLeftPayload) {
				P2pInterfaceController.Instance.WriteToConsole ("Recieved player left payload");
				if (ConnectionController.remoteStatus == RemoteStatus.EstablishedClient) {
					string droppedEndpointId = ((PlayerLeftPayload)payload).droppedEndpointId;
					StateController.Instance.client_ConnectedClients.RemoveAll (rp => rp.remoteEndpointId == droppedEndpointId);
				} else {
					P2pInterfaceController.Instance.WriteToConsole ("Unexpected player left event outside of established client mode");
				}
			} else if (payload is PlayerJoinedPayload) {
				if (ConnectionController.remoteStatus == RemoteStatus.EstablishedClient) {
					StateController.Instance.client_ConnectedClients.Add(((PlayerJoinedPayload)payload).remotePlayer);
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
				StateController.Instance.ExitToTitle();
			} else if (listenerMode == ListenerMode.ListeningToClients) {
				ConnectionController.Instance.Host_EchoMessageToOtherClients(remoteEndpointId, Utility.PayloadToByteArray(new PlayerLeftPayload(remoteEndpointId)), true);
				StateController.Instance.Host_PlayerLeft(remoteEndpointId);

			} else {
				P2pInterfaceController.Instance.WriteToConsole("Error: Remote endpoint disconnected but listener is uninitialized!");
				return;
			}
			P2pInterfaceController.Instance.WriteToConsole ("Remote endpoint disconnected");
		}
	}


}
