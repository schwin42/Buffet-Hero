﻿using UnityEngine;
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

	public ColorScheme (Color defaultColor, Color highlightedColor) {
		this.defaultColor = defaultColor;
		this.highlightedColor = highlightedColor;
	}
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
	private Vector3 _winnerGoStartPosition = new Vector3(-1000F, -105F, 0F);

		//Colors
	public Color disabledTextColor;
	public Color enabledTextYellowColor;
	public Color neutralLightColor;
	public Color neutralDarkColor;
	public Color[] letterRankColors = new Color[7];
	public ColorScheme[] PlayerSchemesPool = new ColorScheme[6] {
		new ColorScheme(new Color32(202, 21, 21, 255), new Color32(255, 0, 0, 255)),
		new ColorScheme(new Color32(231, 196, 2, 255), new Color32(255, 237, 0, 255)),
		new ColorScheme(new Color32(20, 180, 0, 255), new Color32(9, 255, 0, 255)),
		new ColorScheme(new Color32(24, 18, 199, 255), new Color32(0, 44, 255, 255)),
		new ColorScheme(new Color32(204, 102, 36, 255), new Color32(255, 134, 53, 255)),
		new ColorScheme(new Color32(136, 18, 199, 255), new Color32(195, 87, 253, 255)),
	};
	public Color[] highlightColors = new Color[4];
	public Color activeMenuItemColor;
	public Color inactiveMenuItemColor;

	//Inspector
	public GameObject promptPrefab;
	public GameObject letterRankPrefab;
	public UILabel winnerPrefab;
	public Vector3 localPromptPosition = new Vector3(0, -105, 0);
	public Vector3 localLetterRankPosition;

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
	public List<ProfileMenuHandler> activeProfileMenus = new List<ProfileMenuHandler>();
	public string winString = "";
	public List<GameObject> activeWinnerGos = new List<GameObject>();

	[SerializeField] private UIPanel[] mirrorPanels;
    [SerializeField] private UIPanel foregroundPanel;
    [SerializeField] private UIPanel popupPanel;

	[SerializeField] private UILabel[] roundLabels;
    [SerializeField] private UIButton[] nextButtons;

	public List<UIPanel> uiPanelsByPlayerId;

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
	
	void Start () { }
	void Update () { }

	public void DisplayPrompt(string s) {
		foreach(UILabel label in activePrompts) {
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
	}

	public void EnableButton(ButtonHandler buttonHandler, bool b)
	{
		//Debug.Log (buttonHandler.ToString(), buttonHandler.gameObject);
		UIButton uiButton = buttonHandler.GetComponent<UIButton>();
		//Debug.Log("Enable button: "+uiButton.gameObject.transform.parent.gameObject);
		UILabel uiLabel = uiButton.transform.Find("Label").GetComponent<UILabel>();

		if(b)
		{
			//Debug.Log (buttonHandler.player.playerId);
			//Debug.Log (uiLabel.ToString());
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

	public void HidePrompts() {
		foreach(UILabel label in activePrompts) {
			Destroy (label.gameObject);
		}
	}

	public void HideFoodRank() {
		foreach(GameObject go in activeFoodRanks) {
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

		//Cache last state
//		print(player.Id);
//		print (Instance.playerUiStates);
		PlayerUiState oldState = Instance.playerUiStates[player.Id];

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
			break;
		case PlayerUiState.Join:
			player.playerChoice = PlayerChoice.Inactive;
			break;
		case PlayerUiState.Inactive:
			player.trayBacker.gameObject.SetActive(false);
			break;
		}

		//Record state
		Instance.playerUiStates[player.Id] = targetState;
	
	}

	public void SetGameUiState(GameUiState targetState)
	{
		Debug.Log ("Switching to "+targetState+" from "+currentGameState+" @"+Time.frameCount);
		//Cache old state
		GameUiState oldState = currentGameState;

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
		case GameUiState.Results:
			for(int i = 0; i < activeWinnerGos.Count; i++)
			{
				Destroy (activeWinnerGos[i]);
			}
			activeWinnerGos.Clear ();
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
				foreach(Player player in GameController.Instance.PossiblePlayers)
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
				foreach(Player player in GameController.Instance.PossiblePlayers)
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
		case GameUiState.Results:
			foreach(UIPanel panel in mirrorPanels)
			{
				UILabel winnerLabel = Instantiate(winnerPrefab) as UILabel;
				GameObject winnerGo = winnerLabel.gameObject;
				winnerLabel.text = winString;
				winnerGo.transform.parent = panel.transform;
				winnerGo.transform.localScale = Vector3.one;
				winnerGo.transform.localEulerAngles = Vector3.zero;
				winnerGo.transform.localPosition = _winnerGoStartPosition;
				activeWinnerGos.Add (winnerGo);
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
			for(int i = 0; i < GameController.Instance.PossiblePlayers.Count; i++)
			{
				Player player = GameController.Instance.PossiblePlayers[i];
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
				stats1Values[player.Id].text = valueOutput;
				stats1Titles[player.Id].text = titleOutput;
				stats1tastiestEaten[player.Id].text = tastiestEatenOutput;
				stats1grossestEaten[player.Id].text = grossestEatenOutput;
				 } else {
					//Zero out all player stats1 fields
					//string valueOutput = "";
					//string titleOutput = "";
					//Food tastiestFoodEaten = player.ProfileInstance.tastiestFoodEaten;
					//string tastiestEatenOutput = "";
					//Food grossestFoodEaten = player.ProfileInstance.grossestFoodEaten;
					//string grossestEatenOutput = "";
					//valueOutput = "";
					stats1Values[player.Id].text = "";
					stats1Titles[player.Id].text = "";
					stats1tastiestEaten[player.Id].text = "";
					stats1grossestEaten[player.Id].text = "";

				}
			}
			break;
		case GameUiState.Stats2:

			//Cache colors
			string neutralDarkTag = HexTag(neutralDarkColor);

			//Acquire/ generate dynamic text strings
			string tastiestFoodName = GameController.Instance.TastiestFood.Name;
			string tastiestFoodValue = GameController.Instance.TastiestFood.Quality.ToString();
			string tastiestFoodEatenBy = GetFormattedPlayerString(GameController.Instance.tastiestFoodEatenBy, neutralDarkTag);
			string grossestFoodName = GameController.Instance.grossestFood.Name;
			string grossestFoodValue = GameController.Instance.grossestFood.Quality.ToString();
			string grossestFoodEatenBy = GetFormattedPlayerString(GameController.Instance.grossestFoodEatenBy, neutralDarkTag);
			string quickestNabFood = GameController.Instance.quickestNab.Name.ToString();
			string quickestNabTime = GameController.Instance.quickestNabTime.ToString("F2");
			string quickestNabEatenBy = GetFormattedPlayerString(GameController.Instance.quickestNabEatenBy, neutralDarkTag);
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
			string mostFoodsEatenBy = GetFormattedPlayerString(mostFoodsEatenByList, neutralDarkTag);
			List<Player> leastFoodsEatenByList = new List<Player>();
			for (int i = foodSortedPlayers.Count - 1; i >= 0; i--)
			{
				if(foodSortedPlayers[i].plate.foods.Count == leastFoodsEaten)
				{
					leastFoodsEatenByList.Add (foodSortedPlayers[i]);
				} else {
					break;
				}
			}
			string leastFoodsQuantity = leastFoodsEaten.ToString();
			string leastFoodsEatenBy = GetFormattedPlayerString(leastFoodsEatenByList, neutralDarkTag);

			//Combine dynamic strings with static structure to produce output string
			string outputString = string.Format(
				neutralDarkTag+
				"Tastiest Food - {0} ({1}) - {2}\n"+
				neutralDarkTag+
				"Grossest Food - {3} ({4}) - {5}\n"+
				neutralDarkTag+
				"Quickest Nab - {6} - {8} in {7} seconds\n"+
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

	private string GetFormattedPlayerString(Player player, string closingHexTag)
	{
		if(player != null)
		{
		return HexTag(player.PanelScript.PlayerColorScheme.defaultColor)+player.ProfileInstance.playerName + closingHexTag;
		} else {
			Debug.LogError("No player to format @"+Time.frameCount);
			return "";
		}
	}

	private string GetFormattedPlayerString(List<Player> players, string closingHexTag)
	{
		string outputString = "";
		for(int i = 0; i < players.Count; i++)
		{
			Player player = players[i];

			outputString += HexTag(player.PanelScript.PlayerColorScheme.defaultColor)+player.ProfileInstance.playerName;

			if(i != players.Count - 1)
			{
				outputString += " ";
			} else {
				outputString += closingHexTag;
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
		if(player.controlType == ControlType.Human)
		{
			player.humanButton.color = player.PanelScript.PlayerColorScheme.highlightedColor;
			player.computerButton.color = neutralLightColor;
			player.humanButtonLabel.color = neutralLightColor;
			player.computerButtonLabel.color = neutralDarkColor;
		} else if(player.controlType == ControlType.Computer)
		{
			player.computerButton.color = player.PanelScript.PlayerColorScheme.highlightedColor;
			player.humanButton.color = neutralLightColor;
			player.computerButtonLabel.color = neutralLightColor;
			player.humanButtonLabel.color = neutralDarkColor;
		} else {
			Debug.LogError ("Something bad happened");
		}
	}

	public void WriteWinner(Player player)
	{
		winString = player.ProfileInstance.playerName + " wins with "+player.Score+" Points!";
		
	}

	public void InitializeInterface()
	{
		//print ("count" + GameController.Instance.PossiblePlayers.Count);
		for(int i = 0; i < GameController.Instance.PossiblePlayers.Count; i++)
		{
			//AcquirePlayerObjects();

			Player player = GameController.Instance.PossiblePlayers[i];
			player.EnableUi();

			//Start player FSMs
			print ("setting init player state" + player.Id);
			SetPlayerUiState(player, PlayerUiState.Join);
			
			//Unready players
			player.playerChoice = PlayerChoice.Inactive;
			
			
		}

		SetGameUiState(GameUiState.Join);

		SetPopupUiState(PopupUiState.NoPopup);
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

	public static Player GetPlayerFromParentRecursively(Transform transform) {
		if(transform == transform.root) {
			Debug.LogError("Player not found.");
			return null;
		}
		
		if(transform.parent.name.Contains("PanelPlayer")) {
			int playerId = int.Parse(transform.parent.name.Remove(0, 11));
			return GameController.Instance.PossiblePlayers[playerId];
		} else {
			return GetPlayerFromParentRecursively(transform.parent);
		}
	}

}
