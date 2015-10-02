﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P2pGameMaster : MonoBehaviour {

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

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
		if (_timerIsRunning) {
			if(_timeRemaining <= 0) {
				print ("Time's up!");
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
		NextFood ();
		StartTimer ();
	}

	public void EndGame () {
		gameInProgress = false;
	}

	void StartTimer() {
		_timeRemaining = timeAllotted;
		_timerIsRunning = true;
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