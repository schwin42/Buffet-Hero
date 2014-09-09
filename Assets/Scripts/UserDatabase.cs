﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Profile
{
	public int profileId = -1;
	public string playerName = "";
	public int gamesPlayed = 0;
	public float averageScore = 0f;
	public float bestScore = 0f;
	public float worstScore = 0f;
	public float winPercentage = 0f;

}

[System.Serializable]
public class UserInfo
{
	public List<Profile> profiles;

	public int totalGamesPlayed = 0;
	public List<PlayerResult> playerGameResults = new List<PlayerResult>();
	public FoodResult tastiestFoodEaten;
	public FoodResult grossestFoodEaten;
	public FoodResult tastiestFoodMissed;
	public FoodResult grossestFoodMissed;
}

[System.Serializable]
public class FoodResult
{
	public Food food;
	public List<int> playerId;
	public bool wasEaten;

}

[System.Serializable]
public class PlayerResult
{
	//public string playerName = "";
	public string playerStringId = "";
	public float score = 0f;
	public int rank = -1;
	public float remainingHp = 0f;
	public int gameId = -1;
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
	private UserInfo _userInfo = new UserInfo();
	public UserInfo userInfo 
	{
		get
		{
			return _userInfo;
		}

	}

//	private List<PlayerResult> _playerGameRecords  = new List<PlayerResult>();
//	public List<PlayerResult> PlayerGameRecords 
//	{
//		get
//		{
//			return _playerGameRecords;
//		}
//		set
//		{
//			_playerGameRecords = value;
//			SaveToBinaryFile<List<PlayerResult>>("PlayerResults", _playerGameRecords);
//		}
//	}



	void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		}
	}

	void Start()
	{
		_userInfo = GetUserInfoFromBinary();
		//_playerGameRecords = GetPlayerResultsFromBinary();
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

	UserInfo GetUserInfoFromBinary()
	{

		string fileName = "UserInfo.dat";
		string fullFileName = Application.persistentDataPath+"/"+fileName;
		if(File.Exists(fullFileName))
		{
			Debug.Log ("Loading "+fileName+" from "+fullFileName);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open (fullFileName, FileMode.Open);
			UserInfo resultOutput = (UserInfo)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			return resultOutput;
		} else {
			Debug.Log("File not found: "+fullFileName);
			return new UserInfo();
		}
	}

	public void LogGame(List<PlayerResult> newGameEntries)
	{
		_userInfo.totalGamesPlayed ++;
		_userInfo.playerGameResults.AddRange(newGameEntries);
		SaveToBinaryFile<UserInfo>("UserInfo", _userInfo);
	}

}