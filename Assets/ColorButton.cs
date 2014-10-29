using UnityEngine;
using System.Collections;

public class ColorButton : MonoBehaviour {

	//Objects
	UISprite sprite;

	//Constant
	static int normalHeight = 35;
	static int pressedHeight = 33;
	static string normalSprite = "grey_button07";
	static string pressedSprite = "grey_button11";

	// Use this for initialization
	void Start () {
	
		sprite = GetComponent<UISprite>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPress (bool isPressed)
	{
		if(isPressed)
		{
			Down ();
		} else {
			Up ();
		}
	}

	public void OnClick ()
	{
		//Down ();
		AudioController.Instance.PlaySound(SoundEffect.Click);
		SendMessageUpwards("SelectColor", this);
	}

	public void Down()
	{
		sprite.height = pressedHeight;
		sprite.spriteName = pressedSprite;

	}

	public void Up()
	{
		sprite.height = normalHeight;
		sprite.spriteName = normalSprite;
	}

//	public void OnRelease ()
//	{
//		Debug.Log ("Released");
//		sprite.height = normalHeight;
//		sprite.spriteName = normalSprite;
//	}
}
