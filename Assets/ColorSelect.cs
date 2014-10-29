using UnityEngine;
using System.Collections;

public class ColorSelect : MonoBehaviour {

	//Status
	ColorButton selectedButton = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SelectColor (ColorButton colorButton)
	{
		colorButton.Down();
		if(selectedButton)
		{
		selectedButton.Up ();
		}
		selectedButton = colorButton;
	}
}
