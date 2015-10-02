using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class ConnectionManager : MonoBehaviour {

	public P2pGameMaster gameMaster;
	public P2pInterfaceController interfaceController;

	PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
		// enables saving game progress.
		.EnableSavedGames()
			// registers a callback to handle game invitations received while the game is not running.
			.WithInvitationDelegate(HandleInvitation)
			// registers a callback for turn based match notifications received while the
			// game is not running.
			.WithMatchDelegate(HandleNotification)
			.Build();

	public void Start() {

		gameMaster = GetComponent<P2pGameMaster> ();
		interfaceController = GetComponent<P2pInterfaceController> ();

		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
	}


	public void SignIn() {
		// authenticate user:
		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			if(success) {
				interfaceController.WriteToConsole("success!");
				gameMaster.BeginNewGame();
			} else {
				interfaceController.WriteToConsole("failure!");
			}
		});
	}

	public static void HandleInvitation(GooglePlayGames.BasicApi.Multiplayer.Invitation invitation, bool value) {
		print ("invitation received!");
	}

	public static void HandleNotification(GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch match, bool value) {
		print ("match notification received!");
	}
}
