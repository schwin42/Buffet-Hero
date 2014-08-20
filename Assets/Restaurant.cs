using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public class Restaurant : MonoBehaviour {

	public string restaurantName;

//	[Range(0, 1)] public float percentageS;
//	[Range(0, 1)] public float percentageA;
//	[Range(0, 1)] public float percentageB;
//	[Range(0, 1)] public float percentageC;
//	[Range(0, 1)] public float percentageD;
//	[Range(0, 1)] public float percentageE;
//	[Range(0, 1)] public float percentageF;

	[Range(0,1)] public float[] rankPercentages = new float[7]; //In descending order of quality from S to F

	public float _aggregatePercentage;
	public float aggregatePercentage
	{
		get
		{
			return _aggregatePercentage;
		}
	}

	//public override void OnInspectorGUI()
	public void OnGUI()
	{
		float percentageSum = 0F;
		for(int i = 0; i < rankPercentages.Length; i++)
		{
			percentageSum += rankPercentages[i];
		}

		_aggregatePercentage = percentageSum;
	}

}
