using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Advertisements;
using Soomla.Store;

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
	public float choiceStartTime = -1;

	//public int confirmedPlayers = 0;
	public List<Player> humanPlayers = new List<Player>();
	public Phase currentPhase = Phase.GameStart;
	public Food activeFood;
	public Food previousFood;
	//public Dictionary<int, bool> playerChoices = new Dictionary<int, bool>(); //True indicates "eat"

	//Match Stats
	public Food tastiestFood;
	public List<Player> tastiestFoodEatenBy;
	public Food grossestFood = null;
	public List<Player> grossestFoodEatenBy;
	public Food quickestNab;
	public float quickestNabTime;
	public Player quickestNabEatenBy;

	//Debug
	//public int trials = 1000;

	void Awake()
	{
		Instance = this;

		qualifierQueue = GetShuffledAttributes(AttributeType.Qualifier);
		ingredientQueue = GetShuffledAttributes(AttributeType.Ingredient);
		formQueue = GetShuffledAttributes(AttributeType.Form);

		possiblePlayers = GetComponentsInChildren<Player>().ToList();

		InterfaceController.Instance.InitializeInterface();
	}

	// Use this for initialization
	void Start () {
	
		SoomlaStore.Initialize(new StoreController());
		StoreEvents.OnMarketPurchase += OnMarketPurchase;
		


		Advertisement.Initialize ("18656");
		currentPhase = Phase.Pregame;
		UserDatabase.Instance.LoadUserData();


	}
	
	// Update is called once per frame
	void Update () {
	
		//Debug
//		if (Input.GetKeyUp (KeyCode.Alpha1)) {
//			RunTrials();
//				}


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
				Debug.LogError("Something terrible has happened");
			}

			//Run out of food after 2
			//int eatCounter = 0;
			//List<Player> eatingPlayers = new List<Player>();

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

			//int passCounter = 0;
			//List<Player> passingPlayers = new List<Player>();
			var passingPlayersQuery = from player in activePlayers
				where player.playerChoice == PlayerChoice.Pass
					select player;
			int maxPassers = activePlayers.Count - forcedEaters > 0 ? activePlayers.Count - forcedEaters : 1;
			if(passingPlayersQuery.Count () >= maxPassers)
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
		//int newIndex;

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
		food.isEmpty = false;

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
		food.isEmpty = false;
		
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

		InitializeMatchStats();


		//Set game ui state
		InterfaceController.Instance.SetGameUiState(GameUiState.MainGame);

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
		InterfaceController.Instance.DisplayPrompt(activeFood.Name);

		currentPhase = Phase.Choose;

		foreach(Player player in activePlayers)
		{
			if(player.controlType == ControlType.Human)
			{
			player.EnableButtons(true);
			} else {
				player.EnableButtons(false);
			}
		}

		choiceStartTime = Time.time;
	}



	public void EvaluateRound()
	{
		Debug.Log ("Evaluate round: " + currentRound + " @"+Time.frameCount);
		currentPhase = Phase.Evaluate;

		//Show food ranking
		InterfaceController.Instance.ShowFoodRank(GetFoodRank(activeFood));
		
		List<Player> eatingPlayers = new List<Player>();
		Player quickestPlayerOfRound = null;
		for(int i = 0; i < activePlayers.Count; i++)
		{
			Player player = activePlayers[i];
			//Debug.Log ("Player id: "+player.playerId);
			if(player.playerChoice == PlayerChoice.Eat) //if player chose to eat
			{
				eatingPlayers.Add (player);
				if(quickestPlayerOfRound == null)
				{
					Debug.Log (player+" assigned as first quickest player of round. @"+Time.frameCount);
					quickestPlayerOfRound = player;
				} else {
					//Debug.Log (quickestPlayerOfRound);
					//Debug.Log (player);
					//Debug.Log (quickestPlayerOfRound.lastChoiceTimeElapsed);
					//Debug.Log (player.lastChoiceTimeElapsed);
					if(player.lastChoiceTimeElapsed < quickestPlayerOfRound.lastChoiceTimeElapsed)
					{
						Debug.Log ("i != 0. Quickest player assigned: "+player+"@"+Time.frameCount);
						quickestPlayerOfRound = player;
					}
				}

				//Score
				Debug.Log ("Update score");
				float qualityFloat = activeFood.Quality;
				player.PendingScore = qualityFloat;
				//Health
				float hpFloat = -activeFood.Damage;
				player.PendingHp = hpFloat;
				Debug.Log ("Pending hp: "+player.PendingHp);

				//Eat, update profile
				player.ProfileInstance.foodsEaten++;
				if(player.ProfileInstance.tastiestFoodEaten.isEmpty || activeFood.Quality > player.ProfileInstance.tastiestFoodEaten.Quality)
				{
					player.ProfileInstance.tastiestFoodEaten = activeFood;
				} 
				if (player.ProfileInstance.grossestFoodEaten.isEmpty || activeFood.Quality < player.ProfileInstance.grossestFoodEaten.Quality)
				{
					player.ProfileInstance.grossestFoodEaten = activeFood;
				}

			} else {

				//Didn't eat, update profile
				if(player.ProfileInstance.tastiestFoodMissed.isEmpty || activeFood.Quality > player.ProfileInstance.tastiestFoodMissed.Quality)
				{
					player.ProfileInstance.tastiestFoodMissed = activeFood;
				}
				if (player.ProfileInstance.grossestFoodMissed.isEmpty || activeFood.Quality < player.ProfileInstance.grossestFoodMissed.Quality)
				{
					player.ProfileInstance.grossestFoodMissed = activeFood;
				}
			}
		}

		//Update running stats for match
			if(eatingPlayers.Count > 0)
			{
				if(tastiestFood == null || tastiestFood.isEmpty) //If no foods have been eaten previously
				{
					tastiestFood = activeFood;
					tastiestFoodEatenBy = eatingPlayers;
					grossestFood = activeFood;
					grossestFoodEatenBy = eatingPlayers;
				Debug.Log("First assigned to quickest @"+Time.frameCount);
					quickestNab = activeFood;
					quickestNabEatenBy = quickestPlayerOfRound;
					quickestNabTime = quickestPlayerOfRound.lastChoiceTimeElapsed;
					quickestNab = activeFood;
				} else {
					if(activeFood.Quality > tastiestFood.Quality)
					{
						tastiestFood = activeFood;
						tastiestFoodEatenBy = eatingPlayers;
					}
					if(activeFood.Quality < grossestFood.Quality)
					{
						grossestFood = activeFood;
						grossestFoodEatenBy = eatingPlayers;
					}
				if(quickestPlayerOfRound.lastChoiceTimeElapsed < quickestNabTime)
				{
					Debug.Log ("Replacing old quickest time: " + quickestNabEatenBy.ProfileInstance.playerName +", "+quickestNabTime+", "+quickestNab.Name+" @"+Time.frameCount);
					Debug.Log ("New fastest time: "+quickestPlayerOfRound.ProfileInstance.playerName+" @"+quickestPlayerOfRound.lastChoiceTimeElapsed);
					quickestNabEatenBy = quickestPlayerOfRound;
					quickestNabTime = quickestPlayerOfRound.lastChoiceTimeElapsed;
					quickestNab = activeFood;
				}
				}
			}
//			public Food quickestNab;
//			public float quickestNabTime;
//			public List<Player> quickestNabEatenBy;


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
			player.playerNameLabelGame.text = player.ProfileInstance.playerName;
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
			activePlayers[i].plate.foods.Clear ();

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

			EndGame();
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
		currentPhase = Phase.GameOver;

		//Show ad before revealing score
		if(InterfaceController.Instance.adsEnabled){
		if(Advertisement.isReady()){ Advertisement.Show(); }
		}


		//Determine winner
		Player[] winQuery = registeredPlayers.OrderByDescending(player => player.Score).ToArray();
//		foreach(Player player in registeredPlayers)
//		{
//			player.playerChoice = PlayerChoice.Inactive;
//		}
		List<PlayerResult> outputRecords = new List<PlayerResult>();
		foreach(Player player in registeredPlayers)
		{
			PlayerResult record = new PlayerResult();
			record.playerStringId = player.ProfileInstance.playerName;
			record.rank = player.Ranking;
			record.score = player.Score;
			record.remainingHp = player.Hp;
			record.gameId = UserDatabase.Instance.userInfo.totalGamesPlayed;
			outputRecords.Add(record);

			//UI
			InterfaceController.Instance.WriteWinner(winQuery[0]);
			StartCoroutine(DisplayWinner());

			//Update profile stats
			player.ProfileInstance.gamesPlayed++;
			player.ProfileInstance.lifetimeScore += player.Score;
			if(player.Score > player.ProfileInstance.bestScore  || player.ProfileInstance.gamesPlayed == 1)
			{
				player.ProfileInstance.bestScore = player.Score;
			}
			if(player.Score < player.ProfileInstance.worstScore || player.ProfileInstance.gamesPlayed == 1)
			{
				player.ProfileInstance.worstScore = player.Score;
			}
		}
		//Save entries to user database
		Debug.Log ("Writing to user database, output: "+outputRecords.Count);

		UserDatabase.Instance.LogGame(outputRecords);
		//UserDatabase.Instance.gameStats.totalGamesPlayed++;
		//outputRecords.AddRange(UserDatabase.Instance.PlayerGameRecords);
		//UserDatabase.Instance.PlayerGameRecords = outputRecords;


		ResetAndHideGame();

		currentPhase = Phase.GameOver;

	}

	IEnumerator DisplayWinner()
	{
		while(Advertisement.isShowing)
		{
			yield return 0;
		}

		InterfaceController.Instance.SetGameUiState(GameUiState.Results);
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

	public void TerminateGame()
	{
		ResetAndHideGame();
		//Update player stats to clear hovering text

		currentPhase = Phase.Pregame;
	}

	public void ResetAndHideGame()
	{
		InterfaceController.Instance.HideFoodRank();
		InterfaceController.Instance.HidePrompts();
		UpdatePlayerStats();
		activePlayers.Clear();
		humanPlayers.Clear();

	}

	public void InitializeMatchStats()
	{
		  tastiestFood = null;
		  tastiestFoodEatenBy = new List<Player>();
		  grossestFood = null;
		  grossestFoodEatenBy = new List<Player>();
		  quickestNab = null;
		  quickestNabTime = -1F;
		  quickestNabEatenBy = null;
	}

	public void OnMarketPurchase(PurchasableVirtualItem pvi, string payload, Dictionary<string, string> extra) {
		// pvi is the PurchasableVirtualItem that was just purchased
		if(pvi.ItemId == "no_ads") {

		}
		// payload is a text that you can give when you initiate the purchase operation and you want to receive back upon completion
		// extra will contain platform specific information about the market purchase.
		//      Android: The "extra" dictionary will contain "orderId" and "purchaseToken".
		//      iOS: The "extra" dictionary will contain "receipt" and "token".
		
		// ... your game specific implementation here ...
	}


}
