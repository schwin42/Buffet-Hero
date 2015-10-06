using UnityEngine;
using System.Collections;

[System.Serializable] public abstract class Payload { }

[System.Serializable] public class DisplayResultsEvent : Payload { }
[System.Serializable] public class StartGamePayload : Payload {
	public GameSettings gameStartInfo;
	public StartGamePayload (GameSettings gameStartInfo) {
		this.gameStartInfo = gameStartInfo;
	}
}
[System.Serializable] public class ProfilePayload : Payload {
	public OnlineProfile profile;
	public ProfilePayload (OnlineProfile profile) {
		this.profile = profile;
	}
}
[System.Serializable] public class GameResultPayload : Payload {
	public GameResult gameResult;
	public GameResultPayload (GameResult gameResult) {
		this.gameResult = gameResult;
	}
}
