using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Phase
{
	Uninitialized = 0,
	Choose = 1,
	Evaluate = 2,
	FinalScoring = 3,
	GameOver = 4
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

	//Cached
	public Player[] players = new Player[4];
	public List<FoodAttribute> qualifierQueue;
	public List<FoodAttribute> ingredientQueue;
	public List<FoodAttribute> formQueue;

	//Status
	public int currentRound = -1;

	//public int confirmedPlayers = 0;
	public List<Player> humanPlayers = new List<Player>();
	public Phase currentPhase = Phase.Uninitialized;
	public Food activeFood;
	public Food previousFood;
	public Dictionary<int, bool> playerChoices = new Dictionary<int, bool>(); //True indicates "eat"

	//Debug
	public int trials = 1000;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
	//	players = 
		qualifierQueue = GetShuffledAttributes(AttributeType.Qualifier);
		ingredientQueue = GetShuffledAttributes(AttributeType.Ingredient);
		formQueue = GetShuffledAttributes(AttributeType.Form);

		//qualifierQueue

		                        //Acquire objects
		GameObject camera = GameObject.Find ("UI Root/Camera");
	players = camera.GetComponentsInChildren<Player>();


		//Register players
		RegisterPlayers();

		//Initialize game
		//BeginRound();

		//GET AVERAGE SCORE
//		for(int i = 0; i < numberOfTrials; i++)
//		{
//			Player.Instance.Eat();
//		}
//		Debug.Log ("Average score: "+Player.Instance.score / numberOfTrials);

		//RunFoodTrials ();

	}
	
	// Update is called once per frame
	void Update () {
	
		//Debug
		if (Input.GetKeyUp (KeyCode.Alpha1)) {
			RunFoodTrials();
				}


		if(currentPhase == Phase.Choose)
		{
if(playerChoices.Count >= 4)
		{
				EvaluateRound();
		}

			//Run out of food after 2
			int eatCounter = 0;
			List<Player> eatingPlayers = new List<Player>();
			foreach(KeyValuePair<int, bool> entry in playerChoices)
			{
				if(entry.Value)
				{
					eatCounter++;
					eatingPlayers.Add (players[entry.Key]);

				}
			}
			if(eatCounter >= servingsPerFood)
			{
				foreach(Player player in players)
				{
					if(!eatingPlayers.Contains(player))
					{
						player.Pass ();
					}
				}
				EvaluateRound();
				//break;
			}

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


	public FoodAttribute GetRandomAttribute (AttributeType attributeType)
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

	public Food GetRandomFood()
	{
		Debug.Log ("Get random food @" + currentRound);
		Food food = new Food();

		food.attributes.Add (GetRandomAttribute(AttributeType.Form));
		food.attributes.Add (GetRandomAttribute(AttributeType.Ingredient));
		food.attributes.Add (GetRandomAttribute(AttributeType.Qualifier));
		food.Realize(true);
//
		return food;
	}

	public void BeginRound()
	{

		currentRound ++;
		Debug.Log ("Begin round: " + currentRound);
		//Increment round


		//Update score
		foreach(Player player in players)
		{
			player.updateScoreLabel.text = "";
			player.Score += player.pendingScore;
			player.pendingScore = 0f;
		}

		//Set ranking
		Player[] playersByScore = ((from player in players
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

		//Reset choices
		playerChoices = new Dictionary<int, bool>();


		NextFood ();
	}

	public void NextFood()
	{
		Debug.Log ("Next food for: " + currentRound);	
		if(activeFood != null)
		{
			previousFood = activeFood;
		}
		activeFood = GetRandomFood();
		InterfaceController.Instance.WriteToPrompt(
			//"You encounter: "+
		                                           //"\n "+
		                                           activeFood.Name);
		//InterfaceController.Instance.WriteToOutcome("");
		//InterfaceController.Instance.WriteToScore(Player.Instance.score);
		//EvaluateRound();
		currentPhase = Phase.Choose;

		foreach(Player player in players)
		{
			player.EnableButtons(true);
		}
	}

	public void EvaluateRound()
	{
		Debug.Log ("Evaluate round: " + currentRound);
		currentPhase = Phase.Evaluate;
		foreach(Player player in players)
		{
			if(playerChoices[player.playerId]) //if player chose to eat
			{
				Debug.Log ("Update score");
				float qualityFloat = activeFood.Quality;
				string qualityString = qualityFloat >= 0 ? "+" + qualityFloat.ToString ("F0"): qualityFloat.ToString("F0");
				player.updateScoreLabel.text = qualityString;
				player.pendingScore = qualityFloat;
			}
		}
		Debug.Log ("Evaluation ended @"+currentRound);

		//Debug
		//EndRound ();
	}

	void RegisterPlayers()
	{
		Debug.Log ("Register players");
		//Log human players

		for(int i = 0; i < players.Count (); i++)
		{
			//Log as human 
			if(players[i].controlType == ControlType.Human)
			{
				humanPlayers.Add(players[i]);
			}

			//Set identification
			players[i].name = "Player "+i;
			players[i].playerId = i;


			
			//Set color
			players[i].playerColor = (PlayerColor)i;

		}
	}

	public void EndRound()
	{
		Debug.Log ("Ending round " + currentRound);
		if (currentRound < numberOfRounds - 1) {
						BeginRound ();
				} else {
			currentPhase = Phase.GameOver;
				}
	}

	public void RunFoodTrials()
	{
		//GET RANDOM FOOD N TIMES
		Dictionary<string, int> trialLog = new Dictionary<string, int>();
		for(int i = 0; i < trials; i++)
		{
			Food food = GetRandomFood();
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
	
}
