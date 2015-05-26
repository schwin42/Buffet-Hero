using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChefUi : MonoBehaviour {

	public static ChefUi Instance;

	public GameObject textGo;
	//public UILabel somethingelse;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void WriteToText0(string s) {
		Text text = gameObject.GetComponentInChildren<Text>();
	//	textGo.GetComponent<Text>().text = s;
		text.text = s;
	}
}
