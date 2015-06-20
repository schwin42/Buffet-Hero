using UnityEngine;
using System.Collections;

public class PlayerPanel : MonoBehaviour {

	public Player Player;

	private UISprite backer;

	public ColorScheme PlayerColorScheme;

	public void EnablePlayerGoUi (Player player) {
		if(player != this.Player) {
			Debug.LogError("Player mismatch");
		}

		backer = gameObject.transform.FindChild("Backer").GetComponent<UISprite>();
		//this.Player = player;
		//print("player = " + player.Id);
		SetPanelColor(InterfaceController.Instance.PlayerSchemesPool[player.Id]);
	}

	public void SetPanelColor(ColorScheme scheme)
	{
		this.PlayerColorScheme = scheme;
		backer.color = scheme.defaultColor;
		InterfaceController.Instance.HighlightControlType(Player);
	}
}
