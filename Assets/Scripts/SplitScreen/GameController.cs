using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Phase
{
	GameStart = 0,
	Choose = 1,
	Evaluate = 2,
	FinalScoring = 3,
	GameOver = 4,
	Pregame = 5
}

public class GameController : MonoBehaviour
{
	private static GameController _instance;
	public static GameController Instance {
		get {
			return _instance ?? GameObject.FindObjectOfType<GameController>();
		}
	}

	//Rules
	public Restaurant activeRestaurant;
	public int numberOfRounds = 10;
	public int servingsPerFood = 2;
	public int forcedEaters = 1;
	public int startingHp = 100;

	//Configurable
	public float damageConstant = 10f;
	//public Color[] letterRankColors = new Color[7];

	//Generated
	//public static float ScoreConstant = 25f;
	private float scoreWobble = 10f;
	public static float RandomConstant = 0f;

	//Cached
	public List<Player> PossiblePlayers = new List<Player> ();
	public List<Player> registeredPlayers = new List<Player> ();
	public List<Player> activePlayers = new List<Player> ();

	//Status
	public int currentRound = -1;
	public float choiceStartTime = -1;

	//public int confirmedPlayers = 0;
	public List<Player> humanPlayers = new List<Player> ();
	public Phase currentPhase = Phase.GameStart;
	public Food activeFood;
	public Food previousFood;
	//public Dictionary<int, bool> playerChoices = new Dictionary<int, bool>(); //True indicates "eat"

	//Match Stats
	private Food _tastiestFood;
	public Food TastiestFood {
		get {
			return _tastiestFood;
		}
		private set {
			_tastiestFood = value;
			print ("tastiest food set to: " + value);
		}
	}
	public List<Player> tastiestFoodEatenBy;
	public Food grossestFood = null;
	public List<Player> grossestFoodEatenBy;
	public Food quickestNab;
	public float quickestNabTime;
	public Player quickestNabEatenBy;

	//Debug
	//public int trials = 1000;

	void Awake ()
	{
		_instance = this;
		Init();
	}

	public void Init() {
		
		PossiblePlayers = GetComponentsInChildren<Player> ().ToList ();
		
		InterfaceController.Instance.InitializeInterface ();
	}

	void Start ()
	{
//		print (Application.persistentDataPath);
//		Advertisement.Initialize ("18656");

		currentPhase = Phase.Pregame;
		UserDatabase.Instance.LoadUserData ();
	}

	void Update ()
	{

		if (currentPhase == Phase.Choose) {
			var query = from player in activePlayers
				where player.playerChoice != PlayerChoice.Ready
					select player;
			if ((query.Count () == activePlayers.Count ())) {
				EvaluateRound ();
			} else if (query.Count () > activePlayers.Count ()) {
				Debug.LogError ("Something terrible has happened");
			}

			IEnumerable<Player> eatingPlayersQuery = from player in activePlayers
				where player.playerChoice == PlayerChoice.Eat
					select player;
			if (eatingPlayersQuery.Count () >= servingsPerFood) {
				IEnumerable<Player> undecidedPlayersQuery = from player in activePlayers
					where player.playerChoice == PlayerChoice.Ready
						select player;
				foreach (Player passingPlayer in undecidedPlayersQuery) {
					passingPlayer.Pass ();
				}
			}

			var passingPlayersQuery = from player in activePlayers
				where player.playerChoice == PlayerChoice.Pass
					select player;
			int maxPassers = activePlayers.Count - forcedEaters > 0 ? activePlayers.Count - forcedEaters : 1;
			if (passingPlayersQuery.Count () >= maxPassers) {
				var undecidedPlayersQuery = from player in activePlayers
					where player.playerChoice == PlayerChoice.Ready
						select player;
				foreach (Player eatingPlayer in undecidedPlayersQuery) {
					eatingPlayer.Eat ();
				}
			}
		}
	}



	public void BeginGame ()
	{
		currentPhase = Phase.GameStart;
		currentRound = -1;
		//Register players
		RegisterPlayers ();

		InitializeMatchStats ();


		//Set game ui state
		InterfaceController.Instance.SetGameUiState (GameUiState.MainGame);

		if (activePlayers.Count == 0 || activePlayers.Count > 4) {
			Debug.LogError ("Invalid number of players: " + activePlayers.Count);
		}
	
		//Initialize game
		BeginRound ();
	}

