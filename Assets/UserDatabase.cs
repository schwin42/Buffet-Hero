using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class PlayerResult
{
	public string playerName = "";
	public float score = 0f;
	public int rank = -1;
	public float remainingHp = 0f;
}

public class UserDatabase : MonoBehaviour {
	
	private static UserDatabase _instance;
	public static UserDatabase Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<UserDatabase>();
				DontDestroyOnLoad(_instance.gameObject);
			}
				return _instance;
			}
		}

	//Previous players data
	public List<PlayerResult> _playerGameRecords  = new List<PlayerResult>();
	public List<PlayerResult> PlayerGameRecords 
	{
		get
		{
			return _playerGameRecords;
		}
		set
		{
			_playerGameRecords = value;
			SaveToBinaryFile<List<PlayerResult>>("PlayerResults", _playerGameRecords);
		}
	}



	void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	void Start()
	{
		_playerGameRecords = GetPlayerResultsFromBinary();
	}

	void SaveToBinaryFile<T>(string fileName, T param)
	{
		string fullFileName = Application.persistentDataPath+"/"+fileName+".dat";
		Debug.Log("Saving "+fileName+" to "+fullFileName);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(fullFileName);
		binaryFormatter.Serialize(fileStream, param);
		fileStream.Close();
	}

	List<PlayerResult> GetPlayerResultsFromBinary()
	{
		string fileName = "PlayerResults.dat";
		string fullFileName = Application.persistentDataPath+"/"+fileName;
		if(File.Exists(fullFileName))
		{
			Debug.Log ("Loading "+fileName+" from "+fullFileName);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open (fullFileName, FileMode.Open);
			List<PlayerResult> resultOutput = (List<PlayerResult>)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			return resultOutput;
		} else {
			Debug.Log("File not found: "+fullFileName);
			return new List<PlayerResult>();
		}
	}

}
