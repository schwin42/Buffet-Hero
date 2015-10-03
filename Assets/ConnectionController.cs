using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using GooglePlayGames.BasicApi.Nearby;

public class ConnectionController : MonoBehaviour {

	private static ConnectionController _instance;
	public static ConnectionController Instance {
		get {
			if(_instance == null) {
				GameObject.FindObjectOfType<ConnectionController>();
			}
			return _instance;
		}
	}

	public enum RemoteStatus {
		Uninitialized = -1,
		Idle = 0,
		Advertising = 1,
		Discovering = 2,
		ConnectedToHost = 3,
		ConnectedToClient = 4,
	}

	public static string serviceId;

	protected static RemoteStatus _remoteStatus = RemoteStatus.Uninitialized;

	DiscoveryListener discoveryListener;
	public MessageListener messageListener;

	private P2pGameMaster _gameMaster;

	public void Awake() {
		_instance = this;
	}

	public void Start() {

		_gameMaster = GetComponent<P2pGameMaster> ();

		InitializeGpgNearby ();

		serviceId = PlayGamesPlatform.Nearby.GetServiceId ();

		_remoteStatus = RemoteStatus.Idle;
	}

	#region nearby generic

	private void InitializeGpgNearby() {
		PlayGamesPlatform.InitializeNearby((client) => {
			P2pInterfaceController.Instance.WriteToConsole("Initiated nearby with: " + client);
		});
	}

	public void StopAllConnections() {
		PlayGamesPlatform.Nearby.StopAllConnections ();
		P2pInterfaceController.Instance.WriteToConsole ("All connections stopped");
	}

	#endregion

	# region nearby host

	public void BeginAdvertising() {
		P2pInterfaceController.Instance.WriteToConsole ("beginning advertising @" + DateTime.Now);
		List<string> appIdentifiers = new List<string> ();
		appIdentifiers.Add (PlayGamesPlatform.Nearby.GetAppBundleId ());
		PlayGamesPlatform.Nearby.StartAdvertising (
			"First pancake",
			appIdentifiers,
			TimeSpan.FromSeconds (0),
			(AdvertisingResult result) => {
			P2pInterfaceController.Instance.WriteToConsole ("on advertising result: " + result);
		},
		HandleConnectionRequest);

		_remoteStatus = RemoteStatus.Advertising;
	}

	public void HandleConnectionRequest (ConnectionRequest request) {
		P2pInterfaceController.Instance.WriteToConsole ("Received connection request: " +
		                                                request.RemoteEndpoint.DeviceId + " " +
		                                                request.RemoteEndpoint.EndpointId + " " +
		                                                request.RemoteEndpoint.Name);

		messageListener = new MessageListener ();

		PlayGamesPlatform.Nearby.AcceptConnectionRequest (
			request.RemoteEndpoint.EndpointId,
			new byte[1],
			messageListener);
		StateController.Instance.Host_PlayerJoined ();
	}

	#endregion

	#region nearby client

	public void BeginDiscovery() {
		P2pInterfaceController.Instance.WriteToConsole ("Beginning discovery");
		discoveryListener = new DiscoveryListener ();

		PlayGamesPlatform.Nearby.StartDiscovery (
			serviceId,
			TimeSpan.FromSeconds (0),
			discoveryListener);
		P2pInterfaceController.Instance.WriteToConsole ("Discovery in progress");
	}

	#endregion

	#region online multiplayer

	private void InitializeGpgInstance() {
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			// enables saving game progress.
			.EnableSavedGames()
				// registers a callback to handle game invitations received while the game is not running.
				.WithInvitationDelegate(HandleInvitation)
				// registers a callback for turn based match notifications received while the
				// game is not running.
				.WithMatchDelegate(HandleNotification)
				.Build();
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
	}

	private void SignIn() {
		// authenticate user:
		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			if(success) {
				P2pInterfaceController.Instance.WriteToConsole("success!");
				_gameMaster.BeginNewGame();
			} else {
				P2pInterfaceController.Instance.WriteToConsole("failure!");
			}
		});
	}

	private static void HandleInvitation(GooglePlayGames.BasicApi.Multiplayer.Invitation invitation, bool value) {
		print ("invitation received!");
	}

	private static void HandleNotification(GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch match, bool value) {
		print ("match notification received!");
	}

	#endregion

	public class DiscoveryListener : IDiscoveryListener {
		
		public void OnEndpointFound (EndpointDetails discoveredEndpoint)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Endpoint found, sending connection request @" + DateTime.Now);
			
			ConnectionController.Instance.messageListener = new MessageListener ();
			
			PlayGamesPlatform.Nearby.SendConnectionRequest (
				"Marco Polo",
				discoveredEndpoint.EndpointId,
				new byte[1],
				HandleConnectionResponse,
				(IMessageListener)ConnectionController.Instance.messageListener);
			P2pInterfaceController.Instance.WriteToConsole ("Connection request sent to endpoint");
			
		}
		
		public void OnEndpointLost (string lostEndpointId)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Endpoint lost");
		}
		
		public void HandleConnectionResponse(ConnectionResponse response) {
			if (response.ResponseStatus == ConnectionResponse.Status.Accepted) {
				P2pInterfaceController.Instance.WriteToConsole ("Connection successful!");
				PlayGamesPlatform.Nearby.StopDiscovery (ConnectionController.serviceId);
				StateController.Instance.Client_EnterLobby ();
				_remoteStatus = RemoteStatus.ConnectedToHost;
			} else {	
				P2pInterfaceController.Instance.WriteToConsole ("Connection failed: " + response.ResponseStatus);
			}
		}
	}
	
	public class MessageListener : IMessageListener {
		
		public void OnMessageReceived (string remoteEndpointId, byte[] data, bool isReliableMessage)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Message received");
		}
		
		public void OnRemoteEndpointDisconnected (string remoteEndpointId)
		{
			P2pInterfaceController.Instance.WriteToConsole ("Remote endpoint disconnected");
		}
}


}
