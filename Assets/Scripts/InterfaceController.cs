using UnityEngine;
using System.Collections;

public class InterfaceController : MonoBehaviour {

	public static InterfaceController Instance;

	//Inspector
	public Color disabledTextColor;
	public Color enabledTextYellowColor;
	public Color neutralLightColor;
	public Color neutralDarkColor;
	public Color[] colors; 
	public GameObject promptPrefab;
	public GameObject letterRankPrefab;
	public Vector3 localPromptPosition = new Vector3(0, -105, 0);
	public Vector3 localLetterRankPosition;
	public Color[] letterRankColors = new Color[7];

	//Status
	public UILabel[] activePrompts = new UILabel[2];
	GameObject[] activeFoodRanks = new GameObject[2];


	//public UIPanel[] playerPanels;

	public UILabel[] roundLabels;

	public UIPanel[] mirrorPanels;

	public UIButton[] nextButtons;




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
			promptLabel.color = neutralDarkColor;
				//i > 0 ? neutralLightColor : neutralDarkColor;
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

	public void EnableButton(ButtonHandler buttonHandler, bool b)
	{
		UIButton uiButton = buttonHandler.GetComponent<UIButton>();
		//Debug.Log("Enable button: "+uiButton.gameObject.transform.parent.gameObject);
		UILabel uiLabel = uiButton.GetComponentInChildren<UILabel>();

		if(b)
		{
			uiButton.isEnabled = true;
			uiLabel.color = enabledTextYellowColor;
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

	public void ShowFoodRank(LetterRank letterRank)
	{
		for(int i = 0; i < mirrorPanels.Length; i++)
		{
		GameObject letterRankGo = Instantiate(letterRankPrefab) as GameObject;

			letterRankGo.transform.parent = mirrorPanels[i].transform;
			letterRankGo.transform.localScale = Vector3.one;
			letterRankGo.transform.localRotation = Quaternion.identity;
			letterRankGo.transform.localPosition = localLetterRankPosition;
			letterRankGo.GetComponentInChildren<UILabel>().text = letterRank.ToString();
			letterRankGo.GetComponent<UISprite>().color = letterRankColors[(int)letterRank];
			activeFoodRanks[i] = letterRankGo;

		}
	}

	public void HidePrompts()
	{
		Debug.Log ("Hiding prompts");
		foreach(UILabel label in activePrompts)
		{
			Destroy (label.gameObject);
		}
		
	}

	public void HideFoodRank()
	{
		foreach(GameObject go in activeFoodRanks)
		{
			Destroy (go);
		}

	}

	public void EnableNextButtons(bool b)
	{
		foreach(UIButton button in nextButtons)
		{
			button.isEnabled = b;
			if(b)
			{
				button.GetComponentInChildren<UILabel>().color = neutralDarkColor;
			} else {
				button.GetComponentInChildren<UILabel>().color = disabledTextColor;
			}
		}
	}

}
