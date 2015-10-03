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
		SetAppState (AppState.TitleScreen);
	}

	#region event handlers

	public void Host_CreateGame() {
		SetAppState (AppState.HostScreen);
	}

	public void Client_JoinGame() {
		SetAppState (AppState.JoinScreen);
	}

	public void Client_EnterLobby() {
		SetAppState (AppState.LobbyScreen);
	}

	public void Host_PlayerJoined() {
		P2pInterfaceController.Instance.WriteToConsole ("Player joined!");
	}

	public void ExitToTitle() {
		ConnectionController.Instance.StopAllConnections ();
		SetAppState (AppState.TitleScreen);
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
			ConnectionController.Instance.BeginAdvertising();
			break;
		case AppState.JoinScreen:
			ConnectionController.Instance.BeginDiscovery();
			break;
		}
	}
}
