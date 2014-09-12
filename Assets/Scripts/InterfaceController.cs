using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum PlayerUiState
{
	Uninitialized = -1,
	Join = 0,
	Entry = 1,
	Ready = 2,
	Game = 3,
	Inactive = 4
}

[System.Serializable]
public enum GameUIState
{
	Uninitialized = -1,
	Join = 0,
	MainGame = 1,
	Results = 2,
	Pause = 3,
	Stats0 = 4
}

public class InterfaceController : MonoBehaviour {

	public static InterfaceController Instance;

	//Inspector
		//Configurable
	public int maxScoresToDisplay = 12;

		//Colors
	public Color disabledTextColor;
	public Color enabledTextYellowColor;
	public Color neutralLightColor;
	public Color neutralDarkColor;
	public Color[] colors; 
	public Color[] letterRankColors = new Color[7];
	public Color[] playerTrayColors = new Color[4]; //Red, yellow, green, blue
	public Color[] highlightColors = new Color[4];


	public GameObject promptPrefab;
	public GameObject letterRankPrefab;
	public Vector3 localPromptPosition = new Vector3(0, -105, 0);
	public Vector3 localLetterRankPosition;

	//public UIWidget[] gameStateWidgets = new UIWidget[4];

	//Status
	public UILabel[] activePrompts = new UILabel[2];
	GameObject[] activeFoodRanks = new GameObject[2];
	 PlayerUiState[] playerUiStates = new PlayerUiState[] //Start FSM at all players uninitialized
	{
		(PlayerUiState) (-1),
		(PlayerUiState) (-1),
		(PlayerUiState) (-1),
		(PlayerUiState) (-1)
	};
	public GameUIState currentGameState = GameUIState.Uninitialized;
	public bool displayedFirstScreen = false;

	//public UIPanel[] playerPanels;

	public UILabel[] roundLabels;

	public UIPanel[] mirrorPanels;

	public UIPanel foregroundPanel;

	public UIButton[] nextButtons;

	public List<UIPanel> uiLookup; //Panels by player id
	//public UIPanel[] playerPanels;
	public UILabel[] winLabels;

	public UILabel[] highScoreNames;
	public UILabel[] highScoreAmounts;





	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
		for(int i = 0; i < 4; i++) //Magic number of players
		{
			Player player = GameController.Instance.possiblePlayers[i];
			player.EnableUi();
			player.trayBacker.color = playerTrayColors[player.playerId];

			//Start player FSMs
			SetPlayerUiState(player, PlayerUiState.Join);

			//Unready players
			player.playerChoice = PlayerChoice.Inactive;


		}

		//Acquire global UI
		//gameStateWidgets[0] = 
		SetGameUiState(GameUIState.Join);


			//UISprite backer = panel.transform.Find("Backer").GetComponent<UISprite>();
			//backer.color = 


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisplayPrompt(string s)
	{
		foreach(UILabel label in activePrompts)
		{
			Destroy(label);
		}

		for(int i = 0; i < 2; i++)
		{
			GameObject prompt = Instantiate(promptPrefab) as GameObject;
			prompt.transform.parent = mirrorPanels[i].transform;
			prompt.transform.localScale = Vector3.one;
			prompt.transform.localEulerAngles = Vector3.zero;
			prompt.transform.localPosition = localPromptPosition;
			UILabel promptLabel = prompt.GetComponent<UILabel>();
			activePrompts[i] = promptLabel;
			promptLabel.color = neutralDarkColor;
				//i > 0 ? neutralLightColor : neutralDarkColor;
			promptLabel.text = s;
			AudioController.Instance.PlaySound(SoundEffect.Swoop);
		}

		//foreach(UILabel label in promptLabels)
		//{
		//	label.text = s;
		//}
	}

//	public void WriteToOutcome(string s)
//	{
//		//outcomeLabel.text = s;
//	}

//	public void WriteToScore(float f)
//	{
//		//scoreLabel.text = "Score: "+f.ToString();
//	}

	public void EnableButton(ButtonHandler buttonHandler, bool b)
	{
		Debug.Log (buttonHandler.ToString(), buttonHandler.gameObject);
		UIButton uiButton = buttonHandler.GetComponent<UIButton>();
		//Debug.Log("Enable button: "+uiButton.gameObject.transform.parent.gameObject);
		UILabel uiLabel = uiButton.transform.Find("Label").GetComponent<UILabel>();

		if(b)
		{
			Debug.Log (buttonHandler.player.playerId);
			Debug.Log (uiLabel.ToString());
			uiButton.isEnabled = true;
			uiLabel.color = enabledTextYellowColor;
		} else {
			uiButton.isEnabled = false;
			uiLabel.color = disabledTextColor;
		}
	}

