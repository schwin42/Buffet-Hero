using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System;

#region Enums

[System.Serializable]
public enum LetterRank
{
	None = -1,
	S = 0,
	A = 1,
	B = 2,
	C = 3,
	D = 4,
	E = 5,
	F = 6
}

[System.Serializable]
public enum AttributeType
{
	None = 0,
	Qualifier = 1,
	Ingredient = 2,
	Form = 3,
}

[System.Serializable]
public enum AttributeSubtype
{
	None = 0,
	Temperature = 1,
	Freshness = 2
}

[System.Serializable]
public enum Temperature
{
	None = 0,
	Hot = 1,
	Cold = 2
}

#endregion

#region Classes

[System.Serializable]
public class Food
{
	public string Name 
	{
		get
		{
			string returnString = "";
				for (int i = attributes.Count - 1; i >= 0; i--)
			{

				returnString += attributes[i].name;
				if(i > 0)
				{
					returnString += " ";
				}
			}
			//Debug.Log ("Food = "+returnString);
			return returnString;
			//return descriptor.name + " " + ingredient.name + " " + form.name;
		}
	}
		

	public float Quality
	{
		get
		{
			float netValue = 0f;
			float netMagnitude = 0f;
			foreach(Tag tag in Tags)
			{
				netValue += tag.value;
				netMagnitude += tag.magnitude;
			}
			//Debug.Log ("Net value, magnitude"+netValue + ", "+ netMagnitude);
			float staticValue = (netValue >= 0 ? netValue + 1 : netValue) * (netMagnitude <= -4 ? .25f : (netMagnitude / 4) + 1);
			if(GameController.RandomConstant != 0f)
			{
			return Mathf.Round (staticValue * GameController.RandomConstant);
			} else {
				return Mathf.Round (staticValue * GameController.ScoreConstant);
			}

		}
	}

	//public float Value = 0f;

	private int _damage;
	private bool _damageIsSet;
	public float Damage
	{
		get
		{
			if(!_damageIsSet)
			{
				float netDamage = 0f;
				foreach(Tag tag in Tags)
				{
					float difference = tag.damageRange[1] - tag.damageRange[0];
					float random = UnityEngine.Random.value;
					Debug.Log ("Random = "+random);
					netDamage += (random * difference) + tag.damageRange[0];
				}
				Debug.Log ("Net damage" +netDamage);
				_damage = Mathf.RoundToInt(netDamage * 10);
				Debug.Log ("Recorded damage"+_damage);
				_damageIsSet = true;
				return _damage;
			} else {
				return _damage;
			}

		}
	}

	public List<Tag> Tags
	{
		get
		{
			List<Tag> tags = new List<Tag>();
			foreach (FoodAttribute attribute in attributes)
			{
				tags.AddRange (attribute.tags);
//				foreach(Tag tag in attribute.tags)
//				{
//					tags.Add (tag);
//				}

			}
			tags.AddRange (virtualTags);
			return tags; 
		}
	}

	public void Realize(bool b)
	{
		if(b)
		{
//			Tag _virtualTag = new Tag();
//			_virtualTag.name = "combination";
			foreach(Tag tag in Tags)
			{
				//Database.Instance.testTag = tag;
				if(tag.combinesPoorlyWith != null){
				foreach(string tagString in tag.combinesPoorlyWith)
				{
					int hits = tag.GetHitsInFood(this, tagString);
						if(hits > 0)
						{
					Tag virtualTag = new Tag();
					virtualTag.value -= hits;
					virtualTag.name = tag.name+ " combines poorly with "+tagString+" x"+hits;
					virtualTags.Add (virtualTag);
						}
				}
				}
				if(tag.combinesWellWith != null){
				foreach(string tagString in tag.combinesWellWith)
				{
					int hits = tag.GetHitsInFood(this, tagString);
						if(hits > 0)
						{
					Tag virtualTag = new Tag();
					virtualTag.value += hits;
					virtualTag.name = tag.name+ " combines well with "+tagString+" x"+hits;
					virtualTags.Add (virtualTag);
						}
				}
				}
				if(tag.combinesDramaticallyWith != null){
				foreach(string tagString in tag.combinesDramaticallyWith)
				{
					int hits = tag.GetHitsInFood(this, tagString);
						if(hits > 0)
						{
					Tag virtualTag = new Tag();
					virtualTag.magnitude += hits;
					virtualTag.name = tag.name+ " combines dramatically with "+tagString+" x"+hits;
					virtualTags.Add (virtualTag);
						}
				}
				}
			}
			//virtualTag.Add = _virtualTag;
		} else {
			virtualTags = new List<Tag>();
		}
	}

