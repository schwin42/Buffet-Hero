using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

	public float wobbleFactor = 10f;
	public float timeAllotted = 10f;
	
	public bool gameInProgress;
	
	public float ScoreWobbleMultiplier {
		get {
			return FoodLogic.Instance.scoreWobbleMultiplier;
		}
		set {
			FoodLogic.Instance.scoreWobbleMultiplier = value;
		}
	}
	public List<Food> foods;
	public Food displayedFood;
	public List<Food> eatenFoods;
	public List<Food> passedFoods;
	public float currentScore;

	public List<GameResult> otherGameResults;
	public GameResult myGameResult;

	private float _timeRemaining;
	public float TimeRemaining {
		get {
			return _timeRemaining;
		}
	}

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
	void Start () {
	}
	
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

	public void BeginNewGame() {
		gameInProgress = true;
		currentScore = 0;
		myGameResult = null;
		otherGameResults = new List<GameResult> ();
		NextFood ();
		StartTimer ();
	}

	public void EndGame () {
		gameInProgress = false;
		myGameResult = new GameResult (currentScore, eatenFoods.Count, ClientDatabase.activeProfile.profileId);
		StateController.Instance.GameFinished (myGameResult);
	}

	void StartTimer() {
		_timeRemaining = timeAllotted;
		_timerIsRunning = true;
	}

	void TimerEnded ()
	{
		EndGame ();
	}

	public void NextFood () {
		displayedFood = FoodLogic.Instance.GetRandomFoodUsingQueue ();
		foods.Add (displayedFood);
	}

	public void HandleEatResponse() {
		eatenFoods.Add (displayedFood);
		print ("quality: " + displayedFood.Quality.Value);
		currentScore += displayedFood.Quality.Value;
//		ScoreWobbleMultiplier = ((Random.value * wobbleFactor) - wobbleFactor / 2) + GameData.SCORE_CONSTANT; 
		NextFood ();
	}

	public void HandlePassResponse() {
		passedFoods.Add (displayedFood);
		NextFood ();
	}


}

[System.Serializable] public class GameResult {
	public int foodsEaten;
	public float score;
	public Guid profileId;

	public GameResult(float score, int foodsEaten, Guid profileId) {
		this.score = score;
		this.foodsEaten = foodsEaten;
		this.profileId = profileId;
	}
}
