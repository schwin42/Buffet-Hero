using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
public enum QualitySubtype
{
	None = 0,
	Temperature = 1,
	Freshness = 2
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

	public Quality quality;
	public Ingredient ingredient;
	public Form form;
	
}

public class FoodAttribute
{
	public string name = "";
	public Rank rank = Rank.None;
	public string subtype;
	public float multiplier = 1f;
	public float modifier = 0f;
	public string[] tags = new string[0];
	public float spice = 0f;
	public string special = "";
}

[System.Serializable]
public class Ingredient : FoodAttribute
{

}

[System.Serializable]
public class Form : FoodAttribute
{

}

[System.Serializable]
public class Quality : FoodAttribute
{
	//public QualitySubtype subtype = QualitySubtype.None;
}

#endregion


public class Database : MonoBehaviour {

	public string fileName = "";

	public static Database Instance;

	public List<Quality> qualities;
	public List<Ingredient> ingredients;
	public List<Form> forms;

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
		for(int i = 0; i < dataLines.Length; i++)
		{
			Debug.Log (dataLines[i]);
		}
	}
}
