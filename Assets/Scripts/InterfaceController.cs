using UnityEngine;
using System.Collections;

public class InterfaceController : MonoBehaviour {

	public static InterfaceController Instance;

	//Inspector
	public UILabel promptLabel;
	public UILabel outcomeLabel;
	public UILabel scoreLabel;

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
		promptLabel.text = s;
	}

	public void WriteToOutcome(string s)
	{
		outcomeLabel.text = s;
	}

	public void WriteToScore(float f)
	{
		scoreLabel.text = "Score: "+f.ToString();
	}
}
