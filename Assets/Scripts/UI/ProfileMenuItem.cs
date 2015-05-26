using UnityEngine;
using System.Collections;

public class ProfileMenuItem : MonoBehaviour {

	public UILabel label;
	public UIButton button;

	public bool buttonEnabled = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnClick()
	{
		if(buttonEnabled)
		{
			SendMessageUpwards("MakeSelectionWithButton", this);
		}
	}
}
