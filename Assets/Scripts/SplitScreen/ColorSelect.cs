using UnityEngine;
using System.Collections;

public class ColorSelect : MonoBehaviour {

	//Prefabs
	public GameObject colorButtonPrefab;

	//Objects
	public UIGrid grid;

	//Status
	ColorButton selectedButton = null;

	// Use this for initialization
	void Start () {
	
		//Debug.Log ("Starting");
		foreach(ColorScheme scheme in InterfaceController.Instance.PlayerSchemesPool)
		{
			Color color = scheme.defaultColor;
			//Debug.Log ("for loop");
			//Debug.Log (colorButtonPrefab);
			GameObject colorButton = Instantiate(colorButtonPrefab) as GameObject;
			//Debug.Log (colorButton);
			//Debug.Log (grid);
			colorButton.transform.parent = grid.transform;
			colorButton.transform.localScale = Vector3.one;
			UISprite sprite = colorButton.GetComponent<UISprite>();
			sprite.color = color;
			colorButton.GetComponent<ColorButton>().scheme = scheme;

		}
		grid.repositionNow = true;
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
		SendMessageUpwards("SetPanelColor", selectedButton.scheme);
	}
}