	public void DisplayRound()
	{
		foreach(UILabel label in roundLabels)
		{
			label.text = (GameController.Instance.currentRound+1).ToString()+"/ "+GameController.Instance.numberOfRounds;
		}
	}

	public void ShowFoodRank(LetterRank letterRank)
	{
		for(int i = 0; i < mirrorPanels.Length; i++)
		{
		GameObject letterRankGo = Instantiate(letterRankPrefab) as GameObject;

			letterRankGo.transform.parent = mirrorPanels[i].transform;
			letterRankGo.transform.localScale = Vector3.one;
			letterRankGo.transform.localRotation = Quaternion.identity;
			letterRankGo.transform.localPosition = localLetterRankPosition;
			letterRankGo.GetComponentInChildren<UILabel>().text = letterRank.ToString();
			letterRankGo.GetComponent<UISprite>().color = letterRankColors[(int)letterRank];
			activeFoodRanks[i] = letterRankGo;

		}
	}

	public void HidePrompts()
	{
		Debug.Log ("Hiding prompts");
		foreach(UILabel label in activePrompts)
		{
			Destroy (label.gameObject);
		}
		
	}

	public void HideFoodRank()
	{
		foreach(GameObject go in activeFoodRanks)
		{
			Destroy (go);
		}

	}

	public void EnableNextButtons(bool b)
	{
		foreach(UIButton button in nextButtons)
		{
			button.isEnabled = b;
			if(b)
			{
				button.transform.Find ("Label").GetComponent<UILabel>().color = neutralLightColor;
			} else {
				button.transform.Find ("Label").GetComponent<UILabel>().color = disabledTextColor;
			}
		}
	}

	public static void SetPlayerUiState(Player player, PlayerUiState targetState)
	{
		//Debug.Log (player.playerId + "to "+targetState+" from "+Instance.playerUiStates[player.playerId]);
		//Cache last state
		PlayerUiState oldState = Instance.playerUiStates[player.playerId];

		//Disable old elements
		if((int)oldState != -1)
		{
		player.stateWidgets[(int)oldState].gameObject.SetActive(false);
		} else {
			//If unit, disable all
			foreach(UIWidget widget in player.stateWidgets)
			{
				widget.gameObject.SetActive(false);
			}
		}
		//Terminate last state
		switch(oldState)
		{
		case PlayerUiState.Inactive:
			player.trayBacker.gameObject.SetActive(true);
			break;
		}

		//Enable new elements
		player.stateWidgets[(int)targetState].gameObject.SetActive(true);

		//Initialize state
		switch(targetState)
		{
		case PlayerUiState.Entry:
			Instance.HighlightControlType(player);
			player.nameField.text = player.profileInstance.playerName;
			break;
		case PlayerUiState.Ready:
			//player.playerName = player.nameField.value;
			//player.nameInput.value = "";
			break;
		case PlayerUiState.Join:
			//player.playerNameLabel.text = "Player"+player.playerId;
			player.playerChoice = PlayerChoice.Inactive;
			break;
		case PlayerUiState.Inactive:
			player.trayBacker.gameObject.SetActive(false);
			break;
		}


		//Record state
		Instance.playerUiStates[player.playerId] = targetState;
	
	}

