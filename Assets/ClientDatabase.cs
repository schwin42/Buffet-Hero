using UnityEngine;
using System.Collections;
using System;

public class ClientDatabase : MonoBehaviour {
	
	public static OnlineProfile activeProfile = new OnlineProfile ("Guest profile");
}

[System.Serializable]
public class OnlineProfile
{
	public Guid profileId = Guid.Empty;
	public string playerName = "";
	public int gamesPlayed = 0;
	public int foodsEaten = 0;
	public float lifetimeScore = 0f;
	public float AverageGameScore
	{
		get
		{
			if(gamesPlayed > 0)
			{
				return lifetimeScore / gamesPlayed;
			} else {
				return 0;
			}
		}
	}
	public float AverageFoodScore
	{
		get
		{
			return foodsEaten > 0 ? lifetimeScore / foodsEaten : 0;
		}
	}
	public float bestScore = 0f;
	public float worstScore = 0f;
	//public float winPercentage = 0f;
	public Food tastiestFoodEaten = null;
	public Food grossestFoodEaten = null;
	public Food tastiestFoodMissed = null;
	public Food grossestFoodMissed = null;
	
	public OnlineProfile () {}
	public OnlineProfile (string profileName)
	{
		profileId = Guid.NewGuid();
		playerName = profileName;
	}
	public OnlineProfile (OnlineProfile otherProfile)
	{
		profileId = otherProfile.profileId;
		playerName = otherProfile.playerName;
		gamesPlayed = otherProfile.gamesPlayed;
		foodsEaten = otherProfile.foodsEaten;
		lifetimeScore = otherProfile.lifetimeScore;
		bestScore = otherProfile.bestScore;
		worstScore = otherProfile.worstScore;
		tastiestFoodEaten = otherProfile.tastiestFoodEaten;
		grossestFoodEaten = otherProfile.grossestFoodEaten;
		tastiestFoodMissed = otherProfile.tastiestFoodMissed;
		grossestFoodMissed = otherProfile.grossestFoodMissed;
	}
	
}

