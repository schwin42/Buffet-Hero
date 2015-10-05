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
	public OnlineProfile profile;

	public RemotePlayer (string remoteEndpointId, OnlineProfile profile)
	{
		this.remoteEndpointId = remoteEndpointId;
		this.profile = profile;
	}
}

