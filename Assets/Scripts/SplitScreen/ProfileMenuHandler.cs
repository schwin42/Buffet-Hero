using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProfileMenuHandler : MonoBehaviour {

	//Inspector
	public UIPanel playerPanel;
	public GameObject menuWidgetPrefab;
	public GameObject menuInputPrefab;
	//public Color inactiveMenuItemColor;

	public UIGrid menuGrid;

	public List<ProfileMenuItem> activeMenuWidgets = new List<ProfileMenuItem>();
	public UIInput activeInput;

	//Runtime
	Player player;

	// Use this for initialization
	void Start () {
	
		player = GameController.Instance.PossiblePlayers[int.Parse(playerPanel.gameObject.name.Substring(11))];

	}

	void OnEnable()
	{
		//Debug.Log ("Profile menu enabled.");
		InterfaceController.Instance.activeProfileMenus.Add (this);
	}

	void OnDisable()
	{
		//Debug.Log ("Profile menu disabled.");
		InterfaceController.Instance.activeProfileMenus.Remove(this);
	}


	public void ToggleProfileMenuDisplay()
	{
		AudioController.Instance.PlaySound(SoundEffect.Click);
		if(gameObject.activeSelf)
		{

			foreach(ProfileMenuItem profileItem in activeMenuWidgets)
			{
				//Debug.Log ("Destroying "+profileItem);
				Destroy(profileItem.gameObject);
			}
			activeMenuWidgets.Clear();
			gameObject.SetActive(false);
			//StopCoroutine("MenuUpdate");
		} else {
			//Populate menu

			List<string> lockedNames = new List<string>();
			foreach(Player player in GameController.Instance.PossiblePlayers)
			{
				if(player.ProfileInstance != null && player.ProfileInstance.playerName != "Guest")
				{
					lockedNames.Add (player.ProfileInstance.playerName);
				}
			}

			for(int i = 1; i < UserDatabase.Instance.userInfo.profiles.Count; i++)
			{
				Profile profile = UserDatabase.Instance.userInfo.profiles[i];
				GameObject profileMenuGo = Instantiate (menuWidgetPrefab) as GameObject;

				//Debug.Log (profileMenuItem);
				profileMenuGo.name = "_"+profile.playerName;
				UILabel profileLabel = profileMenuGo.GetComponentInChildren<UILabel>();

				profileMenuGo.transform.parent = menuGrid.transform;
				profileLabel.text = profile.playerName;
				profileMenuGo.transform.localScale = Vector3.one;
				profileMenuGo.transform.localRotation = Quaternion.identity;


				//Cache components to script if applicable (for all but add new... and guest)
				ProfileMenuItem profileMenuItem = profileMenuGo.GetComponent<ProfileMenuItem>();
				if(profileMenuItem != null)
				{
					Debug.Log ("Adding to active");
				profileMenuItem.label = profileLabel;
				profileMenuItem.button = profileMenuGo.GetComponent<UIButton>();
					Debug.Log (profileMenuItem.button);
				activeMenuWidgets.Add(profileMenuItem);
				}

				//Disable button if already used
				if(
					lockedNames.Contains(profileMenuItem.label.text)
					){
				profileMenuItem.label.color = InterfaceController.Instance.inactiveMenuItemColor;
				profileMenuItem.buttonEnabled = false;
				}
			}
			menuGrid.repositionNow = true;
			gameObject.SetActive(true);
			//StartCoroutine("MenuUpdate");
		}

	}

	public void MakeSelectionWithButton(ProfileMenuItem button)
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
		Debug.Log ("Open input on "+player.Id+" @"+Time.frameCount);
		GameObject newInputGo = Instantiate(menuInputPrefab) as GameObject;
		UIInput newInput = newInputGo.GetComponentInChildren<UIInput>();
		newInput.value = "";
		activeInput = newInput;
		newInputGo.transform.parent = menuGrid.transform;
		newInputGo.transform.localScale = Vector3.one;
		newInputGo.transform.localRotation = Quaternion.identity;
		menuGrid.repositionNow = true;
		newInput.isSelected = true;
		StartCoroutine("WaitForSubmit");
		//InterfaceController.Instance.SetBlockingCollider(true);

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
		yield return 0; //Wait one frame for field to become selected
		while(activeInput.isSelected)
		{
			Debug.Log ("Selected");
			yield return 0;
		}
		if(!activeInput.isSelected)
		{
			Debug.Log ("Not selected");
			Submit();
			Destroy(activeInput.gameObject);
			//InterfaceController.Instance.SetBlockingCollider(false);
			yield break;
		}
	}

//	IEnumerator MenuUpdate()
//	{
//		while(true)
//		{
////			if(true)
////			{
////				profileLabel.color = inactiveMenuItemColor;
////				profileWidget.GetComponent<UIButton>().enabled = false;
////			}
//			yield return 0;
//		}
//	}
}
