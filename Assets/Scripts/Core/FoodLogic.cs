using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

[System.Serializable]
public class Food
{
	public string Name {
		get {
			string returnString = "";
			for (int i = attributes.Count - 1; i >= 0; i--) {
				
				returnString += attributes [i].Id;
				if (i > 0) {
					returnString += " ";
				}
			}
			return returnString;
		}
	}
	
//	private float netValue;
//	private float netMagnitude;
//	private float netAbsolute;
	private float _quality;
	
	public float? Quality { 
		get {
			return _quality;
		}
		private set {
			_quality = value.Value;
		}
	}
	
	private int _damage;
	private bool _damageIsSet = false;
	
	public float Damage {
		get {
			if (!_damageIsSet) {
				float netDamage = 0f;
				foreach (Tag tag in Tags) {
					float difference = tag.damageRange [1] - tag.damageRange [0];
					float random = UnityEngine.Random.value;
					//Debug.Log ("Random = " + random);
					netDamage += (random * difference) + tag.damageRange [0];
				}
				//Debug.Log ("Net damage" + netDamage);
				_damage = Mathf.RoundToInt (netDamage * 10);
				//Debug.Log ("Recorded damage" + _damage);
				_damageIsSet = true;
				return _damage;
			} else {
				return _damage;
			}
			
			
		}
	}
	
	public List<Tag> Tags {
		get {
			List<Tag> tags = new List<Tag> ();
			foreach (FoodAttribute attribute in attributes) {
				tags.AddRange (attribute.tags);
			}
			tags.AddRange (virtualTags);
			return tags; 
		}
	}
	
	public bool _isRealized = false;
	
	public void Realize ()
	{
		this.PopulateVirtualTags ();
		this.DeriveQualityFromTags (true);
		_isRealized = true;
	}
	
	private void PopulateVirtualTags () {
		foreach(FoodAttribute attribute in this.attributes) {
			foreach(Tag tag in attribute.tags) {
				this.virtualTags.AddRange(GetVirtualTags (FoodCombination.CombinesPoorlyWith, attribute, tag));
				this.virtualTags.AddRange(GetVirtualTags (FoodCombination.CombinesWellWith, attribute, tag));
				this.virtualTags.AddRange(GetVirtualTags (FoodCombination.CombinesDramaticallyWith, attribute, tag));
			}
		}
	}
	
	private List<Tag> GetVirtualTags(FoodCombination foodCombination, FoodAttribute sourceAttribute, Tag sourceTag) {
		string[] criteriaList;
		switch(foodCombination) {
		case FoodCombination.CombinesPoorlyWith:
			criteriaList = sourceTag.combinesPoorlyWith;
			break;
		case FoodCombination.CombinesWellWith:
			criteriaList = sourceTag.combinesWellWith;
			break;
		case FoodCombination.CombinesDramaticallyWith:
			criteriaList = sourceTag.combinesDramaticallyWith;
			break;
		default:
			Debug.LogError("Invalid food combination");
			criteriaList = null;
			break;
		}
		List<Tag> outputTags = new List<Tag>();
		foreach (string searchString in criteriaList) {
			int hits = this.GetHitsInFood (sourceAttribute, searchString);
			if (hits > 0) {
				Tag virtualTag = new Tag ();
				
				switch(foodCombination) {
				case FoodCombination.CombinesPoorlyWith:
					virtualTag.value -= hits;
					break;
				case FoodCombination.CombinesWellWith:
					virtualTag.value += hits;
					break;
				case FoodCombination.CombinesDramaticallyWith:
					virtualTag.magnitude += hits;
					break;
				}
				virtualTag.Id = hits + "x " + sourceTag.Id + " " + foodCombination.ToString() + " " + searchString;
				outputTags.Add (virtualTag);
			}
		}
		return outputTags;
	}
	
