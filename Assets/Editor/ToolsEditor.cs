using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tools))]
public class ToolsEditor : Editor {

	public GUIContent derivePercentilesButton = new GUIContent("Derive Percentiles");

	public Tools tools;

	void OnEnable()
	{
		tools = (Tools) target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		DrawDefaultInspector();
		if(GUILayout.Button(derivePercentilesButton))
		{
			tools.percentiles = tools.DeriveRatingPercentiles();
		}
		serializedObject.ApplyModifiedProperties();
	}
}
