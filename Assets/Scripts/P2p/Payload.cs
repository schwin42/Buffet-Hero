using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] public abstract class Payload { }

[System.Serializable] public class DisplayResultsEvent : Payload { }
[System.Serializable] public class WelcomePayload : Payload {
	public RemotePlayer hostPlayer;
	public List<RemotePlayer> fellowClients; //Will be empty if there are no other clients yet
	public WelcomePayload (RemotePlayer hostPlayer, List<RemotePlayer> remotePlayers) {
		this.hostPlayer = hostPlayer;
		this.fellowClients = remotePlayers;
	}
}
[System.Serializable] public class PlayerJoinedPayload : Payload {
	public RemotePlayer remotePlayer; //Should never be empty
	public PlayerJoinedPayload (RemotePlayer remotePlayer) {
		this.remotePlayer = remotePlayer;
	}
}
[System.Serializable] public class PlayerLeftPayload : Payload {
	public string droppedEndpointId;
	public PlayerLeftPayload (string droppedEndpointId) {
		this.droppedEndpointId = droppedEndpointId;
	}
}
[System.Serializable] public class StartGamePayload : Payload {
	public GameSettings gameStartInfo;
	public StartGamePayload (GameSettings gameStartInfo) {
		this.gameStartInfo = gameStartInfo;
	}
}
[System.Serializable] public class GameResultPayload : Payload {
	public GameResult gameResult;
	public GameResultPayload (GameResult gameResult) {
		this.gameResult = gameResult;
	}
}
