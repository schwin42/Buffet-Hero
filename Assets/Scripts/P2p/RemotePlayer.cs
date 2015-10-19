using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using GooglePlayGames.BasicApi.Nearby;

public enum ProfileStatus {
	Uninitialized = -1,
	EmptyParticipant = 0,
	RemoteEndpoint = 1,
	CompleteParticipant = 2,
}

[System.Serializable] public class RemotePlayer
{
	public string gpgId;
	public OnlineProfile profile = null;
	public ProfileStatus profileStatus = ProfileStatus.Uninitialized;

	public RemotePlayer (string gpgId, OnlineProfile profile, bool isParticipant) //isParticipant as opposed to RemoteEndpoint
	{
		this.gpgId = gpgId;
		this.profile = profile;

		if(isParticipant) {
			profileStatus = ProfileStatus.CompleteParticipant;
		} else {

			profileStatus = ProfileStatus.RemoteEndpoint;
		}
	}

	public RemotePlayer (string participantId) {
		this.gpgId = participantId;
		profileStatus = ProfileStatus.EmptyParticipant;
	}

}

