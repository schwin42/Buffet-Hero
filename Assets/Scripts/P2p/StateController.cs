using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class StateController : MonoBehaviour
{

	private static StateController _instance;

	public static StateController Instance {
		get {
			if (_instance == null) {
				GameObject.FindObjectOfType<StateController> ();
			}
			return _instance;
		}
	}

	public enum AppState
	{
		Uninitialized = -1,
		TitleScreen = 0,
		WaitingScreen = 1,
		JoinScreen = 2,
		GameScreen = 3,
		ResultScreen = 4,
		LobbyScreen = 5,
	}

	//Configurable
	public const float TIME_LIMIT = 15f;

	string userText_WaitingForClients = "Waiting for clients...";
	string userText_WaitingForHost = "Waiting for host...";

	//FSM
	private AppState _currentState = AppState.Uninitialized;
	private Dictionary<AppState, GameObject> stateGoReference = new Dictionary<AppState, GameObject> ();
	public Transform inspector_ScreenContainer;

	//Players
	public List<RemotePlayer> host_ConnectedClients;
	[System.NonSerialized]
	public RemotePlayer client_ConnectedHost;
	private List<RemotePlayer> _client_ConnectedClients;
	public List<RemotePlayer> client_ConnectedClients {
		get {
//			P2pInterfaceController.Instance.WriteToConsole("getting connectedClients: " + Environment.StackTrace);
			P2pInterfaceController.Instance.WriteToConsole("connectedClients count = " + _client_ConnectedClients.Count);
			return _client_ConnectedClients;
		}
		set {
//			P2pInterfaceController.Instance.WriteToConsole("setting connectedClients from: " + Environment.StackTrace);
			P2pInterfaceController.Instance.WriteToConsole("connectedClients count = " + value.Count);
			_client_ConnectedClients = value;
		}
	}
	public List<RemotePlayer> AccessiblePlayers { //Other players in this session
		get {
			P2pInterfaceController.Instance.WriteToConsole ("Start of AccessiblePlayers getter");
			P2pInterfaceController.Instance.WriteToConsole ("AP remote status: " + ConnectionController.remoteStatus);
			if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
				ConnectionController.remoteStatus == ConnectionController.RemoteStatus.Advertising) {
				P2pInterfaceController.Instance.WriteToConsole ("AP as host");
				return StateController.Instance.host_ConnectedClients //Connected players
					.Union (new List<RemotePlayer> { new RemotePlayer (ConnectionController.localEndpointId, DeviceDatabase.activeProfile) }).ToList (); //Self
			} else if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedClient) {
				P2pInterfaceController.Instance.WriteToConsole ("Getting active players, connected clients: " + StateController.Instance.client_ConnectedClients.Count);
				List<RemotePlayer> output = StateController.Instance.client_ConnectedClients //Other connected clients
					.Union (new List<RemotePlayer> { StateController.Instance.client_ConnectedHost }) //Host
						.Union (new List<RemotePlayer> { new RemotePlayer(ConnectionController.localEndpointId, DeviceDatabase.activeProfile) }).ToList (); //Self
				return output;
			} else {
				P2pInterfaceController.Instance.WriteToConsole ("No accessible players in unestablished remote state: " + ConnectionController.remoteStatus);
				return new List<RemotePlayer> { new RemotePlayer(ConnectionController.localEndpointId, DeviceDatabase.activeProfile) }; //Self
			}
		}
	} 


	void Awake ()
	{
		_instance = this;
	}

	// Use this for initialization
	void Start ()
	{
	
		//Initialize screen dictionary
		for (int i = 0; i < Enum.GetValues(typeof(AppState)).Length - 1; i++) {
			AppState appState = (AppState)i;
			GameObject screenGo = inspector_ScreenContainer.Find (appState.ToString ()).gameObject;
			stateGoReference.Add (appState, screenGo);
		}

		//Display first screen
		SetScreenState (AppState.TitleScreen);

		//Debug
//		P2pInterfaceController.Instance.Results_Display ();
//		SetScreenState (AppState.ResultScreen);
	}

	# region local event handlers

	//Buttons
	public void Host_CreateGame ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Creating game");
		try {
			DeviceDatabase.activeProfile.playerName = P2pInterfaceController.Instance.Title_SubmitProfileName ();
			P2pInterfaceController.Instance.WriteToConsole ("Wrote " + DeviceDatabase.activeProfile.playerName + " to active profile");
			ConnectionController.Instance.Host_BeginAdvertising ();
			SetScreenState (AppState.LobbyScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception: " + e.Message);
		}
	}

	public void Host_StartGame ()
	{
		ConnectionController.Instance.Host_BeginSession (); //Stop advertising and update remote status

		GameSettings gameSettings = new GameSettings (TIME_LIMIT, FoodLogic.NextUnityRandomSeed, FoodLogic.NextUnityRandomSeed, FoodLogic.NextUnityRandomSeed); //Bundle and send game settings to clients
		P2pGameMaster.Instance.LoadGameSettings (gameSettings);
		ConnectionController.Instance.BroadcastEvent (new StartGamePayload (gameSettings));
		//TODO Check if event is successful
		SetScreenState (AppState.GameScreen);
	}

