using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionElementsInLine : MonoBehaviour, IPlayerAssignable {

	public Player player;

	public List<GameObject> elements;

	//Constant
	static int startingPositionRight = 2;
	static int startingPositionLeft = -360;
	static int distance1 = 64;
	static int distance2 = 230;

	// Use this for initialization
	void Start () {
	
		IPlayerAssignable iPlayerAssignable = this;
		SendMessageUpwards("AssignPlayerToInterface", iPlayerAssignable);
		bool startFromRightSide = false;
		switch(player.playerId)
		{
		case 0:
			startFromRightSide = true;
			break;
		case 1:
		//	Debug.Log ("Player 1 reached");
			if(elements.Count == 3)
			{
				elements[2].transform.localEulerAngles = new Vector3(0, 0, 180);
				StartCoroutine("ZeroOutColorBarRotation");
			}
			break;
		case 2:
			break;
		case 3:
			startFromRightSide = true;
			if(elements.Count == 3)
			{
				elements[2].transform.localEulerAngles = new Vector3(0, 0, 180);
				StartCoroutine("ZeroOutColorBarRotation");
			}
			break;
		default:
			Debug.LogError("Invalid player ID: "+player.playerId);
			break;
		}
		if(player.playerId == 1 || player.playerId == 2)
		{
			//Fine, do nothing
		} else if(player.playerId == 0 || player.playerId == 3)
		{
			startFromRightSide = true;
		} else {

		}

		for(int i = 0; i < elements.Count; i++)
		{
			Debug.Log (i);
			Vector3 position = elements[i].transform.localPosition;
			switch(i)
			{
			case 0:
				elements[i].transform.localPosition = new Vector3(startFromRightSide ? startingPositionRight : startingPositionLeft, position.y, position.z);
				break;
			case 1:
				elements[i].transform.localPosition = new Vector3(startFromRightSide ? startingPositionRight - distance1 : startingPositionLeft + distance1, position.y, position.z);
				break;
			case 2:

				elements[i].transform.localPosition = new Vector3(startFromRightSide ? startingPositionRight - distance2 : startingPositionLeft + distance2, position.y, position.z);
				break;
			}
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetPlayer(Player returnedPlayer)
	{
		player = returnedPlayer;
	}

	IEnumerator ZeroOutColorBarRotation() //Workaround for color elements not being positioned correctly initially
	{
		yield return 0;
		elements[2].transform.localEulerAngles = new Vector3(0, 0, 0);
	}
}
