using UnityEngine;
using System.Collections;

public class InterfaceController : MonoBehaviour {

	public static InterfaceController Instance;

	//Inspector
	public Color disabledTextColor;
	public Color enabledTextColor;
	public UILabel[] promptLabels;
	//public UILabel outcomeLabel;
	//public UILabel scoreLabel;


	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void WriteToPrompt(string s)
	{
		foreach(UILabel label in promptLabels)
		{
			label.text = s;
		}
	}

//	public void WriteToOutcome(string s)
//	{
//		//outcomeLabel.text = s;
//	}

//	public void WriteToScore(float f)
//	{
//		//scoreLabel.text = "Score: "+f.ToString();
//	}

	public void EnableButton(UIButton uiButton, bool b)
	{
		Debug.Log("Enable button: "+uiButton.gameObject.transform.parent.gameObject);
		UILabel uiLabel = uiButton.GetComponentInChildren<UILabel>();

		if(b)
		{
			uiButton.isEnabled = true;
			uiLabel.color = enabledTextColor;
		} else {
			uiButton.isEnabled = false;
			uiLabel.color = disabledTextColor;
		}
	}

}
