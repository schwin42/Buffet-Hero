using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class P2pInterfaceController : MonoBehaviour {

	private static P2pInterfaceController _instance;
	public static P2pInterfaceController Instance {
		get {
			if(_instance == null) {
				_instance = FindObjectOfType<P2pInterfaceController>();
			}
			return _instance;
		}
	}

	private P2pGameMaster gameMaster;
	public Transform inspector_UiRoot;
	private Text _timeRemainingText;
	private Text _foodLine0;
	private Text _foodLine1;
	private Text _foodLine2;
	private Text _score;
	private Text _console;

	void Awake() {
		_instance = this;
		gameMaster = GetComponent<P2pGameMaster> ();
	}

	// Use this for initialization
	void Start () {

		_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text>();

		_timeRemainingText = inspector_UiRoot.transform.Find ("GameScreen/TimeRemaining").GetComponent<Text>();
		_foodLine0 = inspector_UiRoot.transform.Find ("GameScreen/FoodLine0").GetComponent<Text>();
		_score = inspector_UiRoot.transform.Find ("GameScreen/Score").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameMaster.gameInProgress) {
			if (gameMaster.TimerIsRunning) {
				_timeRemainingText.text = gameMaster.TimeRemaining.ToString ("F2");
			}
			if (gameMaster.displayedFood.attributes.Count != 0) {
				_foodLine0.text = gameMaster.displayedFood.Name;
//				_foodLine1.text = gameMaster.displayedFood.attributes [1].Id;
//				_foodLine2.text = gameMaster.displayedFood.attributes [2].Id;
			}
			_score.text = "Score: " + gameMaster.currentScore.ToString();
		}
	}

	public void WriteToConsole(string text) {
		if (_console == null) {
			_console = inspector_UiRoot.transform.Find ("Console/Text").GetComponent<Text>();
		}
		print ("Console: " + text);
		_console.text = text + "\n" + _console.text;
	}
}
