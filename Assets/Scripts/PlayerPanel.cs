using UnityEngine;
using System.Collections;

public class PlayerPanel : MonoBehaviour {

	public Player player = null;

	//Inspector
	public UISprite backer;

	//Instance
	public ColorScheme playerScheme;


	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
	

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AssignPlayerToButton(ButtonHandler buttonHandler)
	{
		if(player != null)
		{
		buttonHandler.player = player;
		} else {
			//Handle case in which the player is requested before it is assigned from possible players in start
			StartCoroutine("WaitForPlayerThenAssign", buttonHandler);
		}
	}

	IEnumerator WaitForPlayerThenAssign (ButtonHandler buttonHandler)
	{
		//Debug.Log ("Coroutine");
		while (player == null)
		{
			//Debug.Log ("Waiting @"+Time.frameCount);
			yield return 0;
		}

		buttonHandler.player = player;
	}

	public void SetPanelColor(ColorScheme scheme)
	{
		playerScheme = scheme;
		backer.color = scheme.defaultColor;
		InterfaceController.Instance.HighlightControlType(player);
	}
}
