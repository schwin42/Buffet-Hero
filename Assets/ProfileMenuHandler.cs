using UnityEngine;
using System.Collections;

public class ProfileMenuHandler : MonoBehaviour {

	//Inspector
	public UIPanel playerPanel;
	Player player;

	// Use this for initialization
	void Start () {
	
		player = GameController.Instance.possiblePlayers[int.Parse(playerPanel.gameObject.name.Substring(11))];

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ToggleProfileMenuDisplay()
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		if(gameObject.activeSelf)
		{
			gameObject.SetActive(false);
		} else {
			gameObject.SetActive(true);
		}

	}

	public void MakeSelection(UIButton button)
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		string profileName = button.GetComponentInChildren<UILabel>().text;
		gameObject.SetActive(false);

		InterfaceController.Instance.SetPlayerProfile(player, profileName);


	}
}
