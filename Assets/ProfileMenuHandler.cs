using UnityEngine;
using System.Collections;

public class ProfileMenuHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
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
		gameObject.SetActive(false);
	}
}
