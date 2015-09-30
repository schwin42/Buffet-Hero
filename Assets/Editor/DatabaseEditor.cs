using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameData))]
public class DatabaseEditor : Editor {

	static GameData database;

	public static GUIContent loadButtonContent = new GUIContent(
		"Load", "Load data from csv");

	public void OnEnable()
	{
		database = (GameData)target;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		DrawDefaultInspector();
		if(GUILayout.Button(loadButtonContent))
		{
			database.LoadTags ();
			database.LoadAttributes();
		}
		serializedObject.ApplyModifiedProperties();

	}
}
