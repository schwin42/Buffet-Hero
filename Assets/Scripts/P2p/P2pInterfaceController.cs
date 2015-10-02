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
	public Transform inspector_MainPanel;
	private Text _timeRemainingText;
	private Text _foodLine0;
	private Text _foodLine1;
	private Text _foodLine2;
	private Text _score;
	private Text _console;

	void Awake() {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
		gameMaster = GetComponent<P2pGameMaster> ();

		_timeRemainingText = inspector_MainPanel.transform.Find ("TimeRemaining").GetComponent<Text>();
		_foodLine0 = inspector_MainPanel.transform.Find ("FoodLine0").GetComponent<Text>();
//		_foodLine1 = inspector_MainPanel.transform.Find ("FoodLine1").GetComponent<Text>();
//		_foodLine2 = inspector_MainPanel.transform.Find ("FoodLine2").GetComponent<Text>();
		_score = inspector_MainPanel.transform.Find ("Score").GetComponent<Text>();
		_console = inspector_MainPanel.transform.Find ("Console").GetComponent<Text>();
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
		_console.text = text + "\n" + _console.text;
	}
}
