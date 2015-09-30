using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionElementsInLine : MonoBehaviour {

	//Assignable
	public List<GameObject> elements;

	static readonly int STARTING_POSITION_RIGHT = 2;
	static readonly int STARTING_POSITION_LEFT = -360;
	static readonly int DISTANCE1 = 64;
	static readonly int DISTANCE2 = 230;



	public void Start () {

		int playerId = InterfaceController.GetPlayerFromParentRecursively(transform).Id;

		bool startFromRightSide = false;
		switch(playerId)
		{
		case 0:
			startFromRightSide = true;
			break;
		case 1:
			//	Debug.Log ("Player 1 reached");
			if(elements.Count == 3)
			{
				elements[2].transform.localEulerAngles = new Vector3(0, 0, 180);
				//StartCoroutine("ZeroOutColorBarRotation");
			}
			break;
		case 2:
			break;
		case 3:
			startFromRightSide = true;
			if(elements.Count == 3)
			{
				elements[2].transform.localEulerAngles = new Vector3(0, 0, 180);
				//StartCoroutine("ZeroOutColorBarRotation");
			}
			break;
		default:
			Debug.LogError("Invalid player ID: " + playerId);
			break;
		}
		if(playerId == 1 || playerId == 2)
		{
			//Fine, do nothing
		} else if(playerId == 0 || playerId == 3)
		{
			startFromRightSide = true;
		} else {
			
		}
		
		for(int i = 0; i < elements.Count; i++)
		{
			//Debug.Log (i);
			Vector3 position = elements[i].transform.localPosition;
			switch(i)
			{
			case 0:
				elements[i].transform.localPosition = new Vector3(startFromRightSide ? STARTING_POSITION_RIGHT : STARTING_POSITION_LEFT, position.y, position.z);
				break;
			case 1:
				elements[i].transform.localPosition = new Vector3(startFromRightSide ? STARTING_POSITION_RIGHT - DISTANCE1 : STARTING_POSITION_LEFT + DISTANCE1, position.y, position.z);
				break;
			case 2:
				
				elements[i].transform.localPosition = new Vector3(startFromRightSide ? STARTING_POSITION_RIGHT - DISTANCE2 : STARTING_POSITION_LEFT + DISTANCE2, position.y, position.z);
				break;
				
			}
		}

	}

	IEnumerator ZeroOutColorBarRotation() //Workaround for color elements not being positioned correctly initially
	{
		yield return 0;
		elements[2].transform.localEulerAngles = new Vector3(0, 0, 0);
	}
}