	public Food (){}

	public Food (Food food)
	{
		attributes = food.attributes;
		virtualTags = food.virtualTags;
	}

	public Food (List<FoodAttribute> _attributes)
	{
		attributes.AddRange(_attributes);
		Realize (true);
	}



	public List<FoodAttribute> attributes = new List<FoodAttribute>();
	public List<Tag> virtualTags = new List<Tag>();
	
}

[System.Serializable]
public class FoodAttribute
{
	public string name = "";
	//public LetterRank rank = LetterRank.None;
	public string rarity = "";
	public AttributeType attributeType;
	public string attributeSubtype;
//	public float multiplier = 1f;
//	public float modifier = 0f;
	public List<Tag> tags = new List<Tag>();
	public List<string> combinations = new List<string>();
	public string date = "";
//	public Temperature temperature = Temperature.None;
//	public float spice = 0f;
//	public string special = "";
}

[System.Serializable]
public class Tag
{
	public string name = "";
	public string tagType = "";
	public float value = 0f;
	public float magnitude = 0f;
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

	public int GetHitsInFood(Food food, string tagString)
	{
		var dataQuery = from dataTag in Database.Instance.tagData
			where dataTag.name == tagString
				select dataTag;
		Tag[] matchingTagsInData = dataQuery.ToArray();
		if(matchingTagsInData.Length == 0)
		{
			//Handle references and types
			return 0;
		} else {
			var foodQuery = from foodTag in food.Tags
				where foodTag.name == tagString && foodTag != this
					select foodTag;
			Tag[] matchingTagsInFood = foodQuery.ToArray();
			int hits = 0;
			foreach(Tag foodTag in matchingTagsInFood)
			{
				hits++;
			}
			return hits;
		}
	}
}

//[System.Serializable]
//public class Ingredient : FoodAttribute
//{
//
//}
//
//[System.Serializable]
//public class Form : FoodAttribute
//{
//
//}
//
//[System.Serializable]
//public class Quality : FoodAttribute
//{
//	//public QualitySubtype subtype = QualitySubtype.None;
//}

#endregion


public class Database : MonoBehaviour {

	public static Database Instance;

	static System.Random _random = new System.Random();

	//Configurable


	public static string attributesFile = "Buffet Legend - Food Attributes.csv";
	public static string tagsFile = "Buffet Legend - Tags.csv";

	//Data
	public List<Tag> tagData;
	public List<FoodAttribute> attributeData;

	//Debug
	//public Tag testTag;


	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		//LoadData();
		//LoadTags (Application.dataPath + "/Data/" + tagsFile);
		//LoadAttributes(Application.dataPath + "/Data/" + attributesFile);
	}

