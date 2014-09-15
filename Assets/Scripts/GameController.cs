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

//public enum FoodAttributeType
//{
//	None = 0,
//	Quality = 1,
//	Ingredient = 2,
//	Form = 3
//}

public class GameController : MonoBehaviour {

	public static GameController Instance;

	//Configurable
	public Restaurant activeRestaurant;
	public int numberOfRounds = 1;
	public int servingsPerFood = 2;
	public int startingHp = 100;
	public float damageConstant = 10f;
	//public Color[] letterRankColors = new Color[7];

	//Generated
	public static float ScoreConstant = 25f;
	private float scoreWobble = 10f;
	public static float RandomConstant = 0f;

	//Cached
	public List<Player> possiblePlayers = new List<Player>();
	public List<Player> registeredPlayers = new List<Player>();
	public List<Player> activePlayers = new List<Player>();
	public List<FoodAttribute> qualifierQueue;
	public List<FoodAttribute> ingredientQueue;
	public List<FoodAttribute> formQueue;

	//Status
	public int currentRound = -1;

	//public int confirmedPlayers = 0;
	public List<Player> humanPlayers = new List<Player>();
	public Phase currentPhase = Phase.GameStart;
	public Food activeFood;
	public Food previousFood;
	//public Dictionary<int, bool> playerChoices = new Dictionary<int, bool>(); //True indicates "eat"

	//Debug
	public int trials = 1000;

	void Awake()
	{
		Instance = this;

		qualifierQueue = GetShuffledAttributes(AttributeType.Qualifier);
		ingredientQueue = GetShuffledAttributes(AttributeType.Ingredient);
		formQueue = GetShuffledAttributes(AttributeType.Form);

		possiblePlayers = GetComponentsInChildren<Player>().ToList();
	}

	// Use this for initialization
	void Start () {
	

		currentPhase = Phase.Pregame;
		UserDatabase.Instance.LoadUserData();
		InterfaceController.Instance.InitializeInterface();


	//	players = 


		//qualifierQueue

		                        //Acquire objects
		GameObject camera = GameObject.Find ("UI Root/Camera");

	

		//BeginGame();

		//GET AVERAGE SCORE
//		for(int i = 0; i < numberOfTrials; i++)
//		{
//			Player.Instance.Eat();
//		}
//		Debug.Log ("Average score: "+Player.Instance.score / numberOfTrials);

		//Runtrials ();

	}
	
	// Update is called once per frame
	void Update () {
	
		//Debug
		if (Input.GetKeyUp (KeyCode.Alpha1)) {
			RunTrials();
				}


		if(currentPhase == Phase.Choose)
		{
			var query = from player in activePlayers
				where player.playerChoice != PlayerChoice.Ready
					select player;
			if((query.Count() == activePlayers.Count()))
		{
				EvaluateRound();
		} else if(query.Count () > activePlayers.Count ())
			{
				Debug.Log("Something terrible has happened");
			}

			//Run out of food after 2
			int eatCounter = 0;
			List<Player> eatingPlayers = new List<Player>();

			var eatingPlayersQuery = from player in activePlayers
				where player.playerChoice == PlayerChoice.Eat
					select player;
			if(eatingPlayersQuery.Count() >= servingsPerFood)
			{
				var undecidedPlayersQuery = from player in activePlayers
					where player.playerChoice == PlayerChoice.Ready
						select player;
				foreach(Player passingPlayer in undecidedPlayersQuery)
				{
					passingPlayer.Pass ();
				}
			}

			int passCounter = 0;
			List<Player> passingPlayers = new List<Player>();
			var passingPlayersQuery = from player in activePlayers
				where player.playerChoice == PlayerChoice.Pass
					select player;
			if(passingPlayersQuery.Count () >= activePlayers.Count - 1)
			{
				var undecidedPlayersQuery = from player in activePlayers
					where player.playerChoice == PlayerChoice.Ready
						select player;
				foreach(Player eatingPlayer in undecidedPlayersQuery)
				{
					eatingPlayer.Eat();
				}
			}

//			foreach(KeyValuePair<int, bool> entry in playerChoices)
//			{
//				if(entry.Value)
//				{
//					eatCounter++;
//					eatingPlayers.Add (players[entry.Key]);
//
//				}
//			}
//			if(eatCounter >= servingsPerFood)
//			{
//				foreach(Player player in players)
//				{
//					if(!eatingPlayers.Contains(player) && !playerChoices.ContainsKey(player.playerId))
//					{
//						player.Pass ();
//					}
//				}
//				//EvaluateRound();
//				//break;
//			}

		}
	}

//	public Rank GetRandomRank()
//	{
//		Debug.Log ("Get random rank @" + currentRound);
//		Restaurant restaurant = activeRestaurant;
//
//		Dictionary<Rank, float> probabilityTable = new Dictionary<Rank, float>();
//		float rankCutoff = 0f;
//		for(int i = 0; i < restaurant.rankPercentages.Length; i++)
//		{
//			probabilityTable.Add ((Rank)i+1, restaurant.rankPercentages[i] + rankCutoff);
//			rankCutoff += restaurant.rankPercentages[i];
//		}
//
//		float dieRoll = Random.value * rankCutoff;
//		for(int i = 0; i < probabilityTable.Count; i++)
//		{
//			if(dieRoll <= probabilityTable[(Rank)i + 1])
//			{
//				return (Rank)i +1;
//			}
//		}

//		Debug.LogError("Rank out of range");
//		return Rank.None;
//	}


