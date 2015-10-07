using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class P2pInterfaceController : MonoBehaviour {

	private static P2pInterfaceController _instance;
	public static P2pInterfaceController Instance {
		get {
			if(_instance == null) {
				_instance = FindObjectOfType<P2pInterfaceController>();
			}
			return _instance;
		}
	}

	private P2pGameMaster gameMaster;
	public Transform inspector_UiRoot;

	//Title Screen
	private InputField title_NameInput;

	//Host Screen
	private Text host_PlayerList;
	private Button host_StartButton;

	//Game Screen
	private Text _timeRemainingText;
	private Text _foodLine0;
	private Text _score;
	private Text _console;

	//Result Screen
	private Text result_Result;
	private Button result_PlayButton;

	public List<RemotePlayer> Host_JoinedPlayers {
		set {
			string output = "";
			for (int i = 0; i < value.Count; i++) {
				if(i != 0) {
					output += "\n";
				}
				output += value[i].profile.playerName;
			}
			host_PlayerList.text = output;
			WriteToConsole("Completed set joined players");
		}
	}

	public string Title_SubmitProfileName ()
	{
		return title_NameInput.text == "" ? "Guest" : title_NameInput.text;
	}

	public void Host_SetStartButtonInteractive (bool b)
	{
		WriteToConsole ("setting start button to " + b);
		if (b) {
			host_StartButton.interactable = true;
		} else {
			host_StartButton.interactable = false;
		}
		WriteToConsole("Completed set start button to " + b);
	}

	public void Result_SetPlayButtonInteractive (bool b) {
		result_PlayButton.interactable = b;
	}

	void Awake() {
		_instance = this;
		gameMaster = GetComponent<P2pGameMaster> ();
	}

	// Use this for initialization
	void Start () {
		WriteToConsole ("Ui started");
		try {
		_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text>();

		//Title
		title_NameInput = inspector_UiRoot.transform.Find ("TitleScreen/NameInput").GetComponent<InputField>();

		//Host
		host_PlayerList = inspector_UiRoot.transform.Find ("HostScreen/PlayerList").GetComponent<Text>();
		host_StartButton = inspector_UiRoot.transform.Find ("HostScreen/StartButton").GetComponent<Button>();

		//Game
		_timeRemainingText = inspector_UiRoot.transform.Find ("GameScreen/TimeRemaining").GetComponent<Text>();
		_foodLine0 = inspector_UiRoot.transform.Find ("GameScreen/FoodLine0").GetComponent<Text>();
		_score = inspector_UiRoot.transform.Find ("GameScreen/Score").GetComponent<Text>();

		//Result
		result_Result = inspector_UiRoot.transform.Find ("ResultScreen/Result").GetComponent<Text>();
		result_PlayButton = inspector_UiRoot.transform.Find ("ResultScreen/PlayButton").GetComponent<Button>();
		} catch (Exception e) {
			WriteToConsole("Ui start failed");
		}

		ValidateUi ();

		InitializeUi ();
	}

	private void ValidateUi () {
		try {
//		_console.text = "";
		
		//Title
		title_NameInput.text = "";
		
		//Host
			host_PlayerList.text = "";
			host_StartButton.interactable = true;
		
		//Game
			_timeRemainingText.text = "";
			_foodLine0.text = "";
			_score.text = "";
		
		//Result
			result_Result.text = "";
			result_PlayButton.interactable = true;
			WriteToConsole ("Ui validated successfully");
		} catch (Exception e) {
			WriteToConsole("ValidateUI failed: " + e.Message + ", " + e.TargetSite);
		}
	}

	private void InitializeUi() {


		host_PlayerList.text = "";
		host_StartButton.interactable = false;
	}

	// Update is called once per frame
	void Update () {
		if (gameMaster.gameInProgress) {
			if (gameMaster.TimerIsRunning) {
				_timeRemainingText.text = gameMaster.TimeRemaining.ToString ("F2");
			}
			if (gameMaster.displayedFood.attributes.Count != 0) {
				_foodLine0.text = gameMaster.displayedFood.Name;
//				_foodLine1.text = gameMaster.displayedFood.attributes [1].Id;
//				_foodLine2.text = gameMaster.displayedFood.attributes [2].Id;
			}
			_score.text = "Score: " + gameMaster.currentScore.ToString();
		}
	}

	public void Results_Display ()
	{
		try {
			List<GameResult> allGameResults = new List<GameResult> (P2pGameMaster.Instance.otherGameResults);
			allGameResults.Add(P2pGameMaster.Instance.myGameResult);
			//Sort by descending score
			allGameResults = allGameResults.OrderByDescending(gameResult => gameResult.score).ToList();
			string outputString = "";
			WriteToConsole("all game results: " + allGameResults.Count);
			WriteToConsole("active players: " + P2pGameMaster.Instance.ActivePlayers.Count);
			for(int i = 0; i < allGameResults.Count; i++) {
				GameResult currentGameResult = allGameResults[i];
				if(i != 0) {
					outputString += "\n";
				}
				string place = null;
				switch(i) {
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
					place = (i + 1).ToString() + "th";
					break;
				}
				WriteToConsole("current game result id: " + currentGameResult.profileId);
				OnlineProfile player = P2pGameMaster.Instance.ActivePlayers.Single (onlineProfile => onlineProfile.profileId == currentGameResult.profileId);
				string name = player.playerName;
				outputString += place + " - " + name + " - " + currentGameResult.score;
				result_Result.text = outputString;
			}
		} catch (Exception e) {
			WriteToConsole("Exception in Results_Display: " + e.Message + "\n" + e.Source);
		}
	}

	public void WriteToConsole(string text) {
		if (_console == null) {
			_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text>();
		}
		print ("Console: " + text);
		_console.text = text + "\n" + _console.text;
	}
}
