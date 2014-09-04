using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerResult
{
	public string playerName = "";
	public float score = 0f;
	public int rank = -1;
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
	public List<PlayerResult> playerGameRecords = new List<PlayerResult>();


	void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