	public FoodAttribute GetRandomAttributeFromQueue (AttributeType attributeType)
	{
		//Debug.Log ("GetAttribute, rank, type, round: " + rank + ", " + attributeType + ", " + currentRound);
		//IEnumerable<FoodAttribute> query = null;
		FoodAttribute attribute = null;
		int newIndex;
		switch(attributeType)
		{
		case AttributeType.Qualifier:
			 attribute = qualifierQueue[0];
			newIndex = qualifierQueue.Count - 1;
			qualifierQueue.RemoveAt(0);
			if (newIndex > 0) newIndex--; 
			qualifierQueue.Insert(newIndex, attribute);
//			query = from attribute in Database.Instance.attributeData
//				where attribute.rank == rank && attribute.attributeType == AttributeType.Qualifier
//					select attribute;
			break;
		case AttributeType.Ingredient:
			attribute = ingredientQueue[0];
			newIndex = ingredientQueue.Count - 1;
			ingredientQueue.RemoveAt(0);
			if (newIndex > 0) newIndex--; 
			ingredientQueue.Insert(newIndex, attribute);
//			query = from attribute in Database.Instance.attributeData
//				where attribute.rank == rank && attribute.attributeType == AttributeType.Ingredient
//					select attribute;
			break;
		case AttributeType.Form:
			attribute = formQueue[0];
			newIndex = formQueue.Count - 1;
			formQueue.RemoveAt(0);
			if (newIndex > 0) newIndex--; 
			formQueue.Insert(newIndex, attribute);
//			query = from attribute in Database.Instance.attributeData
//				where attribute.rank == rank && attribute.attributeType == AttributeType.Form
//					select attribute;
			break;
		default:
			Debug.LogError ("Invalid attribute type: "+attributeType);
			break;
		}
		return attribute;

//		FoodAttribute[] possibleAttributes = query.ToArray();
//		if(possibleAttributes.Length > 0)
//		{
//		return possibleAttributes[(int)Mathf.Floor (Random.value * possibleAttributes.Length)];
//		} else {
//		Debug.Log("No "+attributeType+"s of Rank "+rank+" found.");
//			FoodAttribute subAttribute = new FoodAttribute();
//			subAttribute.name = "ERROR";
//			return subAttribute;
//		}
	}

	public static FoodAttribute GetRandomAttributeFromData (AttributeType attributeType)
	{
		//Debug.Log ("GetAttribute, rank, type, round: " + rank + ", " + attributeType + ", " + currentRound);
		//IEnumerable<FoodAttribute> query = null;
		FoodAttribute attribute = null;
		FoodAttribute[] query;
		int newIndex;

		switch(attributeType)
		{
		case AttributeType.Qualifier:
			 query = (from element in Database.Instance.attributeData
				where element.attributeType == AttributeType.Qualifier
					select element).ToArray();
			attribute = query[Mathf.FloorToInt(Random.value * query.Count ())];
			break;
		case AttributeType.Ingredient:
			 query = (from element in Database.Instance.attributeData
			             where element.attributeType == AttributeType.Ingredient
			             select element).ToArray();
			attribute = query[Mathf.FloorToInt(Random.value * query.Count ())];
			break;
		case AttributeType.Form:
			 query = (from element in Database.Instance.attributeData
			             where element.attributeType == AttributeType.Form
			             select element).ToArray();
			attribute = query[Mathf.FloorToInt(Random.value * query.Count ())];
			break;
		default:
			Debug.LogError ("Invalid attribute type: "+attributeType);
			break;
		}
		return attribute;
		
		//		FoodAttribute[] possibleAttributes = query.ToArray();
		//		if(possibleAttributes.Length > 0)
		//		{
		//		return possibleAttributes[(int)Mathf.Floor (Random.value * possibleAttributes.Length)];
		//		} else {
		//		Debug.Log("No "+attributeType+"s of Rank "+rank+" found.");
		//			FoodAttribute subAttribute = new FoodAttribute();
		//			subAttribute.name = "ERROR";
		//			return subAttribute;
		//		}
	}

