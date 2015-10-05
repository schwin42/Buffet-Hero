using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

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
		}
	}

	public void Host_SetStartButtonInteractive (bool b)
	{
		if (b) {
			host_StartButton.interactable = true;
		} else {
			host_StartButton.interactable = false;
		}
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

		_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text>();

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

		//TODO Validate that these are all properly acquire here rather than later on

		InitializeUi ();
	}

	private void InitializeUi() {
		_console.text = "";

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
//		WriteToConsole("Found game results: " + P2pGameMaster.Instance.otherGameResults.ToString());
//		WriteToConsole("Found my game result" + P2pGameMaster.Instance.myGameResult.ToString());
//		WriteToConsole ("Found result_result" + result_result.ToString());

		try {
		float highestScore = -9999;
		Guid winningPlayer = Guid.Empty;
		foreach (GameResult gameResult in P2pGameMaster.Instance.otherGameResults) {
			if(gameResult.score > highestScore) {
				highestScore = gameResult.score;
				winningPlayer = gameResult.profileId;
			}
		}
		if (gameMaster.myGameResult.score > highestScore) {
			result_Result.text = "You win with " + gameMaster.myGameResult.score;
		} else {
			result_Result.text = "You lose with " + gameMaster.myGameResult.score;
		} //TODO Handle ties

		} catch (Exception e) {
			WriteToConsole("Exception in Results_Display: " + e.Message);
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
