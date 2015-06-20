using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

//public enum PlayerColor
//{
//	None = -1,
//	Red = 2,
//	Blue = 3,
//	Yellow = 4,
//	Green = 5
//}

[System.Serializable]
public class Plate
{
	public List<Food> foods = new List<Food>();
}

public class Player : MonoBehaviour {

	public static Player Instance;

	public int Id;

	private Profile _profileInstance;
	public Profile ProfileInstance
	{
		get
		{
			return _profileInstance;
		}
		set
		{

			//Debug.Log ("Profile changed.");
			if(value != null)
			{
			if(InterfaceController.Instance)
			{
				//Assign item to unlock 
				string itemToUnlock = "";
				if(!(_profileInstance == null || _profileInstance.playerName == "Guest"))
				{
					itemToUnlock = _profileInstance.playerName;
				}
				//Assign item to lock
				string itemToLock = "";
				if(value.playerName != null && value.playerName != "Guest")
				{
					itemToLock = value.playerName;
				}

				foreach(ProfileMenuHandler profileMenu in InterfaceController.Instance.activeProfileMenus)
				{
					foreach(ProfileMenuItem profileMenuItem in profileMenu.activeMenuWidgets)
					{
						if(profileMenuItem.label.text == itemToLock)
						{
							Debug.Log ("Disable item");
							profileMenuItem.label.color = InterfaceController.Instance.inactiveMenuItemColor;
							profileMenuItem.buttonEnabled = false;
						} else if(profileMenuItem.label.text == itemToUnlock)
						{
							Debug.Log ("Enable item.");
							profileMenuItem.label.color = InterfaceController.Instance.activeMenuItemColor;
							profileMenuItem.buttonEnabled = true;
						}
					}
				}
			}
			_profileInstance = value;
			}
		}
	}

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
	public float lastChoiceTimeElapsed;

	//Computer
	public static float computerDelayLowLimit = 1;
	public static float computerDelayHighLimit = 5;

	//State
	[System.NonSerialized]
	public UIWidget[] stateWidgets = new UIWidget[5];

	//UI
	public UIPanel uiPanel;
	public PlayerPanel PanelScript;

	public UISprite trayBacker;

		//Entry state
	public UISprite humanButton;
	public UISprite computerButton;
	public UILabel humanButtonLabel;
	public UILabel computerButtonLabel;
	public UILabel entryNameField;

		//Game state
	public ButtonHandler eatButton;
	public ButtonHandler passButtton;
	public UILabel scoreLabel;
	public UILabel hpLabel;
	public UILabel updateScoreLabel;
	public UILabel updateHpLabel;
	public UILabel rankingLabel;
	public UILabel playerNameLabelGame;
	//public UILabel playerNameLabelEntry;






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
		if(GameController.Instance.currentPhase == Phase.Choose && controlType == ControlType.Computer 
		   && !computerDecisionRunning && playerChoice == PlayerChoice.Ready)
		{
			computerDecisionRunning = true;
			StartCoroutine(ComputerDecision());
			Debug.Log ("Computer "+Id+" running @"+Time.frameCount);
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
					Debug.Log ("Computer "+Id+ "eat @"+Time.frameCount, this);
					Eat ();
				} else 
				{
					Debug.Log ("Computer "+Id+ "pass @"+Time.frameCount, this);
					Pass ();
				}
			} else {
				timer += Time.deltaTime;
			}
			yield return 0;
		}
		computerDecisionRunning = false;
		Debug.Log ("Computer "+Id+" stopped running. @"+Time.frameCount);
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
		Choose ();

		playerChoice = PlayerChoice.Eat;
		plate.foods.Add (new Food(GameController.Instance.activeFood));
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
		Choose ();
		playerChoice = PlayerChoice.Pass;

