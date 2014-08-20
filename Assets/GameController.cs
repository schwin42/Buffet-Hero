﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public int trials = 1;

	//Status
	public Food activeFood;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
		NextPrompt();

//		for(int i = 0; i < trials; i++)
//		{
//			Player.Instance.Eat();
//		}

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

		//Unit test to compare statistical data with literal percentages

	}
	
	// Update is called once per frame
	void Update () {
	
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

	public Quality GetRandomQuality (Rank rank)
	{
			var query = from quality in Database.Instance.qualities
				where quality.rank == rank
					select quality;
			Quality[] possibleQualities = query.ToArray();
		if(possibleQualities.Length > 0)
		{
		return possibleQualities[(int)Mathf.Floor (Random.value * possibleQualities.Length)];
		} else {
			Debug.LogError("No qualities of rank "+rank+" found.");
			return new Quality();
		}
	}

	public Ingredient GetRandomIngredient (Rank rank)
	{
		//Debug.Log (rank);
		var query = from ingredient in Database.Instance.ingredients
			where ingredient.rank == rank
				select ingredient;
		Ingredient[] possibleIngredients = query.ToArray();
		//Debug.Log (possibleIngredients.Length);
		if(possibleIngredients.Length > 0)
		{
		return possibleIngredients[(int)Mathf.Floor (Random.value * possibleIngredients.Length)];
	} else {
		Debug.LogError("No ingredients of rank "+rank+" found.");
		return new Ingredient();
	}
	}

	public Form GetRandomForm (Rank rank)
	{
		var query = from form in Database.Instance.forms
			where form.rank == rank
				select form;
		Form[] possibleForms = query.ToArray();
		if(possibleForms.Length > 0)
		{
		return possibleForms[(int)Mathf.Floor (Random.value * possibleForms.Length)];
} else {
	Debug.LogError("No forms of rank "+rank+" found.");
	return new Form();
}
	}

	public Food GetRandomFood()
	{
		Food food = new Food();

		Rank qualityRank = GetRandomRank();
		food.quality = GetRandomQuality(qualityRank);
		food.quality.multiplier = GetQualityMultipler(qualityRank);
		Rank ingredientRank = GetRandomRank();
		food.ingredient = GetRandomIngredient(ingredientRank);
		food.ingredient.multiplier = GetIngredientMultipler(ingredientRank);
		Rank formRank = GetRandomRank();
		food.form = GetRandomForm(formRank);
		food.form.modifier = GetFormModifier(formRank);

		return food;
	}

	public float GetQualityMultipler(Rank rank)
	{
		switch(rank)
		{
		case Rank.S:
			return 1.5f;
		case Rank.A:
			return 1.25f;
		case Rank.B:
			return 1.1f;
		case Rank.C:
			return 1f;
		case Rank.D:
			return .5f;
		case Rank.E:
			return -.25f;
		case Rank.F:
			return -.5f;
		default:
			Debug.LogError ("Invalid rank: "+rank);
			return 1f;
		}
	}

	public float GetIngredientMultipler(Rank rank)
	{
		switch(rank)
		{
		case Rank.S:
			return 2f;
		case Rank.A:
			return 1.5f;
		case Rank.B:
			return 1.25f;
		case Rank.C:
			return 1f;
		case Rank.D:
			return .5f;
		case Rank.E:
			return -.5f;
		case Rank.F:
			return -1f;
		default:
			Debug.LogError ("Invalid rank: "+rank);
			return 1f;
		}
	}

	public float GetFormModifier(Rank rank)
	{
		switch(rank)
		{
		case Rank.S:
			return 150f;
		case Rank.A:
			return 133f;
		case Rank.B:
			return 116f;
		case Rank.C:
			return 100f;
		case Rank.D:
			return 84f;
		case Rank.E:
			return 67f;
		case Rank.F:
			return 50f;
		default:
			Debug.LogError ("Invalid rank: "+rank);
			return 10f;
		}
	}

	public void NextPrompt()
	{
		activeFood = GetRandomFood();
		InterfaceController.Instance.WriteToPrompt("You encounter: "+
		                                           "\n "+activeFood.Name);
		//InterfaceController.Instance.WriteToOutcome("");
		InterfaceController.Instance.WriteToScore(Player.Instance.score);
	}
	
}