//	public void LoadData()
//	{
//		string loadPath = Application.dataPath + "/Data/" + attributesFile;
//		LoadAttributes(loadPath);
//		loadPath = Application.dataPath + "/Data/" + tagsFile;
//		LoadTags(loadPath);
//	}

	public void LoadAttributes(string filePath)
	{
		Debug.Log ("Loading attributes from"+filePath);
		string[] dataLines = File.ReadAllLines(filePath);
		Dictionary<int, string> fieldLookup = new Dictionary<int, string>();

		attributeData.Clear();
		for(int i = 0; i < dataLines.Length; i++)
		{
			if(i == 0)
			{
				string [] fieldStrings = dataLines[0].Split(',');
				
				for(int j = 0; j < fieldStrings.Length; j++)
				{
					fieldLookup.Add (j, fieldStrings[j]);
				}
			} else {


				FoodAttribute recordAttribute = new FoodAttribute(); 
				string[] recordStrings = dataLines[i].Split(',');
				for (int j = 0; j < fieldLookup.Count; j++)
				{
					switch(fieldLookup[j])
					{
					case "Name ID":
						recordAttribute.name = recordStrings[j];
						break;
					case "Attribute Type":
						recordAttribute.attributeType = StringToAttributeType(recordStrings[j]);
						break;
					case "Subtype":
						recordAttribute.attributeSubtype = recordStrings[j];
						break;
					case "Tags":
						string[] tagArray = Regex.Split(recordStrings[j], "; ");
						List<string> tagList = new List<string>(tagArray);
						var query = from tag in tagData
							where tagList.Contains(tag.name)
								select tag;
						if(tagArray.Length > query.Count ())
						{
							Debug.Log ("Missing tags on "+recordAttribute.name);
						}
						recordAttribute.tags = query.ToList();
						break;
					case "Rank":
						Debug.LogError ("No handling for rank.");
						//recordAttribute.rank = StringToRank(recordStrings[j]);
						break;
					case "Rarity":
						recordAttribute.rarity = recordStrings[j];
						break;
					case "Combinations":
						recordAttribute.combinations = Regex.Split(recordStrings[j], "; ").ToList();
						break;
					case "Date Added":
						recordAttribute.date = recordStrings[j];
						break;
					default:
						Debug.LogError ("Unhandled field name: "+fieldLookup[j]);
						break;
					}
					
				}
				attributeData.Add (recordAttribute);
			}
			
		}
	}

	public void LoadTags(string filePath)
	{
		Debug.Log ("Loading tags from"+filePath);
		string[] dataLines = File.ReadAllLines(filePath);
		Dictionary<int, string> fieldLookup = new Dictionary<int, string>();

		tagData.Clear();
		for(int i = 0; i < dataLines.Length; i++)
		{
			if(i == 0)
			{
				string [] fieldStrings = dataLines[0].Split(',');
				
				for(int j = 0; j < fieldStrings.Length; j++)
				{
					fieldLookup.Add (j, fieldStrings[j]);
				}
			} else {
				
				Tag recordTag = new Tag(); 
				string[] recordStrings = dataLines[i].Split(',');
				for (int j = 0; j < fieldLookup.Count; j++)
				{
					if(recordStrings[j] != ""){
					switch(fieldLookup[j])
					{
					case "Tag ID":
						recordTag.name = recordStrings[j];
						break;
					case "Type":
						recordTag.tagType = recordStrings[j];
						break;
					case "Value":
						recordTag.value = StringToFloat(recordStrings[j]);
						break;
					case "Calories":
						recordTag.calories = StringToFloat(recordStrings[j]);
						break;
					case "Magnitude":
						recordTag.magnitude = StringToFloat(recordStrings[j]);
						break;
					case "Price":
						recordTag.price = StringToFloat(recordStrings[j]);
						break;
					case "Spiciness":
						recordTag.spiciness = StringToFloat(recordStrings[j]);
						break;
					case "Nausea":
						recordTag.nausea = StringToFloat(recordStrings[j]);
						break;
					case "Anticipation":
						recordTag.anticipation = StringToFloat(recordStrings[j]);
						break;
					case "Effect":
						recordTag.description = recordStrings[j];
						break;
					case "Combines Well With":
					//	Debug.Log (recordStrings[j]);
						string[] tagArray = Regex.Split(recordStrings[j], "; ");
							//Debug.Log (tagArray[0]);
							recordTag.combinesWellWith = tagArray;
							//Debug.Log (recordTag.combinesWellWith[0]);
						break;
					case "Combines Poorly With":
						recordTag.combinesPoorlyWith = Regex.Split(recordStrings[j], "; ");
							break;
					case "Combines Dramatically With":
						recordTag.combinesDramaticallyWith = Regex.Split(recordStrings[j], "; ");
						break;
					case "Damage":
							if(recordStrings[j].Contains(";")){
								string[] damageStrings = Regex.Split (recordStrings[j], ";");
							for(int k = 0; k < damageStrings.Length; k++)
							{
								recordTag.damageRange[k] = float.Parse(damageStrings[k]);
							}
						} else {
							Debug.Log ('"'+recordStrings[j]+'"');
						recordTag.damageRange[0] = float.Parse(recordStrings[j]);
						recordTag.damageRange[1] = float.Parse(recordStrings[j]);
						}
							break;
						case "HelpTag":
							recordTag.helpTag = recordStrings[j];
							break;
					default:
						Debug.LogError ("Unhandled tag name: "+fieldLookup[j]);
						break;
					}
				}

				}
				var query = from tag in tagData
					where tag.name == recordTag.name
						select tag;
				if(query.Count () > 0)
				{
					Debug.Log ("Tag "+recordTag.name+" already exists.");
				} else {
				tagData.Add (recordTag);
				}
			}
		}
		}

	public LetterRank StringToRank(string s)
	{
		switch (s)
		{
		case "S":
			return LetterRank.S;
		case "A":
			return LetterRank.A;
		case "B":
			return LetterRank.B;
		case "C":
			return LetterRank.C;
		case "D":
			return LetterRank.D;
		case "E":
			return LetterRank.E;
		case "F":
			return LetterRank.F;
		default:
			Debug.LogError("Invalid rank: "+s);
			return LetterRank.None;
		}
	}

	public float StringToFloat(string s)
	{
		if(s != "")
		{
			return float.Parse(s);
		} else {
			return 0;
		}
	}

	public AttributeType StringToAttributeType(string s)
	{
		switch (s)
		{
		case "qualifier":
				return AttributeType.Qualifier;
		case "ingredient":
				return AttributeType.Ingredient;
		case "form":
				return AttributeType.Form;
		default:
			Debug.LogError("Invalid attribute type: "+s);
			return AttributeType.None;
		}
	}

