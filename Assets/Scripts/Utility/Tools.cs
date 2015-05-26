using UnityEngine;
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

	//TODO Incomplete
	//public string tagTypeQuery;
//	public List<string> queryOuput = new List<string>();

	public string tagTypeQuery;
	public List<string> tagTypeResults;
	public List<Food> trialFoods;


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
			Food food = Database.GetRandomFoodFromData();
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
	
	public List<Food> RunTrials()
	{
		//GET RANDOM FOOD N TIMES
		Dictionary<Food, int> trialLog = new Dictionary<Food, int>();
		for(int i = 0; i < numberOfTrials; i++)
		{
			Food food = Database.GetRandomFoodFromData();
			if(trialLog.ContainsKey(food))
			{
				trialLog[food] += 1;
			} else {
				trialLog.Add (food, 1);
			}
		}
		Dictionary<Food, int> orderedDict = trialLog.OrderByDescending(trial => trial.Value).ToDictionary(trial => trial.Key, trial => trial.Value);
		List<Food> orderedList = new List<Food>();
		foreach(KeyValuePair<Food, int> pair in orderedDict)
		{
			Debug.Log (pair.Key.Name + " appeared "+pair.Value+" times");
			orderedList.Add (pair.Key);
		}
		return orderedList;
	}

	public List<string> GetTagTypeResults() {
		//print("Running GTTR");
		//List<string> outputStrings = new List<string>();
		Dictionary<string, int> outputDict = new Dictionary<string, int>();
		//for each tag of specified tag type
//		List<Tag> tagsOfQueriedType = new List<Tag>();
//		foreach(Tag tag1 in Database.Instance.TagData) {
//			print("Tag type, target string, # of tags available: " + tag1.TagType + ", " + this.tagTypeQuery + ", " + Database.Instance.TagData.Count);
//
//			tagsOfQueriedType.Add (tag1);
//		}
		List<Tag> tagsOfQueriedType = Database.Instance.TagData.Where(t => t.TagType == this.tagTypeQuery).ToList();
		print ("tags of queried" + tagsOfQueriedType.Count);
		foreach(Tag tag in tagsOfQueriedType) {
			//Get hits in attributes
			//int hits = Database.Instance.AttributeData.Where(a => 
			int hits = Database.Instance.AttributeData.Where(a => Database.TagListContainsId(a.tags, tag.Id)).Count();
			//Print tags and hits to string
			outputDict.Add (tag.Id, hits);
//			outputStrings.Add (hits+ " - " + tag.Id);
		}
		//Sort by descending hits
		var sortedOutputDict = outputDict.OrderByDescending(kp => kp.Value);
		List<string> outputStrings = new List<string>();
		foreach(KeyValuePair<string, int> pair in sortedOutputDict) {
			print ("%" + pair.Value / Database.Instance.TagData.Count);
			outputStrings.Add (pair.Key + " - " + pair.Value + " - " + (((float)pair.Value) / ((float)Database.Instance.TagData.Count) * 100F).ToString("F2") + "%");
		}
		//outputStrings = outputStrings.OrderByDescending(s => s).ToList();

		//Return string
		return outputStrings;
	}
}
