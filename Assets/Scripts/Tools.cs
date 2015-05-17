﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tools : MonoBehaviour {

	public static Tools Instance;

	public int numberOfTrials = 10000;
	//public List<Food> trials = new List<Food>();

	public List<float> percentiles = new List<float>();
//	public List<float> hardPercentiles = new List<float>
//	{
//		187.5f, 112.5f, 50f, -25f, -62.5f, -125f
//	};

	public string[] viewCombinedAttributes = new string[3];
	public Food selectedFood;

	public string tagTypeQuery;
	public List<string> queryOuput = new List<string>();



	void Awake () {
		Instance = this;
	}
	// Use this for initialization
//	void Start () {
	//	}

	
	// Update is called once per frame
	void Update () {
	
	}

	public List<float> DeriveRatingPercentiles()
	{
		List<Food> foodTrials = new List<Food>();
		for(int i = 0; i < numberOfTrials; i++)
		{
			Food food = GameController.GetRandomFoodUsingData();
			foodTrials.Add (food);

			//Debug
			if(food.Quality == 0)
			{
				Debug.LogError(food.Name + " has zero quality");
			}
		}
		//percentiles = DeriveRatingPercentiles(foodTrials);


		List<float> percentilesOutput = new List<float>();
		foodTrials = foodTrials.OrderBy(food => food.Quality).ToList();

		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.95f)].Quality.Value);
		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.85f)].Quality.Value);
		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.65f)].Quality.Value);
		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.35f)].Quality.Value);
		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.15f)].Quality.Value);
		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.05f)].Quality.Value);
//		percentilesOutput.Add(trials[(int)Mathf.Floor( trials.Count * 0.05f)].Quality);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.95f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.90f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.75f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.50f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.25f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.10f)].Value);
//		percentilesOutput.Add(foodTrials[(int)Mathf.Floor( foodTrials.Count * 0.05f)].Value);

		Debug.Log ("Best food: "+foodTrials[foodTrials.Count -1].Name+", "+foodTrials[foodTrials.Count - 1].Quality);
		Debug.Log ("Worst food: "+foodTrials[0].Name+", "+foodTrials[0].Quality);

		return percentilesOutput;
	}
	
	public void RunTrials()
	{
		//GET RANDOM FOOD N TIMES
		Dictionary<string, int> trialLog = new Dictionary<string, int>();
		for(int i = 0; i < numberOfTrials; i++)
		{
			Food food = GameController.Instance.GetRandomFoodUsingQueue();
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
}
