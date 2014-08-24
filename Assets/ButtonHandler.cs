using UnityEngine;
using System.Collections;

public class ButtonHandler : MonoBehaviour {

	Player player;

	public string buttonFunction;

	// Use this for initialization
	void Start () {
	
		player = GetComponentInParent<Player>();
		if(player != null)
		{
		buttonFunction = GetComponentInChildren<UILabel>().text;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnClick()
	{
		if(buttonFunction != "NEXT"){
		AudioController.Instance.PlaySound();
		}

		switch(buttonFunction)
		{
		case "EAT":
			player.Eat();
			break;
		case "PASS":
			player.Pass();
			break;
		case "NEXT":
			if(GameController.Instance.currentPhase == Phase.Evaluate)
			{
				AudioController.Instance.PlaySound();
				GameController.Instance.NextPrompt();
			}
			break;
		default:
			Debug.LogError("Invalid button action: "+buttonFunction);
			break;
		}
	}
}