	public void SetGameUiState(GameUIState targetState)
	{
		//Cache old state
		GameUIState oldState = currentGameState;

		Debug.Log (targetState);
		//Remove old state elements
		if(oldState != GameUIState.Uninitialized)
		{
		foreach(UIPanel panel in mirrorPanels)
		{
			panel.transform.Find("Widget"+oldState.ToString()).gameObject.SetActive(false);
		}
		foregroundPanel.transform.Find("Widget"+oldState.ToString()).gameObject.SetActive(false);
		} else {
			//Remove all panels
			foreach(UIPanel panel in mirrorPanels)
			{
				UIWidget[] stateWidgets = panel.GetComponentsInChildren<UIWidget>();
				foreach(UIWidget widget in stateWidgets)
				{
					if(widget.name.Contains("Widget"))
					{

					widget.gameObject.SetActive(false);
					}
				}
			}
			foreach(UIWidget widget in foregroundPanel.GetComponentsInChildren<UIWidget>())
			{
				if(widget.name.Contains("Widget"))
				{
					
					widget.gameObject.SetActive(false);
				}
			}

		}

		//Add new elements
		foreach(UIPanel panel in mirrorPanels)
		{
			panel.transform.Find("Widget"+targetState.ToString()).gameObject.SetActive(true);
		}
		foregroundPanel.transform.Find("Widget"+targetState.ToString()).gameObject.SetActive(true);
	
		//Initialize new elements
		switch(targetState)
		{
		case GameUIState.Join:
			if(!displayedFirstScreen)
			{
				foreach(Player player in GameController.Instance.possiblePlayers)
				{
					player.ChangeProfile("Guest");
				}

				displayedFirstScreen = true;
			} else {
				GameController.Instance.registeredPlayers.Clear();
				foreach(Player player in GameController.Instance.possiblePlayers)
				{
					player.playerChoice = PlayerChoice.Inactive;
					GameController.Instance.currentPhase = Phase.Pregame;
					if(player.playedInLastGame)
					{
						SetPlayerUiState(player, PlayerUiState.Entry);
						player.nameField.text = player.profileInstance.playerName;
					} else {
						Debug.Log ("Join");
						SetPlayerUiState(player, PlayerUiState.Join);
					}
				}
			}
			break;
		case GameUIState.Stats0:
			//Retrieve top n records from the top scores database in order
			PlayerResult[] allScoresSorted = UserDatabase.Instance.userInfo.playerGameResults.OrderByDescending(element => element.score).ToArray();
			List<PlayerResult> topResults = new List<PlayerResult>();
			for(int i = 0; i < allScoresSorted.Length; i++)
			{
				if(i >= maxScoresToDisplay)
				{
					break;
				}
				topResults.Add (allScoresSorted[i]);
			}
			DisplayScores(topResults);
			break;
		}


		currentGameState = targetState;
	}

	public void DisplayScores(List<PlayerResult> playerScores)
	{
		string names0 = "";
		string names1 = "";
		string scores0 = "";
		string scores1 = "";

		for(int i = 0; i < playerScores.Count; i++) 
		{
			string highlightStartTag = "";
			string highlightEndTag = "";
			//Check if any players played in last game with same name
			if(playerScores[i].gameId == UserDatabase.Instance.userInfo.totalGamesPlayed - 1)
			{
				Debug.Log ( "player scores name, registered players: "+playerScores[i].playerStringId+", "+
				           GameController.Instance.registeredPlayers.Count);
				Player[] query = (from player in GameController.Instance.registeredPlayers
					where player.profileInstance.playerName == playerScores[i].playerStringId
						select player).ToArray();
				if(query.Length > 0)
				{
Debug.Log("Query greater than 0");
					highlightStartTag = "[b]";
					highlightEndTag = "[/b]";
					//colorSubstring = query[0].playerColor;
				}
			}
			//if(from element in 
			//	playerScores[i].playerStringId == 

			if(i < maxScoresToDisplay / 2)
			{
				names0 += highlightStartTag + (i+1).ToString()+". "+ playerScores[i].playerStringId + highlightEndTag + "\n";
				scores0 += highlightStartTag + playerScores[i].score + highlightEndTag + "\n";
			} else {
				names1 += highlightStartTag + (i+1).ToString()+". "+ playerScores[i].playerStringId + highlightEndTag + "\n";
				scores1 += highlightStartTag + playerScores[i].score+ highlightEndTag  + "\n";
			}

		}
		highScoreNames[0].text = names0;
		highScoreNames[1].text = names1;
		highScoreNames[2].text = names0;
		highScoreNames[3].text = names1;
		highScoreAmounts[0].text = scores0;
		highScoreAmounts[1].text = scores1;
		highScoreAmounts[2].text = scores0;
		highScoreAmounts[3].text = scores1;
	}

	public void HighlightControlType(Player player)
	{
		if(player.controlType == ControlType.Human)
		{
			player.humanButton.defaultColor = highlightColors[player.playerId];
			player.computerButton.defaultColor = neutralLightColor;
			player.humanButton.GetComponentInChildren<UILabel>().color = neutralLightColor;
			player.computerButton.GetComponentInChildren<UILabel>().color = neutralDarkColor;
		} else if(player.controlType == ControlType.Computer)
		{
			player.computerButton.defaultColor = highlightColors[player.playerId];
			player.humanButton.defaultColor = neutralLightColor;
			player.computerButton.GetComponentInChildren<UILabel>().color = neutralLightColor;
			player.humanButton.GetComponentInChildren<UILabel>().color = neutralDarkColor;
		} else {
			Debug.LogError ("Something bad happened");
		}
	}

	public void WriteWinner(Player player)
	{
		foreach(UILabel winLabel in winLabels){
		winLabel.text = player.profileInstance.playerName + " wins with "+player.Score+" Points!";
		}
	}

	public void SetPlayerProfile(Player player, string profileName)
	{
		player.profileInstance.playerName = profileName;
		player.nameField.text = profileName; 
	}

}
