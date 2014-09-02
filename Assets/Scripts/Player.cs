using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerChoice
{
	Inactive = -1,
	Ready = 0,
	Eat = 1,
	Pass = 2
}

public enum ControlType
{
	None = 0,
	Human = 1,
	Computer = 2
}

public enum PlayerColor
{
	None = -1,
	Red = 2,
	Blue = 3,
	Yellow = 4,
	Green = 5
}

[System.Serializable]
public class Plate
{
	public List<Food> foods = new List<Food>();
}

public class Player : MonoBehaviour {

	public static Player Instance;

	public int playerId;

	public string playerName;

	public PlayerColor playerColor = PlayerColor.None;


	public ControlType controlType = ControlType.Human;

	//public bool isActive = false;

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
	private float _pendingHp = 0f;
	public float PendingHp 
	{
		get
		{
			return _pendingHp;
		}
		set
		{
			_pendingHp = value;
			updateHpLabel.text = _pendingHp == 0 ? "" : ((_pendingHp > 0 ? "+"+_pendingHp.ToString() : _pendingHp.ToString())+" HP");
		}
	}

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
	public float _pendingScore = 0f;
	public float PendingScore 
	{
		get
		{
			return _pendingScore;
		}
		set
		{
			_pendingScore = value;
			updateScoreLabel.text = _pendingScore == 0 ? "": ((_pendingScore >= 0 ? "+"+_pendingScore.ToString() : _pendingScore.ToString())+" Pts");
		}
	}

	public string status = "";


	//Status
	public PlayerChoice playerChoice = PlayerChoice.Inactive;
	public bool computerDecisionRunning = false;
	public bool playedInLastGame = false;

	//Computer
	public static float computerDelayLowLimit = 1;
	public static float computerDelayHighLimit = 5;

	//State
	[System.NonSerialized]
	public UIWidget[] stateWidgets = new UIWidget[5];

	//UI
	public UIPanel playerPanel;

	public UISprite trayBacker;

		//Entry state
	public UIButton humanButton;
	public UIButton computerButton;
	public UIInput nameInput;

		//Game state
	public ButtonHandler eatButton;
	public ButtonHandler passButtton;
	public UILabel scoreLabel;
	public UILabel hpLabel;
	public UILabel updateScoreLabel;
	public UILabel updateHpLabel;
	public UILabel rankingLabel;
	public UILabel playerNameLabel;






