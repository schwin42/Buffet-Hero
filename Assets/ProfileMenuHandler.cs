using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProfileMenuHandler : MonoBehaviour {

	//Inspector
	public UIPanel playerPanel;
	public GameObject menuWidgetPrefab;
	public GameObject menuInputPrefab;

	public UIGrid menuGrid;

	public List<GameObject> activeMenuWidgets = new List<GameObject>();
	public UIInput activeInput;

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
				//Debug.Log ("Destroying "+profileItem);
				Destroy(profileItem);
			}
			activeMenuWidgets.Clear();
			gameObject.SetActive(false);
		} else {
			//Populate menu
			//Debug.Log ("User profiles amount: "+UserDatabase.Instance.userInfo.profiles.Count);
			for(int i = 1; i < UserDatabase.Instance.userInfo.profiles.Count; i++)
			{
				Profile profile = UserDatabase.Instance.userInfo.profiles[i];
				GameObject profileWidget = Instantiate (menuWidgetPrefab) as GameObject;
				profileWidget.name = "_"+profile.playerName;
				UILabel profileLabel = profileWidget.GetComponentInChildren<UILabel>();
				profileWidget.transform.parent = menuGrid.transform;
				profileLabel.text = profile.playerName;
				profileWidget.transform.localScale = Vector3.one;
				profileWidget.transform.localRotation = Quaternion.identity;
				menuGrid.repositionNow = true;
				activeMenuWidgets.Add(profileWidget);
			}

			gameObject.SetActive(true);
		}

	}

	public void MakeSelectionWithButton(UIButton button)
	{
		//AudioController.Instance.PlaySound(SoundEffect.Click);
		string profileName = button.GetComponentInChildren<UILabel>().text;
		ToggleProfileMenuDisplay();
		
		player.ChangeProfile(profileName);
		
		
	}

	public void MakeSelectionWithString(string profileName)
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		//string profileName = button.GetComponentInChildren<UILabel>().text;
		ToggleProfileMenuDisplay();

		player.ChangeProfile(profileName);

	}

	public void OpenInput()
	{
		GameObject newInputGo = Instantiate(menuInputPrefab) as GameObject;
		UIInput newInput = newInputGo.GetComponentInChildren<UIInput>();
		newInput.value = "";
		activeInput = newInput;
		newInputGo.transform.parent = menuGrid.transform;
		newInputGo.transform.localScale = Vector3.one;
		newInputGo.transform.localRotation = Quaternion.identity;
		menuGrid.repositionNow = true;
		newInput.isSelected = true;
		
		//uiInput.isSelected = true;
	}

	public void Submit()
	{
		Debug.Log ("Submit");
		MakeSelectionWithString(activeInput.value);
		Destroy(activeInput.transform.parent.gameObject);
		//Debug.Log ("On submit @"+Time.frameCount);

	}

	IEnumerator WaitForSubmit()
	{
		while(activeInput.isSelected)
		{

			yield return 0;
		}
		if(!activeInput.isSelected)
		{
			Submit();
			yield break;
		}
	}
}
