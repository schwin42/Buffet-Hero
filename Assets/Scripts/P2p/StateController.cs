using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
	}

	private AppState _currentState = AppState.Uninitialized;

	private Dictionary<AppState, GameObject> stateGoReference = new Dictionary<AppState, GameObject>();

	public Transform inspector_ScreenContainer;

	public Profile playerProfile;

	public List<RemotePlayer> connectedPlayers = new List<RemotePlayer> ();

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

		//Load profile
		playerProfile = new Profile("TestProfile" + Time.time);

		//Display first screen
		SetAppState (AppState.TitleScreen);
	}

	# region local event handlers

	//Buttons
	public void Host_CreateGame() {
		SetAppState (AppState.HostScreen);
	}

	public void Host_StartGame() {
		ConnectionController.Instance.Host_BeginSession ();
		ConnectionController.Instance.BroadcastEvent (new StartGamePayload());
		//TODO Check if event is successful
		SetAppState (AppState.GameScreen);
	}
	
	public void Client_JoinGame() {
		SetAppState (AppState.JoinScreen);
	}

	public void Client_StartGame ()
	{
		SetAppState (AppState.GameScreen);
	}

	public void ExitToTitle() {
		ConnectionController.Instance.StopAllConnections ();
		SetAppState (AppState.TitleScreen);
	}

	//Triggered
	public void GameFinished (GameResult gameResult)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Game finished");
		SetAppState (AppState.ResultScreen);
		ConnectionController.Instance.BroadcastEvent (new GameFinishedPayload(gameResult));

	}
	#endregion

	#region event handlers


	public void Client_EnterLobby() {
		SetAppState (AppState.LobbyScreen);
	}

	public void Host_PlayerJoined(RemotePlayer player) {
		P2pInterfaceController.Instance.WriteToConsole ("Player joined!");
		connectedPlayers.Add (player);
		P2pInterfaceController.Instance.Host_JoinedPlayers = connectedPlayers;
		P2pInterfaceController.Instance.Host_SetStartButtonInteractive (true);
	}

	public void Host_PlayerLeft(RemotePlayer player) {
		P2pInterfaceController.Instance.WriteToConsole ("Player left");
		connectedPlayers.Remove (player);
		if (connectedPlayers.Count == 0) {
			P2pInterfaceController.Instance.Host_SetStartButtonInteractive (false);
		}
	}

	#endregion

	private void SetAppState(AppState targetState) {
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
			ConnectionController.Instance.Host_BeginAdvertising();
			break;
		case AppState.JoinScreen:
			ConnectionController.Instance.Client_BeginDiscovery();
			break;
		case AppState.GameScreen:
			P2pGameMaster.Instance.BeginNewGame();
			break;
		}
	}
}
