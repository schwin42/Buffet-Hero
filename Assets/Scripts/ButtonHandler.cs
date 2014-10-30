using UnityEngine;
using System.Collections;
using System.Linq;

public interface IPlayerAssignable
{
	void SetPlayer(Player player);
}

public enum ButtonAction
{
	None = 0,
	Eat = 1,
	Pass = 2,
	Next = 3,
	Human = 4,
	Computer = 5,
	JoinGame = 6,
	CloseTray = 7,
	ConfirmEntry = 8,
	Stats0Screen = 10,
	Stats1Screen = 11,
	JoinScreen = 12,
	ClearUserData = 13,
	SettingsScreen = 14,
	ProfilesScreen = 15,
	RulesScreen = 16,
	IncrementRule  = 17,
	DecrementRule = 18,
	ResumeGame = 19,
	PauseGame = 20,
	LeaveGame = 21
}

public class ButtonHandler : MonoBehaviour, IPlayerAssignable {


	public bool isPlayerButton = true;
	public ButtonAction buttonAction = ButtonAction.None;


	public Player player;


	// Use this for initialization
	void Start () {
	
		//player = GetComponentInParent<Player>();
//		Transform target = transform;
//		int i = 0;
//		while(player == null && i < 20) //Arbitrary number to break infinite loops
//		{
//
//			Transform parent = target.parent;
//
//			if(parent == transform.root || parent.parent == null)
//			{
//				Debug.Log ("End catch");
//				break;
//			}
//
//			if(parent.name.Contains("Player"))
//			{
//				player = GameController.Instance.possiblePlayers[ int.Parse(parent.name[parent.name.Length - 1].ToString())];
//			} else {
//				target = transform.parent;
//			}
//			i++;
//		}
		//Debug.Log ("Unable to find player for : "+gameObject.name+", i="+i, gameObject);
		//Debug.Log("Sending"+gameObject.name+" upwards"+isPlayerButton);
		if(isPlayerButton) SendMessageUpwards("AssignPlayerToInterface", this);
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
		case ButtonAction.JoinGame:
			//InterfaceController.Instance.PlayerUiStates[player.playerId] = 
			//Debug.Log (player);
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Entry);
			break;
		case ButtonAction.CloseTray:
			InterfaceController.SetPlayerUiState(player, PlayerUiState.Join);
			break;
		case ButtonAction.ConfirmEntry:
			//Debug.Break();
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
//		case ButtonAction.JoinScreen:
//			InterfaceController.Instance.SetGameUiState(GameUIState.Join);
//			break;
		case ButtonAction.Stats0Screen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Stats0);
			break;
		case ButtonAction.Stats1Screen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Stats1);
			break;
		case ButtonAction.JoinScreen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Join);
			break;
		case ButtonAction.ClearUserData:
			UserDatabase.Instance.ClearUserData();
			break;
		case ButtonAction.SettingsScreen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Settings);
			break;
		case ButtonAction.RulesScreen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Rules);
			break;
		case ButtonAction.IncrementRule:
			SendMessageUpwards ("IncrementRule");
			break;
		case ButtonAction.DecrementRule:
			SendMessageUpwards ("DecrementRule");
			break;
		case ButtonAction.PauseGame:
			InterfaceController.Instance.SetPopupUiState(PopupUiState.Pause);
			break;
		case ButtonAction.ResumeGame:
			InterfaceController.Instance.SetPopupUiState(PopupUiState.NoPopup);
			break;
		case ButtonAction.LeaveGame:
			InterfaceController.Instance.SetPopupUiState(PopupUiState.NoPopup);
			GameController.Instance.TerminateGame();
			InterfaceController.Instance.SetGameUiState(GameUiState.Join);
			break;
		default:
			Debug.LogError("Invalid button action: "+buttonAction);
			break;
		}
	}

	public void SetPlayer(Player returnedPlayer)
	{
		player = returnedPlayer;
	}
}
