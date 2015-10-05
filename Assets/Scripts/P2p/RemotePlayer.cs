using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using GooglePlayGames.BasicApi.Nearby;

public class RemotePlayer
{

	public string remoteEndpointId;
	public Profile profile;

	public RemotePlayer (string remoteEndpointId, Profile profile)
	{
		this.remoteEndpointId = remoteEndpointId;
		this.profile = profile;
	}
}

