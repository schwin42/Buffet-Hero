using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

[System.Serializable]
public class Plate
{
	public List<Food> foods = new List<Food>();
}

public class Player : MonoBehaviour {

	public static Player Instance;

	public int playerId;

	public PlayerColor playerColor = PlayerColor.None;

	public ControlType controlType = ControlType.Human;

	public Plate plate;

	private float _hp = 0f;
	public float Hp
	{
		get
		{
			return _hp;
		}
		set
		{
			_hp = value;
			hpLabel.text = _hp.ToString("F0");
		}
	}

	public float pendingHp = 0f;

	private float _score = 0f;
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
	private int _ranking = -1;
	public int Ranking
	{
		get
		{
			return _ranking;
		}
		set
		{
			_ranking = value;
			switch(_ranking)
			{
			case 0:
				rankingLabel.text = "1st";
				break;
			case 1:
				rankingLabel.text = "2nd";
				break;
			case 2:
				rankingLabel.text = "3rd";
				break;
			case 3:
				rankingLabel.text = "4th";
				break;
			default:
			Debug.LogError ("Invalid rank: "+_ranking);
			break;
		}
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

	public UILabel rankingLabel;



	//public bool isConfirmed = false;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		eatButton = transform.FindChild("Eat").GetComponent<UIButton>();
		passButtton = transform.FindChild("Pass").GetComponent<UIButton>();

		hpLabel.text = Hp.ToString();
		updateHpLabel.text = "";
		scoreLabel.text = Score.ToString();
		updateScoreLabel.text = "";
	}

	void Update()
	{
		//Debug.Log ("Updating");
		//Computer choice
		if(GameController.Instance.currentPhase == Phase.Choose && controlType == ControlType.Computer)
		{
			//Debug.Log ("AI controlled during control phase");
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
		plate.foods.Add (new Food(GameController.Instance.activeFood));
		EnableButtons(false);
		AudioController.Instance.PlaySound(SoundEffect.OrderFood);
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
		Debug.Log ("player"+playerId+" pass");
		if(GameController.Instance.playerChoices.ContainsKey(playerId))
		{
		GameController.Instance.playerChoices.Add(playerId, false);
		} else {
			Debug.Log (playerId + " already chose.");
		}
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
