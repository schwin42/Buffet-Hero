using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class Payload {

}

[System.Serializable]
public class ProfilePayload : Payload {
	public Profile profile;

	public ProfilePayload (Profile profile) {
		this.profile = profile;
	}
}

//[System.Serializable]
//public class EventPayload : Payload {
//	public PlayerEvent playerEvent;
//
//	public EventPayload (PlayerEvent playerEvent) {
//		this.playerEvent = playerEvent;
//	}
//}

[System.Serializable]
public class StartGamePayload : Payload {

}

[System.Serializable]
public class GameFinishedPayload : Payload {
	public GameResult gameResult;

	public GameFinishedPayload(GameResult gameResult) {
		this.gameResult = gameResult;
	}
}