//	public Temperature StringToTemperature(string s)
//	{
//		switch(s)
//		{
//		case "hot":
//			return Temperature.Hot;
//		case "cold":
//			return Temperature.Cold;
//		case "":
//			return Temperature.None;
//		default:
//			Debug.LogError("Invalid temperature: "+s);
//			return Temperature.None;
//		}
//	}


//	private void SerializeAndSaveToBinary()
//	{
//		BinaryWriter binaryWriter = new BinaryWriter();
//		FileStream fileStream = File.Create (Application.persistentDataPath
//	}
		//	{
		//#if !UNITY_WINRT
		//		BinaryFormatter binaryFormatter = new BinaryFormatter();
		//		//Debug.Log (dataPath + binaryFileName);
		//		FileStream fileStream = File.Create (Application.dataPath+"/Data/" + binaryFileName);
		//
		//		binaryFormatter.Serialize(fileStream, masterInfo);
		//		fileStream.Close ();
		//
		//		Debug.Log ("Saved to binary @"+Time.frameCount);
		//#endif
		//	}
		
		//	private void LoadAndDeserializeFromBinary()
		//    {
		//#if !UNITY_WINRT
		//		string dataPath = Application.dataPath + "/Data/";
		//        if(File.Exists (dataPath + binaryFileName))
		//		{
		//			BinaryFormatter binaryFormatter = new BinaryFormatter();
		//			FileStream fileStream = File.Open (dataPath + binaryFileName, FileMode.Open);
		//			masterInfo = (MasterInfo)binaryFormatter.Deserialize(fileStream);
		//			fileStream.Close ();
		//			Debug.Log ("Master info loaded from binary");
		//		} else {
		//			Debug.LogError ("Path not found: " + dataPath + binaryFileName);
		//		}
		//#endif
		//    }
	public static void Shuffle<T>(List<T> list)
	{
		var random = _random;
		for (int i = list.Count; i > 1; i--)
		{
			// Pick random element to swap.
			int j = random.Next(i); // 0 <= j <= i-1
			// Swap.
			T tmp = list[j];
			list[j] = list[i - 1];
			list[i - 1] = tmp;
		}
	}

//	public float GetRandomInRange(float lowerLimit, float upperLimit)
//	{
//
//	}


}
