using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Phase
{
	Uninitialized = 0,
	Choose = 1,
	Evaluate = 2,
	FinalScoring = 3
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
	public int numberOfTrials = 1;

	//Cached
	public Player[] players = new Player[4];

	//Status
	//public int confirmedPlayers = 0;
	public Phase currentPhase = Phase.Uninitialized;
	public Food activeFood;
	public Food previousFood;
	public Dictionary<int, bool> playerChoices = new Dictionary<int, bool>(); //True indicates "eat"

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
	//	players = 
		GameObject camera = GameObject.Find ("UI Root/Camera");
	players = camera.GetComponentsInChildren<Player>();
	//	foreach(GameObject playerGo in playerGos)
	//	{
	//		players.Add (playerGo.GetComponent<Player>());
	//	}

		NextPrompt();

//		for(int i = 0; i < numberOfTrials; i++)
//		{
//			Player.Instance.Eat();
//		}
//		Debug.Log ("Average score: "+Player.Instance.score / numberOfTrials);

//		Dictionary<string, int> trialLog = new Dictionary<string, int>();
//		for(int i = 0; i < trials; i++)
//		{
//			Food food = GetRandomFood();
//			if(trialLog.ContainsKey(food.Name))
//			{
//				trialLog[food.Name] += 1;
//			} else {
//				trialLog.Add (food.Name, 1);
//			}
//		}
//		Dictionary<string, int> orderedLog = trialLog.OrderByDescending(trial => trial.Value).ToDictionary(trial => trial.Key, trial => trial.Value);
//		foreach(KeyValuePair<string, int> pair in orderedLog)
//		{
//			Debug.Log (pair.Key + " appeared "+pair.Value+" times");
//		}

	}
	
	// Update is called once per frame
	void Update () {
	
		//bool allPlayersConfirmed = true;
		if(currentPhase == Phase.Choose)
		{
if(playerChoices.Count >= 4)
		{
				EvaluateRound();
		}
		}
	}

	public Rank GetRandomRank()
	{
		Restaurant restaurant = activeRestaurant;

		Dictionary<Rank, float> probabilityTable = new Dictionary<Rank, float>();
		float rankCutoff = 0f;
		for(int i = 0; i < restaurant.rankPercentages.Length; i++)
		{
			probabilityTable.Add ((Rank)i+1, restaurant.rankPercentages[i] + rankCutoff);
			rankCutoff += restaurant.rankPercentages[i];
		}

		float dieRoll = Random.value * rankCutoff;
		for(int i = 0; i < probabilityTable.Count; i++)
		{
			if(dieRoll <= probabilityTable[(Rank)i + 1])
			{
				return (Rank)i +1;
			}
		}

		Debug.LogError("Rank out of range");
		return Rank.None;
	}


	public FoodAttribute GetRandomAttribute (AttributeType attributeType, Rank rank)
	{
		IEnumerable<FoodAttribute> query = null;
		switch(attributeType)
		{
		case AttributeType.Descriptor:
			query = from attribute in Database.Instance.attributeData
				where attribute.rank == rank && attribute.attributeType == AttributeType.Descriptor
					select attribute;
			break;
		case AttributeType.Ingredient:
			query = from attribute in Database.Instance.attributeData
				where attribute.rank == rank && attribute.attributeType == AttributeType.Ingredient
					select attribute;
			break;
		case AttributeType.Form:
			query = from attribute in Database.Instance.attributeData
				where attribute.rank == rank && attribute.attributeType == AttributeType.Form
					select attribute;
			break;
		default:
			Debug.LogError ("Invalid attribute type: "+attributeType);
			break;
		}

		FoodAttribute[] possibleAttributes = query.ToArray();
		if(possibleAttributes.Length > 0)
		{
		return possibleAttributes[(int)Mathf.Floor (Random.value * possibleAttributes.Length)];
		} else {
		Debug.LogError("No "+attributeType+"s of Rank "+rank+" found.");
		return null;
		}
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
		Food food = new Food();

		food.attributes.Add (GetRandomAttribute(AttributeType.Form, GetRandomRank()));
		food.attributes.Add (GetRandomAttribute(AttributeType.Ingredient, GetRandomRank()));
		food.attributes.Add (GetRandomAttribute(AttributeType.Descriptor, GetRandomRank()));
		food.Realize(true);

		return food;
	}

	public void NextPrompt()
	{
		if(currentPhase == Phase.Evaluate || currentPhase == Phase.Uninitialized)
		{
		foreach(Player player in players)
		{
			player.updateScoreLabel.text = "";
			player.Score += player.pendingScore;
				player.pendingScore = 0f;
		}

		currentPhase = Phase.Choose;
		playerChoices = new Dictionary<int, bool>();
		foreach(Player player in players)
		{
			player.EnableButtons(true);
		}

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
		}
	}

	public void EvaluateRound()
	{
		currentPhase = Phase.Evaluate;
		foreach(Player player in players)
		{
			if(playerChoices[player.playerId]) //if player chose to eat
			{
				float qualityFloat = activeFood.Quality;
				string qualityString = qualityFloat >= 0 ? "+" + qualityFloat.ToString ("F0"): qualityFloat.ToString("F0");
				player.updateScoreLabel.text = qualityString;
				player.pendingScore = qualityFloat;
			}
		}
	}
	
}
