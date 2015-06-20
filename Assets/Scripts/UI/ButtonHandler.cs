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
	JoinGame = 6,
	CloseTray = 7,
	ConfirmEntry = 8,
	Stats0Screen = 10,
	Stats1Screen = 11,
	Stats2Screen = 22,
	JoinScreen = 12,
	ClearUserData = 13,
	SettingsScreen = 14,
	ProfilesScreen = 15,
	RulesScreen = 16,
	IncrementRule  = 17,
	DecrementRule = 18,
	ResumeGame = 19,
	PauseGame = 20,
	LeaveGame = 21,
	PurchaseAdRemoval = 23
}

public class ButtonHandler : MonoBehaviour {

	public bool isPlayerButton = true;
	public ButtonAction buttonAction = ButtonAction.None;

	private Player _player;


	void Start () {
		if(isPlayerButton) {
			_player = InterfaceController.GetPlayerFromParentRecursively(transform);
		}

	}


	void Update () {}

	public void OnClick()
	{
		if(buttonAction != ButtonAction.Next){
		AudioController.Instance.PlaySound(SoundEffect.Click);
		}

		switch(buttonAction)
		{
		case ButtonAction.Eat:
			_player.Eat();
			break;
		case ButtonAction.Pass:
			_player.Pass();
			break;
		case ButtonAction.Next:
			if(GameController.Instance.currentPhase == Phase.Pregame)
			{
				if((from possiblePlayer in GameController.Instance.PossiblePlayers
				    where possiblePlayer.playerChoice == PlayerChoice.Ready
				    select possiblePlayer).Count() > 0)
				{
				GameController.Instance.BeginGame();
				}
				} else if(GameController.Instance.currentPhase == Phase.Evaluate)
			{
				AudioController.Instance.PlaySound(SoundEffect.Click);
				GameController.Instance.EndRound();
			} else if(GameController.Instance.currentPhase == Phase.GameOver)
			{
				GameController.Instance.ReadyJoinedPlayers();
				GameController.Instance.BeginGame();
			}
			break;
		case ButtonAction.JoinGame:
			print (_player.Id);
			InterfaceController.SetPlayerUiState(_player, PlayerUiState.Entry);
			break;
		case ButtonAction.CloseTray:
			InterfaceController.SetPlayerUiState(_player, PlayerUiState.Join);
			break;
		case ButtonAction.ConfirmEntry:
			_player.playerChoice = PlayerChoice.Ready;
			InterfaceController.SetPlayerUiState(_player, PlayerUiState.Ready);
			break;
		case ButtonAction.Human:
			_player.controlType = ControlType.Human;
			InterfaceController.Instance.HighlightControlType(_player);
			break;
		case ButtonAction.Computer:
			_player.controlType = ControlType.Computer;
			InterfaceController.Instance.HighlightControlType(_player);
			break;
		case ButtonAction.Stats0Screen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Stats0);
			break;
		case ButtonAction.Stats1Screen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Stats1);
			break;
		case ButtonAction.Stats2Screen:
			InterfaceController.Instance.SetGameUiState(GameUiState.Stats2);
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
		case ButtonAction.PurchaseAdRemoval:
//			Soomla.Store.SoomlaStoreIOS.BuyMarketItem("01RemoveAds", "");
			break;
		default:
			Debug.LogError("Invalid button action: "+buttonAction);
			break;
		}
	}
}
