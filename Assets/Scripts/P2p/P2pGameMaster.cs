﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.ObjectModel;

public class P2pGameMaster : MonoBehaviour {

	private static P2pGameMaster _instance;
	public static P2pGameMaster Instance {
		get {
			if(_instance == null) {
				GameObject.FindObjectOfType<P2pGameMaster>();
			}
			return _instance;
		}
	}

	//Game session records
	public List<OnlineProfile> ActiveProfiles { //Other players in this session
		get {
			return ConnectionController.Instance.AccessiblePlayers.Select(rp => rp.profile).ToList();
		}
	} 

	//Game instance info
	float timeLimit;
	ReadOnlyCollection <FoodAttribute> qualifierPool;
	ReadOnlyCollection <FoodAttribute> ingredientPool;
	ReadOnlyCollection <FoodAttribute> formPool;

	//Game instance records
	List<bool> choices;
	public Food displayedFood; //TODO Currently the interface controller requires this and
	public float currentScore; //this to be public. Fix so that this is not the case.
	int nextFoodIndex;
	public GameSettings currentSettings;

	public List<GameResult> AllGameResults {
		get {
			return new List<GameResult> (otherGameResults.Union(new List<GameResult> { myGameResult }));
		}
	}
	public List<GameResult> otherGameResults;
	public GameResult myGameResult;

	private float _timeRemaining;
	public float TimeRemaining {
		get {
			return _timeRemaining;
		}
	}

	//State
	public bool gameInProgress;
	private bool _timerIsRunning;
	public bool TimerIsRunning {
		get {
			return _timerIsRunning;
		}
	}

	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () { }
	
	// Update is called once per frame
	void Update () {
	
		if (_timerIsRunning) {
			if(_timeRemaining <= 0) {
				TimerEnded();
				_timerIsRunning = false;
			} else {
				_timeRemaining -= Time.deltaTime;
				if(_timeRemaining < 0) {
					_timeRemaining = 0;
				}
			}
		}
	}

	public void LoadGameSettings (GameSettings gameSettings)
	{
		//Validate game settings
		if (qualifierPool == null || qualifierPool.Count == 0 && gameSettings.qualifierSeed == -1) {
			P2pInterfaceController.Instance.WriteToConsole("Error in LoadGameSettings: Attribute pool is unassigned, but random seed was not provided");
		}


		P2pInterfaceController.Instance.WriteToConsole ("Beginning LoadGameSettings");
		P2pInterfaceController.Instance.WriteToConsole ("game settings: " + gameSettings.timeLimit + ", " + gameSettings.qualifierSeed + ", " + gameSettings.startFoodIndex);
		timeLimit = gameSettings.timeLimit;
		
		//Generate food lists from random seed
		if (gameSettings.qualifierSeed != -1) {
			qualifierPool = FoodLogic.GetShuffledAttributes (AttributeType.Qualifier, gameSettings.qualifierSeed);
			ingredientPool = FoodLogic.GetShuffledAttributes (AttributeType.Ingredient, gameSettings.ingredientSeed);
			formPool = FoodLogic.GetShuffledAttributes (AttributeType.Form, gameSettings.formSeed);
		}

		this.nextFoodIndex = gameSettings.startFoodIndex;

		this.currentSettings = gameSettings;
	}

	public void BeginNewGame() {
		gameInProgress = true;
		currentScore = 0;
		myGameResult = null;
		choices = new List<bool> ();
		otherGameResults = new List<GameResult> ();
		NextFood ();
		StartTimer ();
	}

	public void EndGame () {
		gameInProgress = false;
		myGameResult = new GameResult (DeviceDatabase.Instance.ProfileId, choices, currentScore);
		P2pInterfaceController.Instance.GameFinished (myGameResult);
	}

	void StartTimer() {
		_timeRemaining = timeLimit;
		_timerIsRunning = true;
	}

	void TimerEnded ()
	{
		EndGame ();
	}

	public void NextFood () {
		P2pInterfaceController.Instance.WriteToConsole ("Using current food index: " + nextFoodIndex);
		displayedFood = GetFoodFromIndex (nextFoodIndex);
		nextFoodIndex++;
	}

	public void Player_Eat () {
		choices.Add (true);
		currentScore += displayedFood.Quality.Value;

		NextFood ();
	}

	public void Player_Pass () {
		choices.Add (false);

		NextFood ();
	}

	public Food GetFoodFromIndex (int index)
	{
		Food food = new Food ();
		
		food.attributes.Add (GetAttributeFromIndex (index, formPool));
		food.attributes.Add (GetAttributeFromIndex (index, ingredientPool));
		food.attributes.Add (GetAttributeFromIndex (index, qualifierPool));
		
		food.Realize ();
		return food;
	}

	private FoodAttribute GetAttributeFromIndex (int index, ReadOnlyCollection<FoodAttribute> sourceList) {
		try {
//		P2pInterfaceController.Instance.WriteToConsole ("Getting attribute from index");
//		P2pInterfaceController.Instance.WriteToConsole ("GAFI index, source list count: " + index + ", " + (sourceList == null ? "null" : sourceList.Count.ToString()) );
		int finalIndex = index % sourceList.Count;
		return sourceList [finalIndex];
		} catch (Exception e) {
			P2pInterfaceController.Instance.WriteToConsole("Exception in GetAttributeFromIndex: " + e.Message);
			return null;
		}
	}
}

[System.Serializable] public class GameResult {
	public Guid profileId;
	public List<bool> choices = null;
	public float finalScore = -1;

	public GameResult(Guid profileId, List<bool> choices, float finalScore) {
		this.profileId = profileId;
		this.choices = choices;
		this.finalScore = finalScore;
	}
}

[System.Serializable] public class GameSettings {
	//Host sends game specifications to clients
	public float timeLimit = -1;
	public int qualifierSeed = -1;
	public int ingredientSeed = -1;
	public int formSeed = -1;
	public int startFoodIndex = -1;

	public GameSettings (float timeLimit, int startFoodIndex, bool generateRandomSeed) {
		this.timeLimit = timeLimit;
		this.startFoodIndex = startFoodIndex;

		if (generateRandomSeed) {
			this.qualifierSeed = FoodLogic.NextUnityRandomSeed;
			this.ingredientSeed = FoodLogic.NextUnityRandomSeed;
			this.formSeed = FoodLogic.NextUnityRandomSeed;
		}
	}

}