	//	public Quality GetRandomQuality (Rank rank)
	//	{
	//			var query = from quality in Database.Instance.qualities
	//				where quality.rank == rank
	//					select quality;
	//			Quality[] possibleQualities = query.ToArray();
	//		if(possibleQualities.Length > 0)
	//		{
	//		return possibleQualities[(int)Mathf.Floor (Random.value * possibleQualities.Length)];
	//		} else {
	//			Debug.LogError("No qualities of rank "+rank+" found.");
	//			return new Quality();
	//		}
	//	}
	//
//
//	public Form GetRandomForm (Rank rank)
//	{
//		var query = from form in Database.Instance.forms
//			where form.rank == rank
//				select form;
//		Form[] possibleForms = query.ToArray();
//		if(possibleForms.Length > 0)
//		{
//		return possibleForms[(int)Mathf.Floor (Random.value * possibleForms.Length)];
//} else {
//	Debug.LogError("No forms of rank "+rank+" found.");
//	return new Form();
//}
//	}

	public Food GetRandomFoodUsingQueue() //During runtime; for game use
	{
		//Debug.Log ("Get random food @" + currentRound);
		Food food = new Food();

		food.attributes.Add (GetRandomAttributeFromQueue(AttributeType.Form));
		food.attributes.Add (GetRandomAttributeFromQueue(AttributeType.Ingredient));
		food.attributes.Add (GetRandomAttributeFromQueue(AttributeType.Qualifier));
		food.Realize(true);
//
		return food;
	}

	public static Food GetRandomFoodUsingData() //Pre-runtime; for statistical use
	{
		//Debug.Log ("Get random food @" + currentRound);
		Food food = new Food();
		
		food.attributes.Add (GetRandomAttributeFromData(AttributeType.Form));
		food.attributes.Add (GetRandomAttributeFromData(AttributeType.Ingredient));
		food.attributes.Add (GetRandomAttributeFromData(AttributeType.Qualifier));
		food.Realize(true);
		//
		return food;
	}

	public void BeginGame()
	{
		currentPhase = Phase.GameStart;
		currentRound = -1;
	//Register players
	RegisterPlayers();


		//Set game ui state
		InterfaceController.Instance.SetGameUiState(GameUIState.MainGame);

		if(activePlayers.Count == 0 || activePlayers.Count > 4)
		{
			Debug.LogError ("Invalid number of players: "+activePlayers.Count);
		}
	
	//Initialize game
	BeginRound();
	}

	public void BeginRound()
	{

		currentRound ++;
		Debug.Log ("Begin round: " + currentRound);
		//Increment round

		InterfaceController.Instance.DisplayRound();





		//Reset choices
		List<Player> tempPlayers = new List<Player>(activePlayers);
		foreach(Player player in tempPlayers)
		{
			if(player.Hp <= 0)
			{
				activePlayers.Remove(player);
				player.EnableButtons(false);
			}
			player.playerChoice = PlayerChoice.Ready;
		}

		//Enable next buttons
		InterfaceController.Instance.EnableNextButtons(false);


		//Set score constant for next food
		RandomConstant = ((Random.value * scoreWobble)-scoreWobble/2) + ScoreConstant;
		Debug.Log ("Random constant: "+RandomConstant +", Score Constant"+ScoreConstant);

		NextFood ();
	}