	public void BeginRound ()
	{

		currentRound ++;
		Debug.Log ("Begin round: " + currentRound);
		//Increment round

		InterfaceController.Instance.DisplayRound ();

		//Reset choices
		List<Player> tempPlayers = new List<Player> (activePlayers);
		foreach (Player player in tempPlayers) {
			if (player.Hp <= 0) {
				activePlayers.Remove (player);
				player.EnableButtons (false);
			}
			player.playerChoice = PlayerChoice.Ready;
		}

		//Enable next buttons
		InterfaceController.Instance.EnableNextButtons (false);


		//Set score constant for next food
		RandomConstant = ((Random.value * scoreWobble) - scoreWobble / 2) + GameData.SCORE_CONSTANT; //TODO Restore randomness!

		NextFood ();
	}

	public void UpdatePlayerStats ()
	{
		foreach (Player player in activePlayers) {
			//player.updateScoreLabel.text = "";
			player.Score += player.PendingScore;
			player.PendingScore = 0f;
			//player.updateHpLabel.text = "";
			player.Hp += player.PendingHp;
			player.PendingHp = 0f;
		}

		//Set ranking
		Player[] playersByScore = ((from player in registeredPlayers
		                            select player).OrderByDescending (player => player.Score)).ToArray ();
		string debugString = "";
		for (int k = 0; k < playersByScore.Length; k++) {
			debugString += playersByScore [k].name + ": " + playersByScore [k].Score + ", ";
		}
		Debug.Log (debugString);
		int nextRanking = 0;
		for (int i = 0; i < playersByScore.Count(); i++) {
			Player[] matchingScoresQuery = (from player in playersByScore
			                                where player != playersByScore [i] && player.Score == playersByScore [i].Score
			                                select player).ToArray ();
			if (matchingScoresQuery.Length <= 0) {
				//Debug.Log ("No matching");
				playersByScore [i].Ranking = nextRanking;
				//Debug.Log ("Player" + playersByScore [i].playerId + " set to " + nextRanking);
				nextRanking++;
			} else {
				//Debug.Log ("Matches.");
				playersByScore [i].Ranking = nextRanking;
				for (int j = 0; j < matchingScoresQuery.Length; j++) {
					matchingScoresQuery [j].Ranking = nextRanking;
					//Debug.Log ("Player" + playersByScore [i].playerId + " set to " + nextRanking);
					i++;
				}
				nextRanking++;
			}
		}
	}

	public void NextFood ()
	{
		//Debug.Log ("Next food for: " + currentRound);	
		if (activeFood != null) {
			previousFood = activeFood;
		}
		activeFood = FoodLogic.Instance.GetRandomFoodUsingQueue ();
		InterfaceController.Instance.DisplayPrompt (activeFood.Name);

		currentPhase = Phase.Choose;

		foreach (Player player in activePlayers) {
			if (player.controlType == ControlType.Human) {
				player.EnableButtons (true);
			} else {
				player.EnableButtons (false);
			}
		}

		choiceStartTime = Time.time;
	}

