using UnityEngine;
using System.Collections;

public class PlayerPanel : MonoBehaviour {

	public Player player;

	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
	
		player = GameController.Instance.possiblePlayers[int.Parse(gameObject.name[gameObject.name.Length - 1].ToString())];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AssignPlayerToButton(ButtonHandler buttonHandler)
	{
		buttonHandler.player = player;
	}
}
