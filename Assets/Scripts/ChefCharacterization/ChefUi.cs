using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChefUi : MonoBehaviour {

	public static ChefUi _instance;
	public static ChefUi Instance {
		get {
			if(_instance == null) {
				_instance = FindObjectOfType<ChefUi>();
			}
			return _instance;
		}
	}

	//public GameObject textGo;
	public List<GameObject> answerButtons;
	public List<string> buttonStrings;
	//public UILabel somethingelse;

	void Awake () {
		_instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisplayAnswers(List<string> buttonStrings) {
		this.buttonStrings = buttonStrings;
		if(buttonStrings.Count != answerButtons.Count) {
			print ("Different number of answers than buttons");
		}

		for (int i = 0; i < buttonStrings.Count; i++) {
			answerButtons[i].GetComponentInChildren<Text>().text = buttonStrings[i];
		}

	}

	public void HandleButtonClicked(int index) {
		string attributeId = buttonStrings[index];
//		print ("attribute id: " + attributeId);
		print ("instance: "+ChefCharacterization.Instance);
		ChefCharacterization.Instance.SubmitAttribute(attributeId);
	}

//	public void WriteToText0(string s) {
//		Text text = gameObject.GetComponentInChildren<Text>();
//	//	textGo.GetComponent<Text>().text = s;
//		text.text = s;
//	}
}