	public void EvaluateRound ()
	{
		Debug.Log ("Evaluate round: " + currentRound + " @" + Time.frameCount);
		currentPhase = Phase.Evaluate;

		//Show food ranking
		InterfaceController.Instance.ShowFoodRank (GetFoodRank (activeFood));
		
		List<Player> eatingPlayers = new List<Player> ();
		Player quickestPlayerOfRound = null;
		for (int i = 0; i < activePlayers.Count; i++) {
			Player player = activePlayers [i];
			//Debug.Log ("Player id: "+player.playerId);
			if (player.playerChoice == PlayerChoice.Eat) { //if player chose to eat
				eatingPlayers.Add (player);
				if (quickestPlayerOfRound == null) {
					Debug.Log (player + " assigned as first quickest player of round. @" + Time.frameCount);
					quickestPlayerOfRound = player;
				} else {
					//Debug.Log (quickestPlayerOfRound);
					//Debug.Log (player);
					//Debug.Log (quickestPlayerOfRound.lastChoiceTimeElapsed);
					//Debug.Log (player.lastChoiceTimeElapsed);
					if (player.lastChoiceTimeElapsed < quickestPlayerOfRound.lastChoiceTimeElapsed) {
						Debug.Log ("i != 0. Quickest player assigned: " + player + "@" + Time.frameCount);
						quickestPlayerOfRound = player;
					}
				}

				//Score
				//Debug.Log ("Update score");
				float qualityFloat = activeFood.Quality.Value;
				player.PendingScore = qualityFloat;
				//Health
				float hpFloat = -activeFood.Damage;
				player.PendingHp = hpFloat;
				//Debug.Log ("Pending hp: " + player.PendingHp);

				//Ate, update profile
				player.ProfileInstance.foodsEaten++;
				if (player.ProfileInstance.tastiestFoodEaten == null || activeFood.Quality > player.ProfileInstance.tastiestFoodEaten.Quality) {
					player.ProfileInstance.tastiestFoodEaten = activeFood;
				} 
				if (player.ProfileInstance.grossestFoodEaten == null || activeFood.Quality < player.ProfileInstance.grossestFoodEaten.Quality) {
					player.ProfileInstance.grossestFoodEaten = activeFood;
				}

			} else {

				//Didn't eat, update profile
				if (player.ProfileInstance.tastiestFoodMissed == null || activeFood.Quality > player.ProfileInstance.tastiestFoodMissed.Quality) {
					player.ProfileInstance.tastiestFoodMissed = activeFood;
				}
				if (player.ProfileInstance.grossestFoodMissed == null || activeFood.Quality < player.ProfileInstance.grossestFoodMissed.Quality) {
					player.ProfileInstance.grossestFoodMissed = activeFood;
				}
			}
		}
		UpdateRunningStats(eatingPlayers, quickestPlayerOfRound);

		Debug.Log ("Evaluation ended for round" + currentRound + " @" + Time.frameCount);


		InterfaceController.Instance.EnableNextButtons (true);
	}

	void UpdateRunningStats(List<Player> eatingPlayers, Player quickestPlayerOfRound) {
		print ("Update running stats");
		if (eatingPlayers.Count > 0) {
			print ("Eating players > 0");
			if (TastiestFood == null) { //If no foods have been eaten previously
				Debug.Log ("No foods eaten previously" + Time.frameCount);
				TastiestFood = activeFood;
				tastiestFoodEatenBy = eatingPlayers;
				grossestFood = activeFood;
				grossestFoodEatenBy = eatingPlayers;
				quickestNab = activeFood;
				quickestNabEatenBy = quickestPlayerOfRound;
				quickestNabTime = quickestPlayerOfRound.lastChoiceTimeElapsed;
				quickestNab = activeFood;
			} else {
				if (activeFood.Quality > TastiestFood.Quality) {
					TastiestFood = activeFood;
					tastiestFoodEatenBy = eatingPlayers;
				}
				if (activeFood.Quality < grossestFood.Quality) {
					grossestFood = activeFood;
					grossestFoodEatenBy = eatingPlayers;
				}
				if (quickestPlayerOfRound.lastChoiceTimeElapsed < quickestNabTime) {
					Debug.Log ("Replacing old quickest time: " + quickestNabEatenBy.ProfileInstance.playerName + ", " + quickestNabTime + ", " + quickestNab.Name + " @" + Time.frameCount);
					Debug.Log ("New fastest time: " + quickestPlayerOfRound.ProfileInstance.playerName + " @" + quickestPlayerOfRound.lastChoiceTimeElapsed);
					quickestNabEatenBy = quickestPlayerOfRound;
					quickestNabTime = quickestPlayerOfRound.lastChoiceTimeElapsed;
					quickestNab = activeFood;
				}
			}
		}
	}

	void RegisterPlayers ()
	{
		Debug.Log ("Register players");
		//Log human players

		registeredPlayers = (from player in GetComponentsInChildren<Player> ()
			where player.playerChoice == PlayerChoice.Ready
				select player).ToList ();
		activePlayers = new List<Player> (registeredPlayers);
		foreach (Player player in registeredPlayers) {
			InterfaceController.SetPlayerUiState (player, PlayerUiState.Game);
			player.playerNameLabelGame.text = player.ProfileInstance.playerName;
			player.playedInLastGame = true;

		}
		foreach (Player player in PossiblePlayers) {
			if (player.playerChoice != PlayerChoice.Ready) {
				InterfaceController.SetPlayerUiState (player, PlayerUiState.Inactive);
				player.playedInLastGame = false;
			}

		}


		for (int i = 0; i < activePlayers.Count (); i++) {
			//Log as human 
			if (activePlayers [i].controlType == ControlType.Human) {
				humanPlayers.Add (activePlayers [i]);
			}

			//Set starting stats
			activePlayers [i].Hp = startingHp;
			activePlayers [i].Score = 0;
			activePlayers [i].plate.foods.Clear ();

		}
	}

