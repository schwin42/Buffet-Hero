using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

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
	public float timeLimit;
	public List<FoodAttribute> qualifierPool;
	public List<FoodAttribute> ingredientPool;
	public List<FoodAttribute> formPool;

	//Game instance records
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
		timeLimit = gameSettings.timeLimit;
		
		//Generate food lists from random seed
		qualifierPool = FoodLogic.GetShuffledAttributes (AttributeType.Qualifier, gameSettings.qualifierSeed);
		ingredientPool = FoodLogic.GetShuffledAttributes (AttributeType.Ingredient, gameSettings.ingredientSeed);
		formPool = FoodLogic.GetShuffledAttributes (AttributeType.Form, gameSettings.formSeed);
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
		myGameResult = new GameResult (currentScore, eatenFoods.Count, DeviceDatabase.Instance.ProfileId);
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
		displayedFood = GetNextFoodFromPool ();

		foods.Add (displayedFood);
	}

	public void HandleEatResponse() {
		eatenFoods.Add (displayedFood);
		currentScore += displayedFood.Quality.Value;
		NextFood ();
	}

	public void HandlePassResponse() {
		passedFoods.Add (displayedFood);
		NextFood ();
	}


	Food GetNextFoodFromPool ()
	{
		Food food = new Food ();
		
		food.attributes.Add (PullAttributeAndAppend (formPool));
		food.attributes.Add (PullAttributeAndAppend (ingredientPool));
		food.attributes.Add (PullAttributeAndAppend (qualifierPool));
		
		food.Realize ();
		return food;
	}

	FoodAttribute PullAttributeAndAppend(List<FoodAttribute> pool) {
		FoodAttribute attribute = pool [0];
		pool.RemoveAt (0);
		pool.Add (attribute);
		return attribute;
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

[System.Serializable] public class GameSettings {
	//Host sends game specifications to clients
	public float timeLimit;
	public int qualifierSeed;
	public int ingredientSeed;
	public int formSeed;

	public GameSettings (float timeLimit, int qualifierSeed, int ingredientSeed, int formSeed) {
		this.timeLimit = timeLimit;
		this.qualifierSeed = qualifierSeed;
		this.ingredientSeed = ingredientSeed;
		this.formSeed = formSeed;
	}
}
