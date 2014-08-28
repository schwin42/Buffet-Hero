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
		Debug.Log ("Click @" + GameController.Instance.currentRound);
		if(buttonFunction != "NEXT"){
		AudioController.Instance.PlaySound(SoundEffect.Click);
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
			Debug.Log ("Next");
			if(GameController.Instance.currentPhase == Phase.Uninitialized)
			{
				Debug.Log ("Current phase is evaluation");
				AudioController.Instance.PlaySound(SoundEffect.Click);
				Debug.Log ("About to end round.");
				GameController.Instance.BeginRound();
				//GameController.Instance.EndRound();
			}
			break;
		default:
			Debug.LogError("Invalid button action: "+buttonFunction);
			break;
		}
	}
}
