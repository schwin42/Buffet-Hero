using UnityEngine;
using System.Collections;
using System.Linq;

public enum ButtonAction
{
	None = 0,
	Eat = 1,
	Pass = 2,
	Next = 3,
	Human = 4,
	Computer = 5,
	Join = 6,
	Close = 7,
	Confirm = 8,
	Menu = 9,
	Stats = 10
}

public class ButtonHandler : MonoBehaviour {

	[System.NonSerialized]
	public  Player player;

	public ButtonAction buttonAction = ButtonAction.None;

	// Use this for initialization
	void Start () {
	
		//player = GetComponentInParent<Player>();
		Transform target = transform;
		int i = 0;
		while(player == null && i < 20) //Arbitrary number to break infinite loops
		{

			Transform parent = target.parent;

			if(parent == transform.root || parent.parent == null)
			{
				Debug.Log ("End catch");
				break;
			}

			if(parent.name.Contains("Player"))
			{
				player = GameController.Instance.possiblePlayers[ int.Parse(parent.name[parent.name.Length - 1].ToString())];
			} else {
				target = transform.parent;
			}
			i++;
		}
		Debug.Log ("Unable to find player for : "+gameObject.name+", i="+i, gameObject);

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnClick()
	{
		Debug.Log ("Click @" + GameController.Instance.currentRound);
		if(buttonAction != ButtonAction.Next){
		AudioController.Instance.PlaySound(SoundEffect.Click);
		}

		switch(buttonAction)
		{
		case ButtonAction.Eat:
			player.Eat();
			break;
		case ButtonAction.Pass:
			player.Pass();
			break;
		case ButtonAction.Next:
			Debug.Log ("Next");
			if(GameController.Instance.currentPhase == Phase.Pregame)
			{
				if((from possiblePlayer in GameController.Instance.possiblePlayers
				    where possiblePlayer.playerChoice == PlayerChoice.Ready
				    select possiblePlayer).Count() > 0)
				{
				GameController.Instance.BeginGame();
				}
				} else if(GameController.Instance.currentPhase == Phase.Evaluate)
			{
				//Debug.Log ("Current phase is evaluation");
				AudioController.Instance.PlaySound(SoundEffect.Click);
				//Debug.Log ("About to end round.");
				//GameController.Instance.BeginRound();
				GameController.Instance.EndRound();
			} else if(GameController.Instance.currentPhase == Phase.GameOver)
			{
				GameController.Instance.ReadyJoinedPlayers();
				GameController.Instance.BeginGame();
			}
			break;
		case ButtonAction.Join:
			//InterfaceController.Instance.PlayerUiStates[player.playerId] = 
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Entry);
			break;
		case ButtonAction.Close:
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Join);
			break;
		case ButtonAction.Confirm:
			player.playerChoice = PlayerChoice.Ready;
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Ready);
			break;
		case ButtonAction.Human:
			player.controlType = ControlType.Human;
			InterfaceController.Instance.HighlightControlType(player);
			break;
		case ButtonAction.Computer:
			player.controlType = ControlType.Computer;
			InterfaceController.Instance.HighlightControlType(player);
			break;
		case ButtonAction.Menu:
			InterfaceController.Instance.SetGameUiState(GameUIState.Join);
			break;
		case ButtonAction.Stats:
			InterfaceController.Instance.SetGameUiState(GameUIState.Stats0);
			break;
		default:
			Debug.LogError("Invalid button action: "+buttonAction);
			break;
		}
	}
}
