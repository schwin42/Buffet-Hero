using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public enum SoundEffect
{
	None = -1,
	Click = 0,
	Eat = 1,
	Swoop = 2,
	Pass = 3,
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
	StatsScreen = 6,
	OnlineScreen = 7,
}

public class P2pInterfaceController : MonoBehaviour
{
	string userText_WaitingForClients = "Waiting for clients...";
	string userText_WaitingForHost = "Waiting for host...";
	string userText_RealtimeLobbyStatus = "Multiplayer joined";
	private static P2pInterfaceController _instance;

	public static P2pInterfaceController Instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<P2pInterfaceController> ();
			}
			return _instance;
		}
	}

	const float TIME_LIMIT = 30F; //TODO Player should be able to set this during game setup

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

	//Audio
	public List<AudioClip> inspector_AudioClips; 

	//Game Screen
	private Text game_TimeRemaining;
	private Text game_Food;
	private Text game_Score;
	private Text game_PlayerName;
	private Text game_DeltaScore;
	private Transform game_OffScreenTransform;
	private Vector3 game_DeltaScoreStartLocation;

	//Result Screen
	private Text result_Result;
	private Button result_PlayButton;

	//Stats Screen
	private Text stats_Stats;

	//Join Screen
	private Text join_Progress;

	//Animation
	private const float SLERP_DURATION = 0.5F;
	private bool slerpInProgress = false;
	private float slerpProgress;
	private Transform slerpElement = null;
	private Vector3 slerpOrigin, slerpDestination;

	public List<RemotePlayer> PlayersInLobby {
		set {
			string output = "";
			for (int i = 0; i < value.Count; i++) {
				if (i != 0) {
					output += "\n";
				}
				if (value [i].profile != null) {
					output += value [i].profile.playerName;
				} else {
					//Do not add profile-less participant to user text
				}
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
			game_DeltaScore = inspector_UiRoot.transform.Find ("GameScreen/DeltaScore").GetComponent<Text> ();
			game_OffScreenTransform = inspector_UiRoot.transform.Find ("GameScreen/OffScreenTransform");
			//Result
			result_Result = inspector_UiRoot.transform.Find ("ResultScreen/Result").GetComponent<Text> ();
			result_PlayButton = inspector_UiRoot.transform.Find ("ResultScreen/PlayButton").GetComponent<Button> ();
			//Stats
			stats_Stats = inspector_UiRoot.transform.Find ("StatsScreen/Stats").GetComponent<Text> ();
			//Join
			join_Progress = inspector_UiRoot.transform.Find ("JoinScreen/Progress").GetComponent<Text> ();
		} catch (Exception e) {
			WriteToConsole ("Ui start failed: " + e.Message);
		}

		InitializeUi ();

		//Display first screen
		SetScreenState (AppState.TitleScreen);
	}

	private void InitializeUi ()
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
			game_DeltaScoreStartLocation = game_DeltaScore.transform.position;
		
			//Result
			result_Result.text = "";
			result_PlayButton.interactable = true;
			WriteToConsole ("Ui validated successfully");

			//Stats
			stats_Stats.text = "";

			//Join
			join_Progress.text = "";

			lobby_StartButton.interactable = false;

		} catch (Exception e) {
			WriteToConsole ("ValidateUI failed: " + e.Message + ", " + e.TargetSite);
		}
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
			}
			game_Score.text = gameMaster.currentScore.ToString ();
		}
		if (slerpInProgress) {
			if (slerpProgress >= 1) {
				slerpElement.gameObject.SetActive (false);
				slerpElement.position = game_DeltaScoreStartLocation;
			} else {
				slerpProgress += Time.deltaTime / SLERP_DURATION;
				slerpElement.transform.position = Vector3.Slerp (slerpOrigin, slerpDestination, slerpProgress);
			}
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
//		P2pInterfaceController.Instance.WriteToConsole ("Setting screen state to " + targetState + " from " + _currentState);
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
//		P2pInterfaceController.Instance.WriteToConsole ("initializing " + state);
		try {
			switch (state) {
			case AppState.LobbyScreen:
				bool startButtonState = (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
					ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) &&
					ConnectionController.Instance.AccessiblePlayers.Count >= 2;
				P2pInterfaceController.Instance.WriteToConsole ("setting start button to: " + startButtonState);
				P2pInterfaceController.Instance.Lobby_SetStartButtonInteractive (startButtonState);
				
				//Set status
				string status = "Initializing status";
				switch (ConnectionController.Instance.remoteStatus) {
				case ConnectionController.RemoteStatus.Advertising:
					status = userText_WaitingForClients;
					break;
				case ConnectionController.RemoteStatus.EstablishedClient:
					status = userText_WaitingForHost;
					break;
				case ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer:
					status = userText_RealtimeLobbyStatus;
					break;
				default:
					P2pInterfaceController.Instance.WriteToConsole ("Unexpected remote state: " + ConnectionController.Instance.remoteStatus);
					status = "Initialization failed";
					break;
				}
				P2pInterfaceController.Instance.Lobby_Status = status;
				
				P2pInterfaceController.Instance.PlayersInLobby = ConnectionController.Instance.AccessiblePlayers;
				P2pInterfaceController.Instance.WriteToConsole ("completed lobby screen");
				break;
			case AppState.JoinScreen:
				join_Progress.text = "";
				P2pInterfaceController.Instance.WriteToConsole ("completed join screen");
				break;
			case AppState.GameScreen:
				game_PlayerName.text = DeviceDatabase.Instance.ActivePlayerName;
				game_DeltaScore.gameObject.SetActive (false);
				P2pGameMaster.Instance.BeginNewGame ();
				P2pInterfaceController.Instance.WriteToConsole ("completed game screen");
				break;
			case AppState.ResultScreen:
				P2pInterfaceController.Instance.Result_SetPlayButtonInteractive (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
					ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Idle || //TODO Better way of checking for single player
					ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer);
				P2pInterfaceController.Instance.Results_Display ();
				P2pInterfaceController.Instance.WriteToConsole ("completed result screen");
				break;
			case AppState.TitleScreen:
				P2pInterfaceController.Instance.WriteToConsole ("IS, active player name: " + DeviceDatabase.Instance.ActivePlayerName);
				P2pInterfaceController.Instance.WriteToConsole ("IS, name input: " + title_NameInput);
				title_NameInput.text = DeviceDatabase.Instance.ActivePlayerName;
				break;
			case AppState.StatsScreen:
				string statsString = "";

				//TODO Player name in big text
				OnlineProfile activeProfile = DeviceDatabase.Instance.ActiveProfile;

				WriteToConsole ("ISSS, active profile: " + activeProfile.profileId);

				statsString += "Games played: " + activeProfile.gamesPlayed + "\n";
				statsString += "Foods eaten: " + activeProfile.foodsEaten + "\n";
				statsString += "Foods passed: " + activeProfile.foodsPassed + "\n";
				statsString += "Lifetime score: " + activeProfile.lifetimeScore + "\n";
				statsString += "Lifetime seconds played: " + activeProfile.lifetimeSecondsPlayed + "\n";
				statsString += "Best score: " + (activeProfile.bestScore.HasValue ? activeProfile.bestScore.ToString () : "None") + "\n";
				statsString += "Worst score: " + (activeProfile.worstScore.HasValue ? activeProfile.worstScore.ToString () : "None") + "\n";
				statsString += "Tastiest food eaten: \n" + (activeProfile.tastiestFoodEaten == null || !activeProfile.tastiestFoodEaten.isInitialized ? "None" : activeProfile.tastiestFoodEaten.ToString ()) + "\n";
				statsString += "Grossest food eaten: \n" + (activeProfile.grossestFoodEaten == null || !activeProfile.grossestFoodEaten.isInitialized ? "None" : activeProfile.grossestFoodEaten.ToString ()) + "\n";
				statsString += "Tastiest food passed: \n" + (activeProfile.tastiestFoodMissed == null || !activeProfile.tastiestFoodMissed.isInitialized ? "None" : activeProfile.tastiestFoodMissed.ToString ()) + "\n";
				statsString += "Grossest food passed: \n" + (activeProfile.grossestFoodMissed == null || !activeProfile.grossestFoodMissed.isInitialized ? "None" : activeProfile.grossestFoodMissed.ToString ()) + "\n";

				//average game, average food, average choices per second
				statsString += "Average game score: " + activeProfile.AverageGameScore.ToString ("F2") + "\n";
				statsString += "Average food score: " + activeProfile.AverageFoodScore.ToString ("F2") + "\n";
				statsString += "Average choices per second: " + activeProfile.AverageChoicesPerSecond.ToString ("F2");


				P2pInterfaceController.Instance.WriteToConsole ("Stats text = " + (stats_Stats == null ? "null" : "not null"));
				stats_Stats.text = statsString;
				break;
			}
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Shit, InitializeState failed." + e.Message + ", " + e.StackTrace);
		}
	}

	private void BeginUiSlerp (Transform element, Vector3 destination)
	{
		slerpElement = element;
		slerpOrigin = element.transform.position;
		slerpDestination = destination;
		slerpProgress = 0;
		slerpInProgress = true;
	}

	# region button handlers

	public void ButtonHandler_Game_Eat ()
	{
		P2pInterfaceController.Instance.PlaySound (SoundEffect.Eat);

		//Delta score
		float currentFoodQuality = P2pGameMaster.Instance.displayedFood.Quality.Value;
		game_DeltaScore.text = currentFoodQuality >= 0 ? "+" + currentFoodQuality.ToString () : currentFoodQuality.ToString ();
		//Set color to red or green
		game_DeltaScore.color = currentFoodQuality >= 0 ? Color.green : Color.red;
		//Enable gameobject
		game_DeltaScore.gameObject.SetActive (true);
		//Begin delta score animate to net score
		BeginUiSlerp (game_DeltaScore.transform, game_Score.transform.position);

		P2pGameMaster.Instance.Player_Eat ();
	}
	
	public void ButtonHandler_Game_Pass ()
	{
		P2pInterfaceController.Instance.PlaySound (SoundEffect.Pass);

		//Delta score
		float currentFoodQuality = P2pGameMaster.Instance.displayedFood.Quality.Value;
		game_DeltaScore.text = currentFoodQuality >= 0 ? "+" + currentFoodQuality.ToString () : currentFoodQuality.ToString ();
		//Set color to red or green
		game_DeltaScore.color = Color.grey;
		//Enable gameobject
		game_DeltaScore.gameObject.SetActive (true);
		//Begin delta score animate to net score
		BeginUiSlerp (game_DeltaScore.transform, game_OffScreenTransform.position);

		P2pGameMaster.Instance.Player_Pass ();
	}

	public void ButtonHandler_Title_OnePlayerGame ()
	{
		PlaySound (SoundEffect.Click);
		SinglePlayerStartGame (true);
	}

	public void ButtonHandler_Title_HostGame ()
	{
		PlaySound (SoundEffect.Click);
		P2pInterfaceController.Instance.WriteToConsole ("Creating game");
		try {
//			P2pInterfaceController.Instance.WriteToConsole ("Wrote " + DeviceDatabase.Instance.ActivePlayerName + " to active profile");
			ConnectionController.Instance.Host_BeginAdvertising ();
			SetScreenState (AppState.LobbyScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in Host_CreateGame: " + e.Message);
		}
	}
	
	public void ButtonHandler_Lobby_StartGame ()
	{
		PlaySound (SoundEffect.Click);
		if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Advertising || //Not established host yet
			ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) {
			StartMultiplayerGame (true);
		} else {
			P2pInterfaceController.Instance.WriteToConsole ("Error: Unable to start game from remote status: " + ConnectionController.Instance.remoteStatus);
		}
			
			
	}
	
	public void ButtonHandler_Title_JoinGame () //Nearby Join Game as Client
	{
		PlaySound (SoundEffect.Click);
		ConnectionController.Instance.Client_BeginDiscovery ();
		SetScreenState (AppState.JoinScreen);
	}
	
	public void ButtonHandler_DisconnectAndExit ()
	{
		PlaySound (SoundEffect.Click);
		ReturnToTitle (true);
	}

	public void ButtonHandler_ReturnToTitle ()
	{
		PlaySound (SoundEffect.Click);
		ReturnToTitle (false);
	}

	public void PlayAgain ()
	{
		PlaySound (SoundEffect.Click);
		WriteToConsole ("Starting PlayAgain");
		switch (ConnectionController.Instance.remoteStatus) {
		case ConnectionController.RemoteStatus.EstablishedHost:
		case ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer:
			StartMultiplayerGame (false);
			break;
		case ConnectionController.RemoteStatus.Idle:
			SinglePlayerStartGame (false);
			break;
		default:
			WriteToConsole ("Unhandled remote status in PlayAgain: " + ConnectionController.Instance.remoteStatus);
			break;
		}
	}

	public void ButtonHandler_Stats ()
	{
		PlaySound (SoundEffect.Click);
		SetScreenState (AppState.StatsScreen);
	}

	public void ButtonHandler_SignIn ()
	{
		PlaySound (SoundEffect.Click);
		ConnectionController.Instance.SignIn ();
	}

	public void ButtonHandler_QuickGame ()
	{ //RealTimeMultiplayer 
		PlaySound (SoundEffect.Click);
		WriteToConsole ("Creating quick game");
		ConnectionController.Instance.CreateQuickGame ();
		SetScreenState (AppState.JoinScreen);
	}

	public void ButtonHandler_MultiplayerScreen ()
	{
		PlaySound (SoundEffect.Click);
		SetScreenState (AppState.OnlineScreen);
	}

	#endregion

	#region local triggered events
	public void GameFinished (GameResult gameResult)
	{
		WriteToConsole ("Finishing game");
		try {
			if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost || 
				ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedClient ||
				ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) {
				P2pInterfaceController.Instance.WriteToConsole ("Game finished");
				SetScreenState (AppState.WaitingScreen);
				ConnectionController.Instance.BroadcastEvent (new GameResultPayload (gameResult));
			} else if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Idle) {
				SetScreenState (AppState.ResultScreen);
			} else {
				WriteToConsole ("Unhandled remote status in GameFinished: " + ConnectionController.Instance.remoteStatus);
			}
			
			//Save records
			DeviceDatabase.Instance.RecordGameToActiveProfile (gameResult, P2pGameMaster.Instance.currentSettings);
			
		} catch (Exception e) {
			WriteToConsole ("Exception in GameFinished: " + e.Message + ", " + e.StackTrace);
		}
	}
	#endregion

	#region remote triggered events

	public void Join_ReceivedRoomProgress (float progress)
	{
		join_Progress.text = progress.ToString ("F1") + "%";
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

	#endregion

	#region State Navigation

	private void SinglePlayerStartGame (bool firstGame)
	{
		WriteToConsole ("Starting single player game, first? " + firstGame);
		if (firstGame) {
			P2pGameMaster.Instance.LoadGameSettings (new GameSettings (TIME_LIMIT, 0, true));
		} else {
			P2pGameMaster.Instance.LoadGameSettings (new GameSettings (TIME_LIMIT, 
			                                                           P2pGameMaster.Instance.currentSettings.startFoodIndex + P2pGameMaster.Instance.myGameResult.choices.Count + 1, //Increment by 1 to make sure no one has even seen the first food
			                                                           false));
		}
		SetScreenState (AppState.GameScreen);
	}

	private void StartMultiplayerGame (bool firstGame)
	{

		//Generate game settings
		GameSettings gameSettings;
		if (firstGame) {
			gameSettings = new GameSettings (TIME_LIMIT, 0, true); 
		} else {
			gameSettings = new GameSettings (TIME_LIMIT, 
			                                 P2pGameMaster.Instance.currentSettings.startFoodIndex + 
				P2pGameMaster.Instance.AllGameResults.OrderByDescending (gameResult => gameResult.choices.Count).First ().choices.Count + 1, //Increment by 1 to make sure no one has even seen the first food
			                                 false);
		}
		P2pInterfaceController.Instance.WriteToConsole ("Generated game settings: " + gameSettings.qualifierSeed);
		//send game settings to clients
		if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.Advertising) {
			ConnectionController.Instance.Host_BeginSession (); //Stop advertising and establish host
			ConnectionController.Instance.BroadcastEvent (new StartGamePayload (gameSettings)); 
		} else if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedHost) {
			ConnectionController.Instance.BroadcastEvent (new StartGamePayload (gameSettings)); 
		} else if (ConnectionController.Instance.remoteStatus == ConnectionController.RemoteStatus.EstablishedRealTimeMultiplayer) {
			ConnectionController.Instance.Realtime_StartGame (gameSettings);
		}

		P2pGameMaster.Instance.LoadGameSettings (gameSettings);
		//TODO Check if event is successful
		SetScreenState (AppState.GameScreen);
	}

	public void ReturnToTitle (bool resetRemoteStatus)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Exiting to title");
		try {
			if (resetRemoteStatus) {
				ConnectionController.Instance.TerminateAllConnections ();
			}
			SetScreenState (AppState.TitleScreen);
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole ("Exception in ExitToTitle: " + e.Message);
		}
	}
	#endregion

	#region audio

	public void PlaySound (SoundEffect soundEffect)
	{
//		WriteToConsole("Playing sound: "+ soundEffect);
//		WriteToConsole("Sound found: " + inspector_AudioClips[(int)soundEffect].name);
		AudioSource.PlayClipAtPoint (inspector_AudioClips [(int)soundEffect], Vector3.zero);
//		AudioSource.PlayClipAtPoint(audioClips[(int)soundEffect], transform.position, volume);
	}

	#endregion
}
