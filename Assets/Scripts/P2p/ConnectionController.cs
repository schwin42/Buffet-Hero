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
	protected static RemoteStatus _remoteStatus = RemoteStatus.Uninitialized;
	DiscoveryListener discoveryListener;
	public MessageListener messageListener;
	private P2pGameMaster _gameMaster;

	public void Awake ()
	{
		_instance = this;
	}

	public void Start ()
	{

		_gameMaster = GetComponent<P2pGameMaster> ();

		InitializeGpgNearby ();

		serviceId = PlayGamesPlatform.Nearby.GetServiceId ();

		_remoteStatus = RemoteStatus.Idle;
	}

	#region nearby generic

	private void InitializeGpgNearby ()
	{
		PlayGamesPlatform.InitializeNearby ((client) => {
			P2pInterfaceController.Instance.WriteToConsole ("Initiated nearby with: " + client);
		});
	}

	public void StopAllConnections ()
	{
		PlayGamesPlatform.Nearby.StopAllConnections ();
		_remoteStatus = RemoteStatus.Idle;
		P2pInterfaceController.Instance.WriteToConsole ("All connections stopped");
	}

	public void BroadcastEvent (Payload payload)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Broadcasting event");
		try {
			if (_remoteStatus == RemoteStatus.EstablishedClient) {
				PlayGamesPlatform.Nearby.SendReliable (StateController.Instance.connectedPlayers.Select (p => p.remoteEndpointId).ToList (), 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to " + StateController.Instance.connectedPlayers.Count + " host");
			} else if (_remoteStatus == RemoteStatus.EstablishedHost) {
				PlayGamesPlatform.Nearby.SendReliable (StateController.Instance.connectedPlayers.Select (p => p.remoteEndpointId).ToList (), 
			                                      Utility.PayloadToByteArray (payload));
				P2pInterfaceController.Instance.WriteToConsole ("Broadcast " + payload + " to " + StateController.Instance.connectedPlayers.Count + " clients");
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Failed to send message while in " + _remoteStatus);
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

		_remoteStatus = RemoteStatus.Advertising;
	}

	public void Host_HandleConnectionRequest (ConnectionRequest request)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Received connection request: " +
			request.RemoteEndpoint.DeviceId + " " +
			request.RemoteEndpoint.EndpointId + " " +
			request.RemoteEndpoint.Name);

		messageListener = new MessageListener ();

		try {
			PlayGamesPlatform.Nearby.AcceptConnectionRequest (
			request.RemoteEndpoint.EndpointId,
			new byte[1], //TODO Host and other clients' profiles
			messageListener);
			RemotePlayer remotePlayer = new RemotePlayer (request.RemoteEndpoint.EndpointId, ((ProfilePayload)Utility.ByteArrayToPayload (request.Payload)).profile); //TODO better type validation/ error checking
			P2pInterfaceController.Instance.WriteToConsole ("Accepted connection request from " + request.RemoteEndpoint.EndpointId);
			StateController.Instance.Host_PlayerJoined (remotePlayer);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole (e.Message);
		}
	}

	public void Host_BeginSession ()
	{
		PlayGamesPlatform.Nearby.StopAdvertising ();
		_remoteStatus = RemoteStatus.EstablishedHost;
		P2pInterfaceController.Instance.WriteToConsole ("Session started");
	}	

	#endregion

	#region nearby client

	public void Client_BeginDiscovery ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Beginning discovery");
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
			ConnectionController.Instance.messageListener = new MessageListener ();
			try {
				PlayGamesPlatform.Nearby.SendConnectionRequest (
				"Marco Polo",
				discoveredEndpoint.EndpointId,
				Utility.PayloadToByteArray (new ProfilePayload (StateController.Instance.playerProfile)),
				Client_HandleConnectionResponse,
				(IMessageListener)ConnectionController.Instance.messageListener);
				P2pInterfaceController.Instance.WriteToConsole ("Connection request sent to endpoint");
			} catch (Exception e) {
				P2pInterfaceController.Instance.WriteToConsole ("Error: " + e.Message);
			}

		}

		public void Client_HandleConnectionResponse (ConnectionResponse response)
		{
			if (response.ResponseStatus == ConnectionResponse.Status.Accepted) {
				Client_HandleSuccessfulConnectionToHost (response);
				_remoteStatus = RemoteStatus.EstablishedClient;
			} else {	
				P2pInterfaceController.Instance.WriteToConsole ("Connection failed: " + response.ResponseStatus);
			}
		}

		private void Client_HandleSuccessfulConnectionToHost (ConnectionResponse response)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Connection successful!");
			PlayGamesPlatform.Nearby.StopDiscovery (ConnectionController.serviceId);
			StateController.Instance.Client_EnterLobby ();
			StateController.Instance.connectedPlayers.Add (new RemotePlayer (response.RemoteEndpointId, ((ProfilePayload)Utility.ByteArrayToPayload (response.Payload)).profile));
		}
	}
	
	public class MessageListener : IMessageListener
	{
		
		public void OnMessageReceived (string remoteEndpointId, byte[] data, bool isReliableMessage)
		{
			Payload payload = Utility.ByteArrayToPayload (data);
			if (payload is StartGamePayload) {
				P2pInterfaceController.Instance.WriteToConsole("Received start game event");
				StateController.Instance.Client_StartGame();
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("Unhandled message: " + payload);
			}
//			P2pInterfaceController.Instance.WriteToConsole ("Message received");
		}
		
		public void OnRemoteEndpointDisconnected (string remoteEndpointId)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Remote endpoint disconnected");
		}
	}


}
