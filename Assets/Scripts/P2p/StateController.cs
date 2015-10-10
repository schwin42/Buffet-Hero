using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class StateController : MonoBehaviour
{

	private static StateController _instance;

	public static StateController Instance {
		get {
			if (_instance == null) {
				GameObject.FindObjectOfType<StateController> ();
			}
			return _instance;
		}
	}



	//Configurable
	public const float TIME_LIMIT = 30f;




	void Awake ()
	{
		_instance = this;
	}

	// Use this for initialization
	void Start ()
	{
	


		//Debug
//		P2pInterfaceController.Instance.Results_Display ();
//		SetScreenState (AppState.ResultScreen);
	}




	
	}


