using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Advertisements;

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
public enum GameUiState
{
	Uninitialized = -1,
	Join = 0,
	MainGame = 1,
	Results = 2,
	Pause = 3,
	Stats0 = 4,
	Stats1 = 5,
	Stats2 = 8,
	Settings = 6,
	Rules = 7
}

[System.Serializable]
public class ColorScheme
{
	public Color defaultColor;
	public Color highlightedColor;
}

[System.Serializable]
public enum PopupUiState
{
	Uninitialized = -1,
	NoPopup = 0,
	Error = 1,
	Confirm = 2,
	Pause = 3,

}

public class InterfaceController : MonoBehaviour {

	private static InterfaceController _instance;
	public static InterfaceController Instance
	{
		get
		{
			if(_instance == null)
			{
			_instance = GameObject.FindObjectOfType<InterfaceController>();
			}
			return _instance;
		}
	}

	//Inspector
		//Configurable
	public int maxScoresToDisplay = 12;
	public string selectProfileString = "Select Profile";
	public string stats1NoFoodString = "None";

		//Colors
	public Color disabledTextColor;
	public Color enabledTextYellowColor;
	public Color neutralLightColor;
	public Color neutralDarkColor;
	//public Color[] colors; 
	public Color[] letterRankColors = new Color[7];
	//public Color[] playerTrayColors = new Color[4]; //Red, yellow, green, blue
	public ColorScheme[] playerSchemes = new ColorScheme[6];
	public Color[] highlightColors = new Color[4];
	public Color activeMenuItemColor;
	public Color inactiveMenuItemColor;
	//public Color[] playerColors;

	//Inspector
	public GameObject promptPrefab;
	public GameObject letterRankPrefab;
	public Vector3 localPromptPosition = new Vector3(0, -105, 0);
	public Vector3 localLetterRankPosition;

	//public UIWidget[] gameStateWidgets = new UIWidget[4];

	//Status
	public bool adsEnabled = true;
	public UILabel[] activePrompts = new UILabel[2];
	GameObject[] activeFoodRanks = new GameObject[2];
	 PlayerUiState[] playerUiStates = new PlayerUiState[] //Start FSM at all players uninitialized
	{
		(PlayerUiState) (-1),
		(PlayerUiState) (-1),
		(PlayerUiState) (-1),
		(PlayerUiState) (-1)
	};
	public GameUiState currentGameState = GameUiState.Uninitialized;
	public PopupUiState currentPopupState = PopupUiState.Uninitialized;
	public bool displayedFirstScreen = false;
	//public List<Profile> selectedProfiles = new List<Profile>();
	public List<ProfileMenuHandler> activeProfileMenus = new List<ProfileMenuHandler>();

	//public UIPanel[] playerPanels;



	public UIPanel[] mirrorPanels;
	public UIPanel foregroundPanel;
	public UIPanel popupPanel;

	public UILabel[] roundLabels;
	public UIButton[] nextButtons;

	public List<UIPanel> uiLookup; //Panels by player id
	//public UIPanel[] playerPanels;
	public UILabel[] winLabels;

	public UILabel[] highScoreNames;
	public UILabel[] highScoreAmounts;

	//Stats1
	public UILabel[] stats1Values;
	public UILabel[] stats1Titles;
	public UILabel[] stats1tastiestEaten;
	public UILabel[] stats1grossestEaten;

	//Stats2
	public UILabel[] stats2Values;

	//Rules
	public ToggleRule rulesRoundsInput;
	public ToggleRule rulesServingsInput;
	public ToggleRule rulesEatersInput;
	public ToggleRule rulesHpInput;

	//Popup
	public GameObject popupDim;

	//Other
	public Collider blockingCollider;




	void Awake()
	{
		_instance = this;
	}