	//public bool isConfirmed = false;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
	}

	void Update()
	{
		//Debug.Log ("Updating");
		//Computer choice
		if(GameController.Instance.currentPhase == Phase.Choose && controlType == ControlType.Computer 
		   && !computerDecisionRunning && playerChoice == PlayerChoice.Ready)
		{
			computerDecisionRunning = true;
			StartCoroutine(ComputerDecision());
			Debug.Log ("Computer "+playerId+" running @"+Time.frameCount);
			//Debug.Log ("AI controlled during control phase");

		}
	}

	IEnumerator ComputerDecision()
	{
		float timer = 0;
		float decisionDelay = (computerDelayHighLimit - computerDelayLowLimit) * Random.value + computerDelayLowLimit; 

		while(GameController.Instance.currentPhase == Phase.Choose && computerDecisionRunning && playerChoice == PlayerChoice.Ready)
		{
			if( timer >= decisionDelay)
			{
				if(Random.value >= .5f)
				{
					Debug.Log ("Computer "+playerId+ "eat @"+Time.frameCount, this);
					Eat ();
				} else 
				{
					Debug.Log ("Computer "+playerId+ "pass @"+Time.frameCount, this);
					Pass ();
				}
			} else {
				timer += Time.deltaTime;
			}
			yield return 0;
		}
		computerDecisionRunning = false;
		Debug.Log ("Computer "+playerId+" stopped running. @"+Time.frameCount);
		yield break;

		//If game is not in choose phase, end coroutine immediately
//		if(GameController.Instance.currentPhase != Phase.Choose)
//		{
//			Debug.Log ("Not in choose phase, stopping @"+Time.frameCount);
//			computerDecisionRunning = false;
//			yield break;
//		} else {
//			Debug.Log ("In choose phase, proceeding @"+Time.frameCount);
//		}
//		float decisionDelay = (computerDelayHighLimit - computerDelayLowLimit) * Random.value + computerDelayLowLimit; 
//		yield return new WaitForSeconds(decisionDelay);
//		if(Random.value >= .5f)
//		{
//			Debug.Log ("Computer "+playerId+ "eat @"+Time.frameCount, this);
//			Eat ();
//		} else 
//		{
//			Debug.Log ("Computer "+playerId+ "pass @"+Time.frameCount, this);
//			Pass ();
//		}
//		computerDecisionRunning = false;
	}

	public void Eat()
	{
		playerChoice = PlayerChoice.Eat;
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
		playerChoice = PlayerChoice.Pass;
		EnableButtons(false);
//		InterfaceController.Instance.WriteToOutcome("Didn't eat");
//		GameController.Instance.NextPrompt();
		AudioController.Instance.PlaySound(SoundEffect.Swoop);
	}

	public void EnableButtons(bool b)
	{
	//S	Debug.Log ("EnableButtons: "+b);
			InterfaceController.Instance.EnableButton(eatButton, b);
			InterfaceController.Instance.EnableButton(passButtton, b);

	}

	public void EnableUi()
	{
		playerPanel = GameObject.Find ("UI Root/Camera/PanelPlayer"+playerId).GetComponent<UIPanel>();

		//UI

		//General
		trayBacker = playerPanel.transform.Find("Backer").GetComponent<UISprite>();

		//Entry
		humanButton = playerPanel.transform.Find("EntryWidget/ButtonHuman").GetComponent<UIButton>();
		computerButton = playerPanel.transform.Find("EntryWidget/ButtonComputer").GetComponent<UIButton>();
		nameInput = playerPanel.transform.Find ("EntryWidget/BackerName/Label").GetComponent<UIInput>();


		//Game
		eatButton = playerPanel.transform.FindChild("GameWidget/Eat").GetComponent<ButtonHandler>();
		passButtton = playerPanel.transform.FindChild("GameWidget/Pass").GetComponent<ButtonHandler>();
		eatButton.player = this;
		passButtton.player = this;
		hpLabel = playerPanel.transform.FindChild("GameWidget/FieldBacker/LabelHPDisplay").GetComponent<UILabel>();
		rankingLabel = playerPanel.transform.FindChild("GameWidget/FieldBacker/Ranking").GetComponent<UILabel>();
		scoreLabel = playerPanel.transform.FindChild("GameWidget/FieldBacker/LabelScoreDisplay").GetComponent<UILabel>();
		updateHpLabel = playerPanel.transform.FindChild("GameWidget/AreaUpdate/LabelHPUpdate").GetComponent<UILabel>();
		updateScoreLabel = playerPanel.transform.FindChild("GameWidget/AreaUpdate/LabelScoreUpdate").GetComponent<UILabel>();
		playerNameLabel = playerPanel.transform.Find ("GameWidget/LabelPlayer").GetComponent<UILabel>();


		//Acquire state widgets
		//UIWidget[] stateWidgets = new UIWidget[4];
		stateWidgets[0] = playerPanel.transform.Find ("JoinWidget").GetComponent<UIWidget>();
		stateWidgets[1] = playerPanel.transform.Find ("EntryWidget").GetComponent<UIWidget>();
		stateWidgets[2] = playerPanel.transform.Find ("ReadyWidget").GetComponent<UIWidget>();
		stateWidgets[3] = playerPanel.transform.Find ("GameWidget").GetComponent<UIWidget>();
		stateWidgets[4] = playerPanel.transform.Find ("InactiveWidget").GetComponent<UIWidget>(); //Dummy widget
		//stateWidgets = stateWidgets;

		
		hpLabel.text = Hp.ToString();
		updateHpLabel.text = "";
		scoreLabel.text = Score.ToString();
		updateScoreLabel.text = "";
	}

}
