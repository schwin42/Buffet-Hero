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
	Quality = 1,
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
			return quality.name + " " + ingredient.name + " " + form.name;
		}
	}

	public float Value
	{
		get
		{
			float value = form.modifier * ingredient.multiplier;
			if(value < 0)
			{
				if(quality.multiplier <= 1)
				{
					return value * Mathf.Abs (quality.multiplier);
				} else {
					return value * (2 - quality.multiplier);
				}
			} else {
				return value * quality.multiplier;
			}
		}
	}
	
		public FoodAttribute quality;
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
	public float multiplier = 1f;
	public float modifier = 0f;
	public string[] tags = new string[0];
	public Temperature temperature = Temperature.None;
	public float spice = 0f;
	public string special = "";
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

	public string fileName = "";

	public static Database Instance;

//	public List<Quality> qualities;
//	public List<Ingredient> ingredients;
//	public List<Form> forms;

	public List<FoodAttribute> foodAttributes;

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		LoadData();
	}

	public void LoadData()
	{
		string loadPath = Application.dataPath + "/Data/" + fileName;
		string[] dataLines = File.ReadAllLines(loadPath);
		Dictionary<int, string> fieldLookup = new Dictionary<int, string>();
		for(int i = 0; i < dataLines.Length; i++)
		{
			if(i == 0)
			{
				string [] fieldStrings = dataLines[0].Split(',');

				for(int j = 0; j < fieldStrings.Length; j++)
				//foreach(string fieldName in fieldStrings)
				{
					fieldLookup.Add (j, fieldStrings[j]);
				}
			} else {
//				foreach(KeyValuePair<int, string> entry in fieldLookup)
//				{
//					Debug.Log (entry);
//				} 

				FoodAttribute recordAttribute = new FoodAttribute(); 
				string[] recordStrings = dataLines[i].Split(',');
				for (int j = 0; j < fieldLookup.Count; j++)
				{
					//Debug.Log (j);
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
							//recordStrings[j].Split('; ');
						break;
					case "Rank":
						recordAttribute.rank = StringToRank(recordStrings[j]);
						break;
					case "Multiplier":
						recordAttribute.multiplier = StringToFloat(recordStrings[j]);
						break;
					case "Modifier":
						recordAttribute.modifier = StringToFloat(recordStrings[j]);
						break;
					case "Spice":
						recordAttribute.spice = StringToFloat(recordStrings[j]);
						break;
					case "Special":
						recordAttribute.special = recordStrings[j];
						break;
					case "Temperature":
						recordAttribute.temperature = StringToTemperature(recordStrings[j]);
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
		case "quality":
				return AttributeType.Quality;
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