	private int GetHitsInFood (FoodAttribute sourceAttribute, string tagString) {
		List<Tag> iterableTags = new List<Tag>();
		foreach(FoodAttribute attribute in this.attributes) {
			if(attribute == sourceAttribute) { //Count only tags on other attributes
				continue;
			}
			iterableTags.AddRange(attribute.tags);
		}
		Tag[] matchingTags = iterableTags.Where(tag => tag.Id == tagString).ToArray();
		return matchingTags.Length;
	}
	
	private void DeriveQualityFromTags (bool useRandomConstant)
	{
		float netValue = 0f;
		float netMagnitude = 0f;
		int netAbsolute = 0;
		foreach (Tag tag in Tags) {
			netValue += tag.value;
			netMagnitude += tag.magnitude;
			netAbsolute += tag.absolute;
		}
//		this.netValue = netValue;
//		this.netMagnitude = netMagnitude;
//		this.netAbsolute = netAbsolute;
		float preAbsoluteValue = (netValue >= 0 ? netValue + 1 : netValue) * (netMagnitude <= -4 ? .25f : (netMagnitude / 4) + 1);
		float postAbsoluteValue;
		if (netAbsolute == 0) {
			postAbsoluteValue = preAbsoluteValue;
		} else if (netAbsolute > 0) {
			postAbsoluteValue = Mathf.Abs (preAbsoluteValue);
		} else {
			postAbsoluteValue = -Mathf.Abs (preAbsoluteValue);
		}
		float output = 0f;
		if(useRandomConstant) {
			output = Mathf.Round (postAbsoluteValue * FoodLogic.Instance.scoreWobbleMultiplier);
		} else {
			output = Mathf.Round (postAbsoluteValue * GameData.SCORE_CONSTANT);
		}
		Debug.Log ("Quality = " + output);
		this.Quality = output;
	}
	
	//Used in get food functions in game controller
	public Food () { }
	
	//Copy food
	public Food (Food food)
	{
		attributes = food.attributes;
		virtualTags = food.virtualTags;
	}
	
	//Construct food from attributes
	public Food (List<FoodAttribute> _attributes)
	{
		attributes.AddRange (_attributes);
		this.Realize ();
	}
	
	public List<FoodAttribute> attributes = new List<FoodAttribute> ();
	public List<Tag> virtualTags = new List<Tag> ();
	
}

[System.Serializable] public class FoodInfo {
	public bool isInitialized = false;
	public string name;
	public List<string> attributeIds;
	public float quality;

	public FoodInfo (string name, List<string> attributeIds, float quality) {
		this.isInitialized = true;
		this.name = name;
		this.attributeIds = attributeIds;
		this.quality = quality;
	}
}

[System.Serializable]
public class FoodAttribute
{
	public string Id = "";
	//public LetterRank rank = LetterRank.None;
	public string rarity = "";
	public AttributeType attributeType;
	public string attributeSubtype;
	public List<Tag> tags = new List<Tag> ();
	public List<string> combinations = new List<string> ();
	public string date = "";
	//	public string special = "";
}

[System.Serializable]
public class Tag
{
	public string Id = "";
	public string TagType = "";
	public float value = 0f;
	public float magnitude = 0f;
	public int absolute = 0;
	public float[] damageRange = new float[] {0, 0};
	public float price = 0f;
	public float calories = 0f;
	public float spiciness = 0f;
	public float nausea = 0f;
	public float anticipation = 0f;
	public string description = "";
	public string[] combinesWellWith;
	public string[] combinesPoorlyWith;
	public string[] combinesDramaticallyWith;
	public string helpTag = "";
}


public class FoodLogic : MonoBehaviour {

	public static System.Random randomSeed = new System.Random ();

	public static int NextUnityRandomSeed {
		get {
			return randomSeed.Next(1000);
		}
	}

	public float scoreWobbleMultiplier = 25f;

