using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Tools))]
public class ToolsEditor : Editor {

	public GUIContent derivePercentilesButton = new GUIContent("Derive Percentiles");
	public GUIContent submitTagTypeQueryButton = new GUIContent("Submit Tag Type Query");
	public GUIContent displayFoodButton = new GUIContent("Display Food");
	public GUIContent runTrialsButton = new GUIContent("Run Trials");

	public FoodMetadata tools;

	void OnEnable()
	{
		tools = (FoodMetadata) target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		if(GUILayout.Button(derivePercentilesButton))
		{
			tools.percentiles = tools.DeriveRatingPercentiles();
		}
		if(GUILayout.Button(submitTagTypeQueryButton))
		{
			//SubmitTagTypeQuery();
			tools.tagTypeResults = tools.GetTagTypeResults();
		}
		if(GUILayout.Button(displayFoodButton))
		{
			DisplayFood();
		}
		if(GUILayout.Button(runTrialsButton))
		{
			tools.trialFoods = tools.RunTrials();
		}

		DrawDefaultInspector();

	
		serializedObject.ApplyModifiedProperties();
	}

	public void DisplayFood()
	{
		//List<FoodAttribute> outputAttributes = new List<Food>();
		List<FoodAttribute> attributeMatches = (from attribute in GameData.Instance.AttributeData
			where tools.viewCombinedAttributes.Contains(attribute.Id)
				select attribute).ToList();
//		foreach(string attributeString in tools.viewCombinedAttributes)
//		{
//			outputAttributes.Add (from attribute in Database.Instance.attributeData
//			                      where attribute.name == attributeString
//			                      select attribute)
//		}
		Food food = new Food(attributeMatches);
		tools.selectedFood = food;
	}

	public void SubmitTagTypeQuery()
	{
		Debug.Log ("I got nuthin'");
//		tools.queryOuput.Clear ();
//		Dictionary<string, List<FoodAttribute>> tagHits = new Dictionary<string, List<FoodAttribute>>();
//	
//		Tag[] matchingTypeTags = (from tag in Database.Instance.tagData
//			where tag.tagType == tools.tagTypeQuery
//				select tag).ToArray();
//
//		foreach(Tag tag in matchingTypeTags)
//		{
//			if(tagHits.ContainsKey(tag.name))
//			{
//				tagHits[tag.name].Add ();
//			} else {
//				List<Tag> hitList = new List<Tag>();
//				hitList.Add (tag);
//				tagHits.Add (tag.tagType, hitList);
//			}
//		}
//
//		var sortedTagNames = tagHits.OrderByDescending(entry => entry.Value.Count).ToDictionary(entry => entry.Key, entry => entry.Value);
//
//		foreach(KeyValuePair<string, List<Tag>> entry in sortedTagNames)
//		{
//			tools.queryOuput.Add (entry.Key +" occurred "+entry.Value.Count+" times.");
//		}

	}
}