//	public void Host_ReceiveGameResult() {
//		//Echo game result to all other clients
//	}

	public void Client_JoinGame ()
	{
		DeviceDatabase.activeProfile.playerName = P2pInterfaceController.Instance.Title_SubmitProfileName ();
		SetScreenState (AppState.JoinScreen);
	}

	public void Client_StartGame (GameSettings gameStartInfo)
	{
		ConnectionController.remoteStatus = ConnectionController.RemoteStatus.EstablishedClient;
		P2pGameMaster.Instance.LoadGameSettings (gameStartInfo);
		SetScreenState (AppState.GameScreen);
	}

	public void DisplayResult ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Beginning display result, remote status: " + ConnectionController.remoteStatus);
		if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
			ConnectionController.Instance.BroadcastEvent (new DisplayResultsEvent ());
		}

		P2pInterfaceController.Instance.WriteToConsole ("Displaying result");
		SetScreenState (AppState.ResultScreen);
	}

	public void ExitToTitle ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Exiting to title");
		try {
		ConnectionController.Instance.TerminateAllConnections ();
		SetScreenState (AppState.TitleScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole("Exception in ExitToTitle: " + e.Message);
		}
	}

	//Triggered
	public void GameFinished (GameResult gameResult)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Game finished");
		SetScreenState (AppState.WaitingScreen);
		ConnectionController.Instance.BroadcastEvent (new GameResultPayload (gameResult));

	}
	#endregion

	#region remote event handlers


	public void Client_EnterLobby ()
	{
		SetScreenState (AppState.LobbyScreen);
	}

	public void Host_PlayerJoined (RemotePlayer player)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Player joined!");
		host_ConnectedClients.Add (player);
		P2pInterfaceController.Instance.Lobby_JoinedPlayers = AccessiblePlayers;
		P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive (true);
	}

	public void Host_PlayerLeft (string remoteEndpointId)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Player left");
		try {
			RemotePlayer playerToRemove = host_ConnectedClients.Single (rp => rp.remoteEndpointId == remoteEndpointId);
			host_ConnectedClients.Remove (playerToRemove);
			P2pInterfaceController.Instance.Lobby_JoinedPlayers = AccessiblePlayers;
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
		P2pGameMaster.Instance.otherGameResults.Add (gameResult);

		//Broadcast display result event if all games have been received
		if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
			if (P2pGameMaster.Instance.otherGameResults.Count == host_ConnectedClients.Count) {
				DisplayResult ();
			}
		}
	}

	#endregion

	private void SetScreenState (AppState targetState)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Setting screen state to " + targetState + " from " + _currentState);
		try {
			if (_currentState != AppState.Uninitialized) {
				//Disable last state and clean up
				stateGoReference [_currentState].SetActive (false);
			} else {
				//Disable all screens before initialization
				foreach (KeyValuePair<AppState, GameObject> pair in stateGoReference) {
					pair.Value.SetActive (false);
				}
			}
			_currentState = targetState;
			InitializeState (_currentState);
			stateGoReference [_currentState].SetActive (true);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in SetScreenState: " + e.Message + ", " + e.StackTrace);
		}
	}

	private void InitializeState (AppState state)
	{
		P2pInterfaceController.Instance.WriteToConsole ("initializing " + state);
		try {
			switch (state) {
			case AppState.LobbyScreen:
				P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive( (host_ConnectedClients != null && host_ConnectedClients.Count > 0));

				//Set status
				string status = "Initializing status";
				if(ConnectionController.remoteStatus == ConnectionController.RemoteStatus.Advertising) { 
					status = userText_WaitingForClients;
				} else if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedClient) {
					status = userText_WaitingForHost;
				} else {
					P2pInterfaceController.Instance.WriteToConsole("Unexpected remote state: " + ConnectionController.remoteStatus);
					status = "Initialization failed";
				}
				P2pInterfaceController.Instance.Lobby_Status = status;

				P2pInterfaceController.Instance.Lobby_JoinedPlayers = AccessiblePlayers;
				P2pInterfaceController.Instance.WriteToConsole ("completed lobby screen");
				break;
			case AppState.JoinScreen:
				ConnectionController.Instance.Client_BeginDiscovery ();
				P2pInterfaceController.Instance.WriteToConsole ("completed join screen");
				break;
			case AppState.GameScreen:
				P2pGameMaster.Instance.BeginNewGame ();
				P2pInterfaceController.Instance.WriteToConsole ("completed game screen");
				break;
			case AppState.ResultScreen:
				P2pInterfaceController.Instance.Result_SetPlayButtonInteractive (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost);
				P2pInterfaceController.Instance.Results_Display ();
				P2pInterfaceController.Instance.WriteToConsole ("completed result screen");
				break;
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Shit, initialize state failed." + e.Message);
		}
	}
}