	private static FoodLogic _instance;
	public static FoodLogic Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<FoodLogic>();
				print (_instance);
			}
			return _instance;
		}
	}

	private List<FoodAttribute> _qualifierQueue;
	private List<FoodAttribute> QualifierQueue {
		get {
			if (_qualifierQueue == null || _qualifierQueue.Count == 0) {
				_qualifierQueue = GetShuffledAttributes (AttributeType.Qualifier);
			}
			return _qualifierQueue;
		}
	}

	private List<FoodAttribute> _ingredientQueue;
	private List<FoodAttribute> IngredientQueue {
		get {
			if (_ingredientQueue == null || _ingredientQueue.Count == 0) {
				_ingredientQueue = GetShuffledAttributes (AttributeType.Ingredient);
			}
			return _ingredientQueue;
		}
	}

	private List<FoodAttribute> _formQueue;
	private List<FoodAttribute> FormQueue {
		get {
			if (_formQueue == null || _formQueue.Count == 0) {
				_formQueue = GetShuffledAttributes (AttributeType.Form);
			}
			return _formQueue;
		}
	}



	public static List<FoodAttribute> GetShuffledAttributes (AttributeType type)
	{
		List <FoodAttribute> attributes = GameData.Instance.AttributeData.Where (a => a.attributeType == type).ToList();
		ShuffleList (attributes);
		return attributes;
	}

	public static ReadOnlyCollection<FoodAttribute> GetShuffledAttributes (AttributeType type, int seed) {
		List <FoodAttribute> attributes = GameData.Instance.AttributeData.Where (a => a.attributeType == type).ToList();
		ShuffleList (attributes, seed);
		P2pInterfaceController.Instance.WriteToConsole("!!!GSA count: " + attributes.Count);
		return attributes.AsReadOnly();
	}
	
	private FoodAttribute GetRandomAttributeFromQueue (AttributeType attributeType)
	{
		List<FoodAttribute> sourceList = null;
		switch (attributeType) {
		case AttributeType.Form:
			sourceList = FormQueue;
			break;
		case AttributeType.Ingredient:
			sourceList = IngredientQueue;
			break;
		case AttributeType.Qualifier:
			sourceList = QualifierQueue;
			break;
		default:
			Debug.LogError("Invalid attribute type: " + attributeType);
			break;
		}

		FoodAttribute attribute = sourceList [0];
		sourceList.RemoveAt (0);
		sourceList.Insert (sourceList.Count, attribute);
		return attribute;
	}
	
	public Food GetRandomFoodUsingQueue () //During runtime; for game use
	{
		Food food = new Food ();

		food.attributes.Add (GetRandomAttributeFromQueue (AttributeType.Form));		
		food.attributes.Add (GetRandomAttributeFromQueue (AttributeType.Ingredient));
		food.attributes.Add (GetRandomAttributeFromQueue (AttributeType.Qualifier));

		food.Realize ();
		return food;
	}

	public static void ShuffleList<T> (List<T> list, int seed) {
		UnityEngine.Random.seed = seed;
		for (int i = list.Count; i > 1; i--) {
			// Pick random element to swap.
			int j = UnityEngine.Random.Range (0, i); // 0 <= j <= i-1
			// Swap.
			T tmp = list [j];
			list [j] = list [i - 1];
			list [i - 1] = tmp;
		}
	}

	public static void ShuffleList<T> (List<T> list)
	{
		for (int i = list.Count; i > 1; i--) {
			// Pick random element to swap.
			int j = randomSeed.Next (i); // 0 <= j <= i-1
			// Swap.
			T tmp = list [j];
			list [j] = list [i - 1];
			list [i - 1] = tmp;
		}
	}

	public static FoodInfo GetFoodInfo (Food food)
	{
		if (!food.Quality.HasValue) {
			P2pInterfaceController.Instance.WriteToConsole ("Error in GetFoodInfo, food quality has no value");
			return null;
		}
		return new FoodInfo (food.Name, food.attributes.Select (attribute => attribute.Id).ToList (), food.Quality.Value);
	}
}
