using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

	//UIInput uiInput;



	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
		//uiInput = GetComponent<UIInput>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnSubmit()
	{
		SendMessageUpwards("Submit");
	}



}