	public void UpdatePlayerStats()
	{
		foreach(Player player in activePlayers)
		{
			//player.updateScoreLabel.text = "";
			player.Score += player.PendingScore;
			player.PendingScore = 0f;
			//player.updateHpLabel.text = "";
			player.Hp += player.PendingHp;
			player.PendingHp = 0f;
		}

		//Set ranking
		Player[] playersByScore = ((from player in registeredPlayers
		                            select player).OrderByDescending(player => player.Score)).ToArray();
		string debugString = "";
		for(int k = 0; k < playersByScore.Length; k++)
		{
			debugString += playersByScore[k].name +": "+playersByScore[k].Score+", ";
		}
		Debug.Log (debugString);
		int nextRanking = 0;
		for(int i = 0; i < playersByScore.Count(); i++)
		{
			Player[] matchingScoresQuery = (from player in playersByScore
			                                where player != playersByScore[i] && player.Score == playersByScore[i].Score
			                                select player).ToArray();
			if(matchingScoresQuery.Length <= 0)
			{
				Debug.Log ("No matching");
				playersByScore[i].Ranking = nextRanking;
				Debug.Log ("Player"+playersByScore[i].playerId+" set to "+nextRanking);
				nextRanking++;
			} else {
				Debug.Log ("Matches.");
				playersByScore[i].Ranking = nextRanking;
				for(int j = 0; j < matchingScoresQuery.Length; j++)
				{
					matchingScoresQuery[j].Ranking = nextRanking;
					Debug.Log ("Player"+playersByScore[i].playerId+" set to "+nextRanking);
					i++;
				}
				nextRanking++;
			}
		}
	}

	public void NextFood()
	{
		Debug.Log ("Next food for: " + currentRound);	
		if(activeFood != null)
		{
			previousFood = activeFood;
		}
		activeFood = GetRandomFoodUsingQueue();
		InterfaceController.Instance.DisplayPrompt(
			//"You encounter: "+
		                                           //"\n "+
		                                           activeFood.Name);
		//InterfaceController.Instance.WriteToOutcome("");
		//InterfaceController.Instance.WriteToScore(Player.Instance.score);
		//EvaluateRound();
		currentPhase = Phase.Choose;

		foreach(Player player in activePlayers)
		{
			player.EnableButtons(true);
		}
	}

	public void EvaluateRound()
	{
		Debug.Log ("Evaluate round: " + currentRound + " @"+Time.frameCount);
		currentPhase = Phase.Evaluate;

		//Show food ranking
		InterfaceController.Instance.ShowFoodRank(GetFoodRank(activeFood));

		foreach(Player player in activePlayers)
		{
			//Debug.Log ("Player id: "+player.playerId);
			if(player.playerChoice == PlayerChoice.Eat) //if player chose to eat
			{
				//Score
				Debug.Log ("Update score");
				float qualityFloat = activeFood.Quality;
				//string qualityString = qualityFloat >= 0 ? "+" + qualityFloat.ToString ("F0"): qualityFloat.ToString("F0");
				//player.updateScoreLabel.text = qualityString;
				player.PendingScore = qualityFloat;
				//Health
				float hpFloat = -activeFood.Damage;
				//string hpString = hpFloat < 0 ? hpFloat.ToString ("F0"): "";
				//player.updateHpLabel.text = hpString;
				player.PendingHp = hpFloat;
				Debug.Log ("Pending hp: "+player.PendingHp);

				//Eat, update profile
				player.profileInstance.foodsEaten++;
				if(activeFood.Quality > player.profileInstance.tastiestFoodEaten.Quality || player.profileInstance.tastiestFoodEaten.attributes.Count == 0)
				{
					player.profileInstance.tastiestFoodEaten = activeFood;
				} 
				if (activeFood.Quality < player.profileInstance.grossestFoodEaten.Quality || player.profileInstance.grossestFoodEaten.attributes.Count == 0)
				{
					player.profileInstance.grossestFoodEaten = activeFood;
				}

			} else {

				//Didn't eat, update profile
				if(activeFood.Quality > player.profileInstance.tastiestFoodMissed.Quality  || player.profileInstance.tastiestFoodMissed.attributes.Count == 0)
				{
					player.profileInstance.tastiestFoodMissed = activeFood;
				}
				if (activeFood.Quality < player.profileInstance.grossestFoodMissed.Quality || player.profileInstance.grossestFoodMissed.attributes.Count == 0)
				{
					player.profileInstance.grossestFoodMissed = activeFood;
				}
			}
		}
		Debug.Log ("Evaluation ended for round"+currentRound+" @"+Time.frameCount);


		InterfaceController.Instance.EnableNextButtons(true);
		//Debug
		//EndRound ();
	}

	void RegisterPlayers()
	{
		Debug.Log ("Register players");
		//Log human players

		registeredPlayers = (from player in GetComponentsInChildren<Player>()
			where player.playerChoice == PlayerChoice.Ready
				select player).ToList();
		activePlayers = new List<Player>(registeredPlayers);
		foreach(Player player in registeredPlayers)
		{
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Game);
			player.playerNameLabelGame.text = player.profileInstance.playerName;
			player.playedInLastGame = true;

		}
		foreach(Player player in possiblePlayers)
		{
			if(player.playerChoice != PlayerChoice.Ready)
			{
				InterfaceController.SetPlayerUiState(player, PlayerUiState.Inactive);
				player.playedInLastGame = false;
			}

		}