	public void EndRound ()
	{
		InterfaceController.Instance.HideFoodRank ();
		InterfaceController.Instance.HidePrompts ();

		//Update score health and ranking
		UpdatePlayerStats ();

		Debug.Log ("Ending round " + currentRound);
		if (currentRound < numberOfRounds - 1) {
			BeginRound ();
		} else {
			EndGame ();
		}
	}



	public void EndGame ()
	{
		currentPhase = Phase.GameOver;

		//Show ad before revealing score
//		if (InterfaceController.Instance.adsEnabled) {
//			if (Advertisement.isReady ()) {
//				Advertisement.Show ();
//			}
//		}


		//Determine winner
		Player[] winQuery = registeredPlayers.OrderByDescending (player => player.Score).ToArray ();
//		foreach(Player player in registeredPlayers)
//		{
//			player.playerChoice = PlayerChoice.Inactive;
//		}
		List<PlayerResult> outputRecords = new List<PlayerResult> ();
		foreach (Player player in registeredPlayers) {
			PlayerResult record = new PlayerResult ();
			record.playerStringId = player.ProfileInstance.playerName;
			record.rank = player.Ranking;
			record.score = player.Score;
			record.remainingHp = player.Hp;
			record.gameId = UserDatabase.Instance.userInfo.totalGamesPlayed;
			outputRecords.Add (record);

			//UI
			InterfaceController.Instance.WriteWinner (winQuery [0]);
//			StartCoroutine (
			DisplayWinner ();
//				);

			//Update profile stats
			player.ProfileInstance.gamesPlayed++;
			player.ProfileInstance.lifetimeScore += player.Score;
			if (player.Score > player.ProfileInstance.bestScore || player.ProfileInstance.gamesPlayed == 1) {
				player.ProfileInstance.bestScore = player.Score;
			}
			if (player.Score < player.ProfileInstance.worstScore || player.ProfileInstance.gamesPlayed == 1) {
				player.ProfileInstance.worstScore = player.Score;
			}
		}
		//Save entries to user database
		Debug.Log ("Writing to user database, output: " + outputRecords.Count);

		UserDatabase.Instance.LogGame (outputRecords);

		ResetAndHideGame ();

		currentPhase = Phase.GameOver;
	}

	void DisplayWinner ()
//	IEnumerator DisplayWinner ()
	{
//		while (Advertisement.isShowing) {
//			yield return 0;
//		}

		InterfaceController.Instance.SetGameUiState (GameUiState.Results);
	}
	
	public LetterRank GetFoodRank (Food food)
	{
		List<float> percentiles = FoodMetadata.Instance.percentiles;
		for (int i = 0; i < percentiles.Count; i++) {
			if (food.Quality > percentiles [i]) {
				return (LetterRank)i;
			}
		}
		return LetterRank.F;
	}

	public void ReadyJoinedPlayers ()
	{
		foreach (Player player in PossiblePlayers) {
			if (player.playedInLastGame) {
				player.playerChoice = PlayerChoice.Ready;
			}
		}
	}

	public void TerminateGame ()
	{
		ResetAndHideGame ();
		//Update player stats to clear hovering text

		currentPhase = Phase.Pregame;
	}

	public void ResetAndHideGame ()
	{
		InterfaceController.Instance.HideFoodRank ();
		InterfaceController.Instance.HidePrompts ();
		UpdatePlayerStats ();
		activePlayers.Clear ();
		humanPlayers.Clear ();

	}

	public void InitializeMatchStats ()
	{
		print ("Initializing match stats");
		TastiestFood = null;
		tastiestFoodEatenBy = new List<Player> ();
		grossestFood = null;
		grossestFoodEatenBy = new List<Player> ();
		quickestNab = null;
		quickestNabTime = -1F;
		quickestNabEatenBy = null;
	}
}
