using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

	UIInput uiInput;

	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
		uiInput = GetComponent<UIInput>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OpenInput()
	{
		uiInput.isSelected = true;
	}

	public void OnSubmit()
	{
		Debug.Log ("On submit @"+Time.frameCount);
		SendMessageUpwards("MakeSelectionWithString", uiInput.value);
	}
}
