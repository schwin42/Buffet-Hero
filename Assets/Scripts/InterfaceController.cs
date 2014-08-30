using UnityEngine;
using System.Collections;

public class InterfaceController : MonoBehaviour {

	public static InterfaceController Instance;

	//Inspector
	public Color disabledTextColor;
	public Color enabledTextColor;
	public Color neutralLightColor;
	public Color neutralDarkColor;
	public Color[] colors; 
	public GameObject promptPrefab;
	public Vector3 localPromptPosition = new Vector3(0, -105, 0);

	//Status
	UILabel[] activePrompts = new UILabel[2];


	//public UIPanel[] playerPanels;

	public UILabel[] roundLabels;

	public UIPanel[] mirrorPanels;


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

	public void DisplayPrompt(string s)
	{
		foreach(UILabel label in activePrompts)
		{
			Destroy(label);
		}

		for(int i = 0; i < 2; i++)
		{
			GameObject prompt = Instantiate(promptPrefab) as GameObject;
			prompt.transform.parent = mirrorPanels[i].transform;
			prompt.transform.localScale = Vector3.one;
			prompt.transform.localEulerAngles = Vector3.zero;
			prompt.transform.localPosition = localPromptPosition;
			UILabel promptLabel = prompt.GetComponent<UILabel>();
			activePrompts[i] = promptLabel;
			promptLabel.color = i > 0 ? neutralLightColor : neutralDarkColor;
			promptLabel.text = s;
			AudioController.Instance.PlaySound(SoundEffect.Swoop);
		}

		//foreach(UILabel label in promptLabels)
		//{
		//	label.text = s;
		//}
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
		//Debug.Log("Enable button: "+uiButton.gameObject.transform.parent.gameObject);
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

	public void DisplayRound()
	{
		foreach(UILabel label in roundLabels)
		{
			label.text = (GameController.Instance.currentRound+1).ToString()+"/ "+GameController.Instance.numberOfRounds;
		}
	}

}
