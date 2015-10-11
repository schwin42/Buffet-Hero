using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

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

public class P2pInterfaceController : MonoBehaviour
{
	string userText_WaitingForClients = "Waiting for clients...";
	string userText_WaitingForHost = "Waiting for host...";

	private static P2pInterfaceController _instance;

	public static P2pInterfaceController Instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<P2pInterfaceController> ();
			}
			return _instance;
		}
	}

	const float TIME_LIMIT = 30F;

	//FSM
	private AppState _currentState = AppState.Uninitialized;
	private Dictionary<AppState, GameObject> stateGoReference = new Dictionary<AppState, GameObject> ();
	public Transform inspector_ScreenContainer;


	private P2pGameMaster gameMaster;
	public Transform inspector_UiRoot;

	//Debug
	private Text _console;

	//Title Screen
	private InputField title_NameInput;

	//Lobby Screen
	private Text lobby_PlayerList;
	private Button lobby_StartButton;
	private Text lobby_Status;
	public string Lobby_Status {
		set {
			lobby_Status.text = value;
		}
	}

	//Game Screen
	private Text game_TimeRemaining;
	private Text game_Food;
	private Text game_Score;
	private Text game_PlayerName;

	//Result Screen
	private Text result_Result;
	private Button result_PlayButton;

	public List<RemotePlayer> PlayersInLobby {
		set {
			string output = "";
			for (int i = 0; i < value.Count; i++) {
				if (i != 0) {
					output += "\n";
				}
				output += value [i].profile.playerName;
			}
			lobby_PlayerList.text = output;
			WriteToConsole ("Completed set joined players");
		}
	}

	public void Title_SubmitProfileName ()
	{
		DeviceDatabase.Instance.ActivePlayerName = title_NameInput.text;
	}

	public void Lobby_SetStartButtonInteractive (bool b)
	{
		WriteToConsole ("setting start button to " + b);
		if (b) {
			lobby_StartButton.interactable = true;
		} else {
			lobby_StartButton.interactable = false;
		}
		WriteToConsole ("Completed set start button to " + b);
	}

	public void Result_SetPlayButtonInteractive (bool b)
	{
		result_PlayButton.interactable = b;
	}

	void Awake ()
	{
		_instance = this;
		gameMaster = GetComponent<P2pGameMaster> ();
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


		WriteToConsole ("Ui started");
		try {
			DeviceDatabase.Instance.ProfileChanged += UpdateProfileDisplay;

			//Title
			title_NameInput = inspector_UiRoot.transform.Find ("TitleScreen/NameInput").GetComponent<InputField> ();
			//Lobby
			lobby_PlayerList = inspector_UiRoot.transform.Find ("LobbyScreen/PlayerList").GetComponent<Text> ();
			lobby_StartButton = inspector_UiRoot.transform.Find ("LobbyScreen/StartButton").GetComponent<Button> ();
			lobby_Status = inspector_UiRoot.transform.Find ("LobbyScreen/Status").GetComponent<Text> ();

			//Game
			game_TimeRemaining = inspector_UiRoot.transform.Find ("GameScreen/TimeRemaining").GetComponent<Text> ();
			game_Food = inspector_UiRoot.transform.Find ("GameScreen/FoodLine0").GetComponent<Text> ();
			game_Score = inspector_UiRoot.transform.Find ("GameScreen/Score").GetComponent<Text> ();
			game_PlayerName = inspector_UiRoot.transform.Find ("GameScreen/PlayerName").GetComponent<Text> ();
			//Result
			result_Result = inspector_UiRoot.transform.Find ("ResultScreen/Result").GetComponent<Text> ();
			result_PlayButton = inspector_UiRoot.transform.Find ("ResultScreen/PlayButton").GetComponent<Button> ();
		} catch (Exception e) {
			WriteToConsole ("Ui start failed: " + e.Message);
		}

		ValidateUi ();

		InitializeUi ();

		//Display first screen
		SetScreenState (AppState.TitleScreen);
	}

	private void ValidateUi ()
	{
		try {
//		_console.text = "";
		
			//Title
			title_NameInput.text = "";
		
			//Host
			lobby_PlayerList.text = "";
			lobby_StartButton.interactable = true;
		
			//Game
			game_TimeRemaining.text = "";
			game_Food.text = "";
			game_Score.text = "";
			game_PlayerName.text = "";
		
			//Result
			result_Result.text = "";
			result_PlayButton.interactable = true;
			WriteToConsole ("Ui validated successfully");
		} catch (Exception e) {
			WriteToConsole ("ValidateUI failed: " + e.Message + ", " + e.TargetSite);
		}
	}

	private void InitializeUi ()
	{


		lobby_PlayerList.text = "";
		lobby_StartButton.interactable = false;
	}

	// Update is called once per frame
	void Update ()
	{
		if (gameMaster.gameInProgress) {
			if (gameMaster.TimerIsRunning) {
				game_TimeRemaining.text = gameMaster.TimeRemaining.ToString ("F1");
			}
			if (gameMaster.displayedFood.attributes.Count != 0) {
				game_Food.text = gameMaster.displayedFood.Name;
//				_foodLine1.text = gameMaster.displayedFood.attributes [1].Id;
//				_foodLine2.text = gameMaster.displayedFood.attributes [2].Id;
			}
			game_Score.text = gameMaster.currentScore.ToString ();
		}
	}

	public void Results_Display ()
	{
		try {
			List<GameResult> allGameResults = new List<GameResult> (P2pGameMaster.Instance.otherGameResults);
			allGameResults.Add (P2pGameMaster.Instance.myGameResult);
			//Sort by descending score
			allGameResults = allGameResults.OrderByDescending (gameResult => gameResult.finalScore).ToList ();
			string outputString = "";
			WriteToConsole ("all game results: " + allGameResults.Count);
			WriteToConsole ("active players: " + P2pGameMaster.Instance.ActiveProfiles.Count);
			for (int i = 0; i < allGameResults.Count; i++) {
				GameResult currentGameResult = allGameResults [i];
				if (i != 0) {
					outputString += "\n";
				}
				string place = null;
				switch (i) {
				case 0:
					place = "1st";
					break;
				case 1:
					place = "2nd";
					break;
				case 2:
					place = "3rd";
					break;
				default:
					place = (i + 1).ToString () + "th";
					break;
				}
				WriteToConsole ("current game result id: " + currentGameResult.profileId);
				OnlineProfile player = P2pGameMaster.Instance.ActiveProfiles.Single (onlineProfile => onlineProfile.profileId == currentGameResult.profileId);
				string name = player.playerName;
				outputString += place + " - " + name + " - " + currentGameResult.finalScore;
				result_Result.text = outputString;
			}
		} catch (Exception e) {
			WriteToConsole ("Exception in Results_Display: " + " message:  " + e.Message);
		}
	}

	public void WriteToConsole (string text)
	{
		if (_console == null) {
			_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text> ();
		}
		print ("Console: " + text);
		_console.text = text + "\n" + _console.text;
	}

	void UpdateProfileDisplay (object sender, EventArgs e)
	{
		string name = DeviceDatabase.Instance.ActivePlayerName;
		game_PlayerName.text = name;
		title_NameInput.text = name;
	}

	public void SetScreenState (AppState targetState)
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
				bool startButtonState = ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost &&
					ConnectionController.Instance.AccessiblePlayers.Count >= 2;
				P2pInterfaceController.Instance.WriteToConsole("setting start button to: " + startButtonState);
				P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive(startButtonState);
				
				//Set status
				string status = "Initializing status";
				if(ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Advertising) { 
					status = userText_WaitingForClients;
				} else if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedClient) {
					status = userText_WaitingForHost;
				} else {
					P2pInterfaceController.Instance.WriteToConsole("Unexpected remote state: " + ConnectionController.Instance.remoteStatus);
					status = "Initialization failed";
				}
				P2pInterfaceController.Instance.Lobby_Status = status;
				
				P2pInterfaceController.Instance.PlayersInLobby = ConnectionController.Instance.AccessiblePlayers;
				P2pInterfaceController.Instance.WriteToConsole ("completed lobby screen");
				break;
			case AppState.JoinScreen:
				ConnectionController.Instance.Client_BeginDiscovery ();
				P2pInterfaceController.Instance.WriteToConsole ("completed join screen");
				break;
			case AppState.GameScreen:
				game_PlayerName.text = DeviceDatabase.Instance.ActivePlayerName;
				P2pGameMaster.Instance.BeginNewGame ();
				P2pInterfaceController.Instance.WriteToConsole ("completed game screen");
				break;
			case AppState.ResultScreen:
				P2pInterfaceController.Instance.Result_SetPlayButtonInteractive (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost);
				P2pInterfaceController.Instance.Results_Display ();
				P2pInterfaceController.Instance.WriteToConsole ("completed result screen");
				break;
			case AppState.TitleScreen:
				P2pInterfaceController.Instance.WriteToConsole("IS, active player name: " + DeviceDatabase.Instance.ActivePlayerName);
				P2pInterfaceController.Instance.WriteToConsole("IS, name input: " + title_NameInput);
				title_NameInput.text = DeviceDatabase.Instance.ActivePlayerName;
				break;
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Shit, initialize state failed." + e.Message);
		}
	}

	# region button handlers

	public void Single_StartGame () {
		WriteToConsole ("Starting single player game");
		P2pGameMaster.Instance.LoadGameSettings (new GameSettings (TIME_LIMIT));
		SetScreenState (AppState.GameScreen);
	}

	public void Host_CreateGame ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Creating game");
		try {
//			P2pInterfaceController.Instance.WriteToConsole ("Wrote " + DeviceDatabase.Instance.ActivePlayerName + " to active profile");
			ConnectionController.Instance.Host_BeginAdvertising ();
			SetScreenState (AppState.LobbyScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in Host_CreateGame: " + e.Message);
		}
	}
	
	public void Host_StartGame ()
	{
		ConnectionController.Instance.Host_BeginSession (); //Stop advertising and update remote status
		
		GameSettings gameSettings = new GameSettings (TIME_LIMIT); //Bundle and send game settings to clients
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
		SetScreenState (AppState.JoinScreen);
	}
	
	public void DisplayResult ()
	{
		P2pInterfaceController.Instance.WriteToConsole ("Beginning display result, remote status: " + ConnectionController.Instance.remoteStatus);
		if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
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
		WriteToConsole ("Finishing game");
		try {
			if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
			    ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedClient) {
				P2pInterfaceController.Instance.WriteToConsole ("Game finished");
				SetScreenState (AppState.WaitingScreen);
				ConnectionController.Instance.BroadcastEvent (new GameResultPayload (gameResult));
			} else if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Idle) {
				SetScreenState(AppState.ResultScreen);
			} else {
				WriteToConsole("Unhandled remote status in GameFinished: " + ConnectionController.Instance.remoteStatus);
			}

			//Save records
			DeviceDatabase.Instance.RecordGameToActiveProfile(gameResult);

		} catch (Exception e) {
			WriteToConsole("Exception in GameFinished: " + e.Message);
		}

		
	}
	#endregion
}
