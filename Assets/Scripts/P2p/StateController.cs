using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class StateController : MonoBehaviour {

	private static StateController _instance;
	public static StateController Instance {
		get {
			if(_instance == null) {
				GameObject.FindObjectOfType<StateController>();
			}
			return _instance;
		}
	}

	public enum AppState {
		Uninitialized = -1,
		TitleScreen = 0,
		HostScreen = 1,
		JoinScreen = 2,
		GameScreen = 3,
		ResultScreen = 4,
		LobbyScreen = 5,
		WaitingScreen = 6
	}

	private AppState _currentState = AppState.Uninitialized;

	private Dictionary<AppState, GameObject> stateGoReference = new Dictionary<AppState, GameObject>();

	public Transform inspector_ScreenContainer;

	public List<RemotePlayer> connectedClients;
	[System.NonSerialized] public RemotePlayer connectedHost;

	void Awake () {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
	
		//Initialize screen dictionary
		for(int i = 0; i < Enum.GetValues(typeof(AppState)).Length - 1; i++) {
			AppState appState = (AppState) i;
			GameObject screenGo = inspector_ScreenContainer.Find(appState.ToString()).gameObject;
			stateGoReference.Add(appState, screenGo);
		}

		//Display first screen
		SetScreenState (AppState.TitleScreen);

		//Debug
//		P2pInterfaceController.Instance.Results_Display ();
//		SetScreenState (AppState.ResultScreen);
	}

	# region local event handlers

	//Buttons
	public void Host_CreateGame() {
		SetScreenState (AppState.HostScreen);
	}

	public void Host_StartGame() {
		ConnectionController.Instance.Host_BeginSession (); //Stop advertising and update remote status
		ConnectionController.Instance.BroadcastEvent (new StartGameEvent());
		//TODO Check if event is successful
		SetScreenState (AppState.GameScreen);
	}

//	public void Host_ReceiveGameResult() {
//		//Echo game result to all other clients
//	}

	public void Client_JoinGame() {
		SetScreenState (AppState.JoinScreen);
	}

	public void Client_StartGame ()
	{
		ConnectionController.remoteStatus = ConnectionController.RemoteStatus.EstablishedClient;
		SetScreenState (AppState.GameScreen);
	}

	public void DisplayResult () {
		P2pInterfaceController.Instance.WriteToConsole ("Beginning display result, remote status: " + ConnectionController.remoteStatus);
		if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
			ConnectionController.Instance.BroadcastEvent(new DisplayResultsEvent());
		}

		P2pInterfaceController.Instance.WriteToConsole ("Displaying result");
		SetScreenState (AppState.ResultScreen);
		P2pInterfaceController.Instance.WriteToConsole ("Succesffully displayed result.");
	}

	public void ExitToTitle() {
		ConnectionController.Instance.StopAllConnections ();
		SetScreenState (AppState.TitleScreen);
	}

	//Triggered
	public void GameFinished (GameResult gameResult)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Game finished");
		SetScreenState (AppState.WaitingScreen);
		ConnectionController.Instance.BroadcastEvent (new GameResultPayload(gameResult));

	}
	#endregion

	#region remote event handlers


	public void Client_EnterLobby() {
		SetScreenState (AppState.LobbyScreen);
	}

	public void Host_PlayerJoined(RemotePlayer player) {
		P2pInterfaceController.Instance.WriteToConsole ("Player joined!");
		connectedClients.Add (player);
		P2pInterfaceController.Instance.Host_JoinedPlayers = connectedClients;
		P2pInterfaceController.Instance.Host_SetStartButtonInteractive (true);
	}

	public void Host_PlayerLeft(string remoteEndpointId) {
		P2pInterfaceController.Instance.WriteToConsole ("Player left");

		try {
			RemotePlayer playerToRemove = connectedClients.Single(rp => rp.remoteEndpointId == remoteEndpointId);
			connectedClients.Remove (playerToRemove);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole("Error in Host_PlayerLeft: " + e);
		}

		if (connectedClients.Count == 0) {
			P2pInterfaceController.Instance.Host_SetStartButtonInteractive (false);
			P2pInterfaceController.Instance.Result_SetPlayButtonInteractive (false);

		}
	}

	public void ReceiveGameResult (GameResult gameResult)
	{
		P2pGameMaster.Instance.otherGameResults.Add (gameResult);

		//Broadcast display result event if all games have been received
		if (ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
			if (P2pGameMaster.Instance.otherGameResults.Count == connectedClients.Count) {
				DisplayResult();
			}
		}
	}

	#endregion

	private void SetScreenState(AppState targetState) {
P2pInterfaceController.Instance.WriteToConsole("Setting screen state to " + targetState + " from " + _currentState);
		try {
		if (_currentState != AppState.Uninitialized) {
			//Disable last state and clean up
			stateGoReference [_currentState].SetActive (false);
			TerminateState (_currentState);
		} else {
			//Disable all screens before initialization
			foreach(KeyValuePair<AppState, GameObject> pair in stateGoReference) {
				pair.Value.SetActive(false);
			}
		}
		_currentState = targetState;
		InitializeState (_currentState);
		stateGoReference[_currentState].SetActive(true);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole("Exception in SetScreenState: " + e.Message);
		}
	}

	private void TerminateState(AppState state) {
		switch (state) {
		case AppState.HostScreen:
			
			break;
		}
	}

	private void InitializeState(AppState state) {
		switch (state) {
		case AppState.HostScreen:
			P2pInterfaceController.Instance.Host_SetStartButtonInteractive(false);
			ConnectionController.Instance.Host_BeginAdvertising();
			break;
		case AppState.JoinScreen:
			ConnectionController.Instance.Client_BeginDiscovery();
			break;
		case AppState.GameScreen:
			P2pGameMaster.Instance.BeginNewGame();
			break;
		case AppState.ResultScreen:
			P2pInterfaceController.Instance.Result_SetPlayButtonInteractive(ConnectionController.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost);
			P2pInterfaceController.Instance.Results_Display();
			break;
		}
	}
}