		for(int i = 0; i < activePlayers.Count (); i++)
		{
			//Log as human 
			if(activePlayers[i].controlType == ControlType.Human)
			{
				humanPlayers.Add(activePlayers[i]);
			}

			//Set identification
			//activePlayers[i].name = "Player "+i;
			//activePlayers[i].playerId = i;

			//Activate
			//activePlayers[i].isActive = true;

			//Set color
			//activePlayers[i].playerColor = (PlayerColor)i;

			//Set starting stats
			activePlayers[i].Hp = startingHp;
			activePlayers[i].Score = 0;

		}
	}

	public void EndRound()
	{
		InterfaceController.Instance.HideFoodRank();
		InterfaceController.Instance.HidePrompts();

		//Update score health and ranking
		UpdatePlayerStats();

		Debug.Log ("Ending round " + currentRound);
		if (currentRound < numberOfRounds - 1) {
						BeginRound ();
				} else {
			currentPhase = Phase.GameOver;
			EndGame();
				}
	}

	public void RunTrials()
	{
		//GET RANDOM FOOD N TIMES
		Dictionary<string, int> trialLog = new Dictionary<string, int>();
		for(int i = 0; i < trials; i++)
		{
			Food food = GetRandomFoodUsingQueue();
			if(trialLog.ContainsKey(food.Name))
			{
				trialLog[food.Name] += 1;
			} else {
				trialLog.Add (food.Name, 1);
			}
		}
		Dictionary<string, int> orderedLog = trialLog.OrderByDescending(trial => trial.Value).ToDictionary(trial => trial.Key, trial => trial.Value);
		foreach(KeyValuePair<string, int> pair in orderedLog)
		{
			Debug.Log (pair.Key + " appeared "+pair.Value+" times");
		}
	}

	List<FoodAttribute> GetShuffledAttributes(AttributeType type)
	{
		
		var query = from attribute in Database.Instance.attributeData
			where attribute.attributeType == type
				select attribute;
		List <FoodAttribute> attributes = query.ToList();
		Database.Shuffle(attributes);
		return attributes;
	}

	public void EndGame()
	{
		//Determine winner
		Player[] winQuery = registeredPlayers.OrderByDescending(player => player.Score).ToArray();
		InterfaceController.Instance.WriteWinner(winQuery[0]);
		InterfaceController.Instance.SetGameUiState(GameUIState.Results);
//		foreach(Player player in registeredPlayers)
//		{
//			player.playerChoice = PlayerChoice.Inactive;
//		}
		List<PlayerResult> outputRecords = new List<PlayerResult>();
		foreach(Player player in registeredPlayers)
		{
			PlayerResult record = new PlayerResult();
			record.playerStringId = player.profileInstance.playerName;
			record.rank = player.Ranking;
			record.score = player.Score;
			record.remainingHp = player.Hp;
			record.gameId = UserDatabase.Instance.userInfo.totalGamesPlayed;
			outputRecords.Add(record);

			//Update profile stats
			player.profileInstance.gamesPlayed++;
			player.profileInstance.lifetimeScore += player.Score;
			if(player.Score > player.profileInstance.bestScore  || player.profileInstance.gamesPlayed == 1)
			{
				player.profileInstance.bestScore = player.Score;
			}
			if(player.Score < player.profileInstance.worstScore || player.profileInstance.gamesPlayed == 1)
			{
				player.profileInstance.worstScore = player.Score;
			}

		}

		//Save entries to user database
		Debug.Log ("Writing to user database, output: "+outputRecords.Count);

		UserDatabase.Instance.LogGame(outputRecords);
		//UserDatabase.Instance.gameStats.totalGamesPlayed++;
		//outputRecords.AddRange(UserDatabase.Instance.PlayerGameRecords);
		//UserDatabase.Instance.PlayerGameRecords = outputRecords;


		activePlayers.Clear();
		humanPlayers.Clear();

	}

	public LetterRank GetFoodRank (Food food)
	{
		List<float> percentiles = Tools.Instance.percentiles;
		for(int i = 0; i < percentiles.Count; i++)
		{
			if(food.Quality > percentiles[i])
			{
				return (LetterRank)i;
			}
		}
		return LetterRank.F;
	}

	public void ReadyJoinedPlayers()
	{
		foreach(Player player in possiblePlayers)
		{
			if(player.playedInLastGame)
			{
			player.playerChoice = PlayerChoice.Ready;
			}
		}
	}


}