//		InterfaceController.Instance.WriteToOutcome("Didn't eat");
//		GameController.Instance.NextPrompt();
		AudioController.Instance.PlaySound(SoundEffect.Swoop);
	}

	public void Choose()
	{
		lastChoiceTimeElapsed = Time.time - GameController.Instance.choiceStartTime;
		EnableButtons(false);
	}

	public void EnableButtons(bool b)
	{
	//S	Debug.Log ("EnableButtons: "+b);
			InterfaceController.Instance.EnableButton(eatButton, b);
			InterfaceController.Instance.EnableButton(passButtton, b);

	}

	public void EnableUi()
	{
		uiPanel = GameObject.Find ("UI Root/Camera/PanelPlayer"+Id).GetComponent<UIPanel>();
		PanelScript = uiPanel.GetComponent<PlayerPanel>();
		PanelScript.Player = this;

		//General
		trayBacker = uiPanel.transform.Find("Backer").GetComponent<UISprite>();

		//Entry
		humanButton = uiPanel.transform.Find("EntryWidget/ButtonHuman").GetComponent<UISprite>();
		computerButton = uiPanel.transform.Find("EntryWidget/ButtonComputer").GetComponent<UISprite>();
		humanButtonLabel = uiPanel.transform.Find("EntryWidget/ButtonHuman/Label").GetComponent<UILabel>();
		computerButtonLabel = uiPanel.transform.Find("EntryWidget/ButtonComputer/Label").GetComponent<UILabel>();
		entryNameField = uiPanel.transform.Find ("EntryWidget/BackerName/Label").GetComponent<UILabel>();

		//Game
		eatButton = uiPanel.transform.FindChild("GameWidget/Eat").GetComponent<ButtonHandler>();
		passButtton = uiPanel.transform.FindChild("GameWidget/Pass").GetComponent<ButtonHandler>();
		hpLabel = uiPanel.transform.FindChild("GameWidget/FieldBacker/LabelHPDisplay").GetComponent<UILabel>();
		rankingLabel = uiPanel.transform.FindChild("GameWidget/FieldBacker/Ranking").GetComponent<UILabel>();
		scoreLabel = uiPanel.transform.FindChild("GameWidget/FieldBacker/LabelScoreDisplay").GetComponent<UILabel>();
		updateHpLabel = uiPanel.transform.FindChild("GameWidget/AreaUpdate/LabelHPUpdate").GetComponent<UILabel>();
		updateScoreLabel = uiPanel.transform.FindChild("GameWidget/AreaUpdate/LabelScoreUpdate").GetComponent<UILabel>();
		playerNameLabelGame = uiPanel.transform.Find ("GameWidget/LabelPlayer").GetComponent<UILabel>();

		//Acquire state widgets
		//UIWidget[] stateWidgets = new UIWidget[4];
		stateWidgets[0] = uiPanel.transform.Find ("JoinWidget").GetComponent<UIWidget>();
		stateWidgets[1] = uiPanel.transform.Find ("EntryWidget").GetComponent<UIWidget>();
		stateWidgets[2] = uiPanel.transform.Find ("ReadyWidget").GetComponent<UIWidget>();
		stateWidgets[3] = uiPanel.transform.Find ("GameWidget").GetComponent<UIWidget>();
		stateWidgets[4] = uiPanel.transform.Find ("InactiveWidget").GetComponent<UIWidget>(); //Dummy widget

		hpLabel.text = Hp.ToString();
		updateHpLabel.text = "";
		scoreLabel.text = Score.ToString();
		updateScoreLabel.text = "";

		this.uiPanel.gameObject.BroadcastMessage("EnablePlayerGoUi", this);
	}

	public void ChangeProfile(string profileStringId)
	{
		if(string.IsNullOrEmpty(profileStringId))
		{
			ProfileInstance = null;
		} else {

			Profile matchingProfile = null;
		foreach(Profile profile in UserDatabase.Instance.userInfo.profiles)
		{
				//Debug.Log ("Local name, inputName"+profile.playerName + ", "+profileStringId);
			if( profile.playerName == profileStringId)
			{
				matchingProfile = new Profile(profile);
				break;
			}
		}
			if(matchingProfile != null)
			{
				ProfileInstance = matchingProfile;
				//Debug.Log ("Existing profile: "+ProfileInstance.playerName+" set to player id:"+playerId);
			} else {
		Debug.Log (profileStringId + " not found, creating new profile.");
			ProfileInstance = UserDatabase.Instance.AddNewProfile(profileStringId);

			}
		}
		playerNameLabelGame.text = ProfileInstance.playerName;
		entryNameField.text = ProfileInstance.playerName;
		//Debug.Log(profileStringId+" set to "+playerId+" @"+Time.deltaTime);
		}

//	public void SetProfile()
//	{
//		var query = 
//		profileInstance = new Profile(
//	}

}
