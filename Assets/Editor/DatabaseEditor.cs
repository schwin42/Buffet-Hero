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
		Database.Instance = database;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		DrawDefaultInspector();
		if(GUILayout.Button(loadButtonContent))
		{
			database.LoadTags (Application.dataPath + "/Data/" + Database.tagsFile);
			database.LoadAttributes(Application.dataPath + "/Data/" + Database.attributesFile);
		}
		serializedObject.ApplyModifiedProperties();

	}
}