	// Use this for initialization
	void Start () {
	





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
		//Debug.Log (player.playerId);
		           //+ "to "+targetState+" from "+Instance.playerUiStates[player.playerId]);
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
			player.entryNameField.text = player.ProfileInstance.playerName == "Guest" ? Instance.selectProfileString : player.ProfileInstance.playerName;
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

	public void SetGameUiState(GameUiState targetState)
	{
		Debug.Log ("Switching to "+targetState+"from "+currentGameState+" @"+Time.frameCount);
		//Cache old state
		GameUiState oldState = currentGameState;

		//Debug.Log (targetState);
		//Remove old state elements
		if(oldState != GameUiState.Uninitialized)
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
		//Terminate last state
		switch(oldState)
		{
		case GameUiState.Rules:
			GameController.Instance.servingsPerFood = rulesServingsInput.ruleValue;
			GameController.Instance.numberOfRounds = rulesRoundsInput.ruleValue;
			GameController.Instance.forcedEaters = rulesEatersInput.ruleValue;
			GameController.Instance.startingHp = rulesHpInput.ruleValue;
			break;
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
		case GameUiState.Join:
			if(!displayedFirstScreen)
			{
				foreach(Player player in GameController.Instance.possiblePlayers)
				{
					if(player.ProfileInstance == null || string.IsNullOrEmpty(player.ProfileInstance.playerName))
					{
					player.ChangeProfile("Guest");
					} else {
						player.ChangeProfile(player.ProfileInstance.playerName);
					}
				}

				displayedFirstScreen = true;
			} else {
				GameController.Instance.registeredPlayers.Clear();
				foreach(Player player in GameController.Instance.possiblePlayers)
				{
					//player.playerChoice = PlayerChoice.Inactive;
					GameController.Instance.currentPhase = Phase.Pregame;

						if(oldState == GameUiState.Rules || oldState == GameUiState.Settings)
						{
						//Don't change player state
					} else if(oldState == GameUiState.MainGame || oldState == GameUiState.Results || oldState == GameUiState.Stats0 
					          || oldState == GameUiState.Stats1 || oldState == GameUiState.Stats2){
						if(player.playedInLastGame)
						{
							//Debug.Log (player.name);
						SetPlayerUiState(player, PlayerUiState.Ready);
							player.playerChoice = PlayerChoice.Ready;
						player.entryNameField.text = player.ProfileInstance.playerName;
						} else {
							SetPlayerUiState(player, PlayerUiState.Join);
							player.playerChoice = PlayerChoice.Inactive;
						}
					} else {
						Debug.Log ("Join");
						SetPlayerUiState(player, PlayerUiState.Join);
					}
				}
			}
			break;
		case GameUiState.Stats0:
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
		case GameUiState.Stats1:
			for(int i = 0; i < GameController.Instance.possiblePlayers.Count; i++)
			{
				Player player = GameController.Instance.possiblePlayers[i];
				if(GameController.Instance.registeredPlayers.Contains(player))
				   {
				string valueOutput = "";
				string titleOutput = player.ProfileInstance.playerName;
				Food tastiestFoodEaten = player.ProfileInstance.tastiestFoodEaten;
				string tastiestEatenOutput = tastiestFoodEaten.Quality == 0 ? stats1NoFoodString : tastiestFoodEaten.Name + ": " + tastiestFoodEaten.Quality;
				Food grossestFoodEaten = player.ProfileInstance.grossestFoodEaten;
				string grossestEatenOutput = grossestFoodEaten.Quality == 0 ? stats1NoFoodString : grossestFoodEaten.Name + ": " + grossestFoodEaten.Quality;
				valueOutput = 
					player.ProfileInstance.gamesPlayed + "\n" +
						player.ProfileInstance.lifetimeScore + "\n" +
						player.ProfileInstance.AverageFoodScore.ToString("F2") + "\n" +
						player.ProfileInstance.bestScore;
				stats1Values[player.playerId].text = valueOutput;
				stats1Titles[player.playerId].text = titleOutput;
				stats1tastiestEaten[player.playerId].text = tastiestEatenOutput;
				stats1grossestEaten[player.playerId].text = grossestEatenOutput;
				 } else {
					//Zero out all player stats1 fields
					//string valueOutput = "";
					//string titleOutput = "";
					//Food tastiestFoodEaten = player.ProfileInstance.tastiestFoodEaten;
					//string tastiestEatenOutput = "";
					//Food grossestFoodEaten = player.ProfileInstance.grossestFoodEaten;
					//string grossestEatenOutput = "";
					//valueOutput = "";
					stats1Values[player.playerId].text = "";
					stats1Titles[player.playerId].text = "";
					stats1tastiestEaten[player.playerId].text = "";
					stats1grossestEaten[player.playerId].text = "";

				}
			}
			break;
		case GameUiState.Stats2:
			string tastiestFoodName = GameController.Instance.tastiestFood.Name;
			string tastiestFoodValue = GameController.Instance.tastiestFood.Quality.ToString();
			string tastiestFoodEatenBy = GetFormattedPlayerString(GameController.Instance.tastiestFoodEatenBy);
			string grossestFoodName = GameController.Instance.grossestFood.Name;
			string grossestFoodValue = GameController.Instance.grossestFood.Quality.ToString();
			string grossestFoodEatenBy = GetFormattedPlayerString(GameController.Instance.grossestFoodEatenBy);
			string quickestNabFood = GameController.Instance.quickestNab.ToString();
			string quickestNabTime = GameController.Instance.quickestNabTime.ToString();
			string quickestNabEatenBy = GetFormattedPlayerString(GameController.Instance.quickestNabEatenBy);
			List<Player> foodSortedPlayers = GameController.Instance.registeredPlayers.OrderByDescending(player => player.plate.foods.Count).ToList();
			int mostFoodsEaten = foodSortedPlayers[0].plate.foods.Count;
			int leastFoodsEaten = foodSortedPlayers[foodSortedPlayers.Count - 1].plate.foods.Count;
			List<Player> mostFoodsEatenByList = new List<Player>();
			for (int i = 0; i < foodSortedPlayers.Count; i++)
			{
				if(foodSortedPlayers[i].plate.foods.Count == mostFoodsEaten)
				{
					mostFoodsEatenByList.Add (foodSortedPlayers[i]);
				} else {
					break;
				}
			}
			string mostFoodsQuantity = mostFoodsEaten.ToString();
			string mostFoodsEatenBy = GetFormattedPlayerString(mostFoodsEatenByList);
			List<Player> leastFoodsEatenByList = new List<Player>();
			for (int i = foodSortedPlayers.Count - 1; i >= 0; i--)
			{
				if(foodSortedPlayers[i].plate.foods.Count == leastFoodsEaten)
				{
					mostFoodsEatenByList.Add (foodSortedPlayers[i]);
				} else {
					break;
				}
			}
			string leastFoodsQuantity = leastFoodsEaten.ToString();
			string leastFoodsEatenBy = GetFormattedPlayerString(leastFoodsEatenByList);
			string neutralDarkTag = HexTag(neutralDarkColor);
			string outputString = string.Format(
				neutralDarkTag+
				"Tastiest Food - {0} ({1}) - {2}\n"+
				neutralDarkTag+
				"Grossest Food - {3} ({4}) - {5}\n"+
				neutralDarkTag+
				"Quickest Nab - {6} ({7}) - {8}\n"+
				neutralDarkTag+
				"Most Foods Eaten - {9} with {10}\n"+
				neutralDarkTag+
				"Least Foods Eaten - {11} with {12}"
				, new string[13]
				{
				tastiestFoodName,
				tastiestFoodValue,
				tastiestFoodEatenBy,
				grossestFoodName,
				grossestFoodValue,
				grossestFoodEatenBy,
				quickestNabFood,
				quickestNabTime,
				quickestNabEatenBy,
				mostFoodsEatenBy,
				mostFoodsQuantity,
				leastFoodsEatenBy,
				leastFoodsQuantity
			});
			foreach(UILabel label in stats2Values)
			{
				label.text = outputString;
			}

			break;
		case GameUiState.Rules:
			rulesServingsInput.ruleValue = GameController.Instance.servingsPerFood;
			rulesRoundsInput.ruleValue = GameController.Instance.numberOfRounds;
			rulesEatersInput.ruleValue = GameController.Instance.forcedEaters;
			rulesHpInput.ruleValue = GameController.Instance.startingHp;
			break;
		}

		//Update current state
		currentGameState = targetState;
	}

	string GetFormattedPlayerString(Player player)
	{
		return HexTag(player.playerPanelScript.playerScheme.defaultColor)+player.ProfileInstance.playerName;
	}

	string GetFormattedPlayerString(List<Player> players)
	{
		string outputString = "";
		for(int i = 0; i < players.Count; i++)
		{
			Player player = players[i];

			outputString += HexTag(player.playerPanelScript.playerScheme.defaultColor)+player.ProfileInstance.playerName;
			if(i != players.Count - 1)
			{
				outputString += " ";
			}
			
		}
		return outputString;
	}

	public void SetPopupUiState(PopupUiState targetState)
	{
		Debug.Log ("Switching to "+targetState+"from "+currentGameState+" @"+Time.frameCount);
		//Cache old state
		PopupUiState oldState = currentPopupState;
		
		UIPanel panel = popupPanel;

		//Remove old state elements
		if(oldState == PopupUiState.Uninitialized)
		{
			//Remove all panels
			UIWidget[] stateWidgets = panel.GetComponentsInChildren<UIWidget>();
			foreach(UIWidget widget in stateWidgets)
			{
				if(widget.name.Contains("Widget"))
				{
					widget.gameObject.SetActive(false);
				}
			}
		} else if(oldState == PopupUiState.NoPopup)
		{ 
			//No widget to remove, do nothing
		} else {
			panel.transform.Find("Widget"+oldState.ToString()).gameObject.SetActive(false);
		}

		//Terminate last state
		switch(oldState)
		{
		case PopupUiState.Pause:
			Time.timeScale = 1f;
			break;
		}

		//Add new elements
		if(targetState != PopupUiState.NoPopup && targetState != PopupUiState.Uninitialized)
		{
		panel.transform.Find("Widget"+targetState.ToString()).gameObject.SetActive(true);
		}

		//Initialize new elements
		if(targetState != PopupUiState.NoPopup)
		{
			popupDim.SetActive(true);
		} else {
			popupDim.SetActive(false);
		}
		switch(targetState)
		{
		case PopupUiState.Pause:
			Time.timeScale = 0f;
			break;
		}

		//Update current state
		currentPopupState = targetState;
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
					where player.ProfileInstance.playerName == playerScores[i].playerStringId
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
		Debug.Log (player);
		if(player.controlType == ControlType.Human)
		{
			player.humanButton.color = player.playerPanelScript.playerScheme.highlightedColor;
			//	highlightColors[player.playerId];
			player.computerButton.color = neutralLightColor;
			player.humanButtonLabel.color = neutralLightColor;
			player.computerButtonLabel.color = neutralDarkColor;
		} else if(player.controlType == ControlType.Computer)
		{
			player.computerButton.color = player.playerPanelScript.playerScheme.highlightedColor;
			//	highlightColors[player.playerId];
			player.humanButton.color = neutralLightColor;
			player.computerButtonLabel.color = neutralLightColor;
			player.humanButtonLabel.color = neutralDarkColor;
		} else {
			Debug.LogError ("Something bad happened");
		}
	}

	public void WriteWinner(Player player)
	{
		foreach(UILabel winLabel in winLabels){
		winLabel.text = player.ProfileInstance.playerName + " wins with "+player.Score+" Points!";
		}
	}

	public void InitializeInterface()
	{
		for(int i = 0; i < 4; i++) //Magic number of players
		{
			Player player = GameController.Instance.possiblePlayers[i];
			player.EnableUi();
			//player.trayBacker.color = playerTrayColors[player.playerId];
			player.playerPanelScript.SetPanelColor(playerSchemes[player.playerId]);

			//Start player FSMs
			SetPlayerUiState(player, PlayerUiState.Join);
			
			//Unready players
			player.playerChoice = PlayerChoice.Inactive;
			
			
		}
		
		//Acquire global UI
		//gameStateWidgets[0] = 
		SetGameUiState(GameUiState.Join);

		SetPopupUiState(PopupUiState.NoPopup);
		
		//UISprite backer = panel.transform.Find("Backer").GetComponent<UISprite>();
		//backer.color = 
	}

//	public void SetPlayerProfile(Player player, string profileName)
//	{
//		player.profileInstance.playerName = profileName;
//		player.nameField.text = profileName; 
//	}

//	public void SetBlockingCollider(bool enabled)
//	{
//		if(enabled)
//		{
//			blockingCollider.gameObject.SetActive(true);
//		} else {
//			blockingCollider.gameObject.SetActive(false);
//		}
//	}

	public static string HexTag(Color color)
	{
		Color32 color32 = color;
		string outputTag = "["+color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2")+"]";
		//print (outputTag);
		return outputTag;
	}

}
