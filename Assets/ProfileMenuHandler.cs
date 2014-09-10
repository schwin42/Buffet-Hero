using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProfileMenuHandler : MonoBehaviour {

	//Inspector
	public UIPanel playerPanel;
	public GameObject menuWidgetPrefab;
	public UIGrid menuGrid;

	public List<GameObject> activeMenuWidgets = new List<GameObject>();

	//Runtime
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

			foreach(GameObject profileItem in activeMenuWidgets)
			{
				Debug.Log ("Destroying "+profileItem);
				Destroy(profileItem);
			}
			activeMenuWidgets.Clear();
			gameObject.SetActive(false);
		} else {
			//Populate menu
			foreach(Profile profile in UserDatabase.Instance.userInfo.profiles)
			{
				GameObject profileWidget = Instantiate (menuWidgetPrefab) as GameObject;
				profileWidget.transform.parent = menuGrid.transform;
				profileWidget.transform.localScale = Vector3.one;
				menuGrid.repositionNow = true;
				activeMenuWidgets.Add(profileWidget);
			}

			gameObject.SetActive(true);
		}

	}

	public void MakeSelectionWithButton(UIButton button)
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		string profileName = button.GetComponentInChildren<UILabel>().text;
		ToggleProfileMenuDisplay();
		
		InterfaceController.Instance.SetPlayerProfile(player, profileName);
		
		
	}

	public void MakeSelectionWithString(string profileName)
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		//string profileName = button.GetComponentInChildren<UILabel>().text;
		ToggleProfileMenuDisplay();

		InterfaceController.Instance.SetPlayerProfile(player, profileName);


	}
}
