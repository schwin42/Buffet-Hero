using UnityEngine;
using System.Collections;

public enum ControlType
{
	None = 0,
	Human = 1,
	Computer = 2
}

public enum PlayerColor
{
	None = -1,
	Red,
	Blue,
	Yellow,
	Green
}

public class Player : MonoBehaviour {

	public static Player Instance;

	public int playerId;

	public PlayerColor playerColor = PlayerColor.None;

	public ControlType controlType = ControlType.Human;

	public float constitution = 0f;
	public float pendingConstitution = 0f;

	public float _score = 0f;
	public float Score
	{
		get
		{
			return _score;
		}
		set
		{
			_score = value;
			scoreLabel.text = _score.ToString("F0");
		}
	}
	public float pendingScore = 0f;

	public string status = "";



	//UI
	public UIButton eatButton;
	public UIButton passButtton;

	public UILabel scoreLabel;
	public UILabel hpLabel;

	public UILabel updateScoreLabel;
	public UILabel updateHpLabel;



	//public bool isConfirmed = false;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		eatButton = transform.FindChild("Eat").GetComponent<UIButton>();
		passButtton = transform.FindChild("Pass").GetComponent<UIButton>();
	}

	void Update()
	{
		//Debug.Log ("Updating");
		//Computer choice
		if(GameController.Instance.currentPhase == Phase.Choose && controlType == ControlType.Computer)
		{
			Debug.Log ("AI controlled during control phase");
			if(GameController.Instance.playerChoices.Count >= GameController.Instance.humanPlayers.Count && !GameController.Instance.humanPlayers.Contains(this))
			{
				Debug.Log ("Human players gone");
				//Choose whether to eat
				if(Random.value >= .5f)
				{

					Eat ();
				} else 
				{
					Pass ();
				}
				Debug.Log ("Chose");
			}
		}
	}

	public void Eat()
	{
		GameController.Instance.playerChoices.Add(playerId, true);
		EnableButtons(false);
//		Food food = GameController.Instance.activeFood;
//		//float foodValue = GameController.Instance.activeFood.Value;
//		float foodQuality = food.Quality;
//		score += foodQuality;
//
//		string commentary = "";
//		if(foodQuality < -100f) {
//			commentary = "You're going to be sick"; 
//		} 
//		else if(foodQuality >= -100f && foodQuality < -75f) {
//			commentary = "God awful"; 
//		} 
//		else if(foodQuality >= -75 && foodQuality < -50f){
//			commentary = "Absolutely disgusting";
//		} 
//		else if(foodQuality >= -50f && foodQuality < -25f) {
//			commentary = "Really bad"; 
//		} 
//		else if(foodQuality >= -25f && foodQuality < -0f) {
//			commentary = "Kinda gross"; 
//		} 
//		else if(foodQuality >= 0f && foodQuality < 25f){
//			commentary = "You've had better"; 
//		}
//		else if(foodQuality >= 25f && foodQuality < 50f) {
//			commentary = "Not bad."; 
//		} 
//		else if(foodQuality >= 50f && foodQuality < 75f) //A
//		{
//			commentary = "Quite good"; 
//		}
//		else if(foodQuality >= 75f && foodQuality < 100f) //A
//		{
//			commentary = "Mmm, delicious"; 
//		} 
//		else if(foodQuality >= 100) {
//			commentary = "Amazing!"; 
//		}
//		InterfaceController.Instance.WriteToOutcome(
//			//food.attributes + " * " + food.ingredient.multiplier + " * " + food.quality.multiplier + " = " + foodQuality
//		     //                                       + "\n"+
//			foodQuality + ": "+commentary );
//		InterfaceController.Instance.WriteToScore(score);
//		                                     
//		GameController.Instance.NextPrompt();
	}

	public void Pass()
	{
		GameController.Instance.playerChoices.Add(playerId, false);
		EnableButtons(false);
//		InterfaceController.Instance.WriteToOutcome("Didn't eat");
//		GameController.Instance.NextPrompt();
	}

	public void EnableButtons(bool b)
	{
	//S	Debug.Log ("EnableButtons: "+b);
			InterfaceController.Instance.EnableButton(eatButton, b);
			InterfaceController.Instance.EnableButton(passButtton, b);

	}


}
