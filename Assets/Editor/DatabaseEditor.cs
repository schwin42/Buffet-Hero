using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Database))]
public class DatabaseEditor : Editor {

	static Database database;

	public static GUIContent loadButtonContent = new GUIContent(
		"Load", "Load data from csv");

	public void OnEnable()
	{
		database = (Database)target;
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
