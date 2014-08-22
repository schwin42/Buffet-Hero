using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#region Enums

[System.Serializable]
public enum Rank
{
	None = 0,
	S = 1,
	A = 2,
	B = 3,
	C = 4,
	D = 5,
	E = 6,
	F = 7
}

[System.Serializable]
public enum AttributeType
{
	None = 0,
	Descriptor = 1,
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
			return descriptor.name + " " + ingredient.name + " " + form.name;
		}
	}

//	public float Value
//	{
//		get
//		{
//			float value = form.modifier * ingredient.multiplier;
//			if(value < 0)
//			{
//				if(quality.multiplier <= 1)
//				{
//					return value * Mathf.Abs (quality.multiplier);
//				} else {
//					return value * (2 - quality.multiplier);
//				}
//			} else {
//				return value * quality.multiplier;
//			}
//		}
//	}
	
		public FoodAttribute descriptor;
		public FoodAttribute ingredient;
		public FoodAttribute form;
	
}

[System.Serializable]
public class FoodAttribute
{
	public string name = "";
	public Rank rank = Rank.None;
	public AttributeType attributeType;
	public string attributeSubtype;
//	public float multiplier = 1f;
//	public float modifier = 0f;
	public string[] tags = new string[0];
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
	public float price = 0f;
	public float spiciness = 0f;
	public float nausea = 0f;
	public float anticipation = 0f;
	public string description = "";
	public string combinesWellWith = "";
	public string combinesPoorlyWith = "";
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

	public string attributesFile = "";
	public string tagsFile = "Buffet Hero - Tags";

	public static Database Instance;

//	public List<Quality> qualities;
//	public List<Ingredient> ingredients;
//	public List<Form> forms;

	public List<FoodAttribute> foodAttributes;
	public List<Tag> tags;

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		//LoadData();
		LoadTags (Application.dataPath + "/Data/" + tagsFile);
		LoadAttributes(Application.dataPath + "/Data/" + attributesFile);
	}

	public void LoadData()
	{
		string loadPath = Application.dataPath + "/Data/" + attributesFile;
		LoadAttributes(loadPath);
		loadPath = Application.dataPath + "/Data/" + tagsFile;
		LoadTags(loadPath);
	}

	public void LoadAttributes(string filePath)
	{
		Debug.Log ("Load attributes");
		string[] dataLines = File.ReadAllLines(filePath);
		Dictionary<int, string> fieldLookup = new Dictionary<int, string>();
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
						recordAttribute.tags = Regex.Split(recordStrings[j], "; ");
						break;
					case "Rank":
						recordAttribute.rank = StringToRank(recordStrings[j]);
						break;
					default:
						Debug.LogError ("Unhandled field name: "+fieldLookup[j]);
						break;
					}
					
				}
				foodAttributes.Add (recordAttribute);
			}
			
		}
	}

	public void LoadTags(string filePath)
	{
		Debug.Log ("Load tags");
		string[] dataLines = File.ReadAllLines(filePath);
		Dictionary<int, string> fieldLookup = new Dictionary<int, string>();
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
					case "Description":
						recordTag.description = recordStrings[j];
						break;
					case "Combines Well With":
						recordTag.combinesWellWith = recordStrings[j];
						break;
					case "Combines Poorly With":
						recordTag.combinesPoorlyWith = recordStrings[j];
						break;
					default:
						Debug.LogError ("Unhandled tag name: "+fieldLookup[j]);
						break;
					}
					
				}
				tags.Add (recordTag);
			}
			
		}
	}

	public Rank StringToRank(string s)
	{
		switch (s)
		{
		case "S":
			return Rank.S;
		case "A":
			return Rank.A;
		case "B":
			return Rank.B;
		case "C":
			return Rank.C;
		case "D":
			return Rank.D;
		case "E":
			return Rank.E;
		case "F":
			return Rank.F;
		default:
			Debug.LogError("Invalid rank: "+s);
			return Rank.None;
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
		case "descriptor":
				return AttributeType.Descriptor;
		case "ingredient":
				return AttributeType.Ingredient;
		case "form":
				return AttributeType.Form;
		default:
			Debug.LogError("Invalid attribute type: "+s);
			return AttributeType.None;
		}
	}

	public Temperature StringToTemperature(string s)
	{
		switch(s)
		{
		case "hot":
			return Temperature.Hot;
		case "cold":
			return Temperature.Cold;
		case "":
			return Temperature.None;
		default:
			Debug.LogError("Invalid temperature: "+s);
			return Temperature.None;
		}
	}
}
