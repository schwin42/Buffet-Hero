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

	public void AssignPlayerToInterface(IPlayerAssignable iPlayerAssignable) 
	//	ButtonHandler buttonHandler)
	{
		if(player != null)
		{
			iPlayerAssignable.SetPlayer(player);
		//buttonHandler.player = player;
		} else {
			Debug.Log ("Not ideal activity for AssignPlayer");
			//Handle case in which the player is requested before it is assigned from possible players in start
			StartCoroutine("WaitForPlayerThenAssign", iPlayerAssignable);
		}
	}

	IEnumerator WaitForPlayerThenAssign (IPlayerAssignable iPlayerAssignable)
	{
		//Debug.Log ("Coroutine");
		while (player == null)
		{
			//Debug.Log ("Waiting @"+Time.frameCount);
			yield return 0;
		}

		iPlayerAssignable.SetPlayer(player);;
	}

	public void SetPanelColor(ColorScheme scheme)
	{
		playerScheme = scheme;
		backer.color = scheme.defaultColor;
		InterfaceController.Instance.HighlightControlType(player);
	}
}
