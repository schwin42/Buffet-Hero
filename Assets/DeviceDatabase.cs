﻿using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class DeviceDatabase : MonoBehaviour {
	
	private static string GetDeviceSettingsFullName() {
		return Application.persistentDataPath + "/" + "DeviceSettings.dat";
	}

	private static string GetProfileFullName(Guid profileId) {
		return Application.persistentDataPath + "/" + profileId.ToString() + ".dat";
	}

	private static DeviceDatabase _instance;
	public static DeviceDatabase Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<DeviceDatabase>();
			}
			return _instance;
		}
	}

	private DeviceSettings _deviceSettings = null;

	private List<OnlineProfile> _userProfiles = null; 
	private OnlineProfile _activeProfile = null;

	#region Settings API

	Guid ActiveProfileId {
		get {
			return _deviceSettings.activeId;
		}
		set {
			_deviceSettings.activeId = value;
			OnSettingsChanged();
		}
	}

	public OnlineProfile ActiveProfile {
		get {
			return new OnlineProfile(_activeProfile);
		}
	}

	#endregion

	#region ActivePlayer API
	public string ActivePlayerName {
		get {

			return _activeProfile.playerName;
		}
		set {
			_activeProfile.playerName = value;
			OnProfileChanged(_activeProfile);
		}
	}

	public Guid ProfileId {
		get {
			return _activeProfile.profileId;
		}
	}
	#endregion

	public void RecordGameToActiveProfile (GameResult gameResult)
	{
		_activeProfile.gamesPlayed ++;
		_activeProfile.foodsEaten += gameResult.foodsEaten;
		_activeProfile.lifetimeScore += gameResult.finalScore;
		if (_activeProfile.bestScore.HasValue) {
			if(_activeProfile.bestScore < gameResult.finalScore) _activeProfile.bestScore = gameResult.finalScore;
		} else {
			_activeProfile.bestScore = gameResult.finalScore;
		}
		if (_activeProfile.worstScore.HasValue) {
			if(_activeProfile.worstScore < gameResult.finalScore) _activeProfile.worstScore = gameResult.finalScore;
		} else {
			_activeProfile.worstScore = gameResult.finalScore;
		}

		SaveToBinaryFile(GetProfileFullName(_activeProfile.profileId), _activeProfile);

		//TODO
//		tastiestFoodEaten = sourceProfile.tastiestFoodEaten;
//		grossestFoodEaten = sourceProfile.grossestFoodEaten;
//		tastiestFoodMissed = sourceProfile.tastiestFoodMissed;
//		grossestFoodMissed = sourceProfile.grossestFoodMissed;
		
//		this.pointRating = sourceProfile.pointRating;

	}

	public event EventHandler<ProfileEventArgs> ProfileChanged;
	private void OnProfileChanged(OnlineProfile profile) {
		if (ProfileChanged != null) {
			ProfileChanged.Invoke(this, new ProfileEventArgs(profile));
		}
	}
	public class ProfileEventArgs : EventArgs {
		public OnlineProfile profile;
		public ProfileEventArgs (OnlineProfile profile) {
			this.profile = profile;
		}
	}

	public event EventHandler SettingsChanged;
	private void OnSettingsChanged() {
		P2pInterfaceController.Instance.WriteToConsole ("On settings changed");
		if (SettingsChanged != null) {
			SettingsChanged.Invoke(this, EventArgs.Empty);
		}
	}

	void Awake() {
		P2pInterfaceController.Instance.WriteToConsole ("Device database awake");
		ProfileChanged += HandleProfileChanged;
		SettingsChanged += HandleSettingsChanged;

		P2pInterfaceController.Instance.WriteToConsole ("Device database initialized events");
		LoadDeviceData ();
		InitializeDeviceData ();
	}

	void HandleSettingsChanged (object sender, EventArgs e)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Saving settings");
		SaveToBinaryFile<DeviceSettings> (GetDeviceSettingsFullName(), _deviceSettings);
	}

	void HandleProfileChanged (object sender, ProfileEventArgs e)
	{
		//Persist data to device
		P2pInterfaceController.Instance.WriteToConsole ("Saving profile: " + e.profile.playerName);
		SaveToBinaryFile<OnlineProfile> (GetProfileFullName (e.profile.profileId), e.profile);

	}

	void LoadDeviceData() {
		_userProfiles = new List<OnlineProfile> ();
		P2pInterfaceController.Instance.WriteToConsole ("Loading device data");
		string deviceSettingsFullName = GetDeviceSettingsFullName ();
		foreach (string filename in Directory.GetFiles(Application.persistentDataPath)) {
			P2pInterfaceController.Instance.WriteToConsole("Looking for filename: " + filename);
			if(filename == deviceSettingsFullName) { //Check for device settings
				P2pInterfaceController.Instance.WriteToConsole("Found device settings");
				_deviceSettings = GetObjectFromBinary<DeviceSettings>(deviceSettingsFullName);
			} else { //Must be user profile
				_userProfiles.Add(GetObjectFromBinary<OnlineProfile>(filename));
			}
		}
	}

	void InitializeDeviceData () {
		P2pInterfaceController.Instance.WriteToConsole ("Initializing device data");
		//No profiles found, create one
		if (_userProfiles.Count == 0) {
			OnlineProfile profile = new OnlineProfile();
			P2pInterfaceController.Instance.WriteToConsole("new profile id: " + profile.profileId);
			_userProfiles.Add(profile);
			SaveToBinaryFile<OnlineProfile>(GetProfileFullName(profile.profileId), profile);
		}

		//No device settings found, create one
		if (_deviceSettings == null) {
			_deviceSettings = new DeviceSettings ();
			SaveToBinaryFile<DeviceSettings> (GetDeviceSettingsFullName(), _deviceSettings);
		} 

		_activeProfile = _userProfiles.SingleOrDefault(onlineProfile => onlineProfile.profileId == _deviceSettings.activeId) ?? _userProfiles[0];
		ActiveProfileId = _activeProfile.profileId;
		P2pInterfaceController.Instance.WriteToConsole ("Completed initializing device data - user count" + _userProfiles.Count + 
		                                                ", active profile id: " + _activeProfile.profileId + ", active profile name: " + _activeProfile.playerName + ", settings active id: " + _deviceSettings.activeId);
	}

	void SaveToBinaryFile<T>(string fullFilename, T payload)
	{
		P2pInterfaceController.Instance.WriteToConsole("Saving to "+fullFilename);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(fullFilename);
		binaryFormatter.Serialize(fileStream, payload);
		fileStream.Close();
	}
	
	T GetObjectFromBinary<T>(string fullFileName)
	{
		P2pInterfaceController.Instance.WriteToConsole ("Getting object from binary for filename: " + fullFileName);
//		string fullFileName = Application.persistentDataPath+"/"+filename;
		if(File.Exists(fullFileName))
		{
			P2pInterfaceController.Instance.WriteToConsole ("Loading "+fullFileName+" from "+fullFileName);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open (fullFileName, FileMode.Open);
			T resultOutput = (T)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			P2pInterfaceController.Instance.WriteToConsole ("Object successfully loaded.");
			return resultOutput;
		} else {
			P2pInterfaceController.Instance.WriteToConsole("File not found: "+fullFileName);
			return default(T);
		}
	}
}
[System.Serializable] public class OnlineProfile
{
	public Guid profileId = Guid.Empty;
	public string playerName = "";

	//TODO
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
	public float? bestScore = null;
	public float? worstScore = null;
	//public float winPercentage = 0f;
	public FoodInfo tastiestFoodEaten = null;
	public FoodInfo grossestFoodEaten = null;
	public FoodInfo tastiestFoodMissed = null;
	public FoodInfo grossestFoodMissed = null;

	//TODO New
	public int pointRating;

	
	public OnlineProfile () {
		profileId = Guid.NewGuid ();
		playerName = "Guest";

	}
	public OnlineProfile (string profileName)
	{
		profileId = Guid.NewGuid();
		playerName = profileName;
	}
	public OnlineProfile (OnlineProfile sourceProfile)
	{
		profileId = sourceProfile.profileId;
		playerName = sourceProfile.playerName;


		gamesPlayed = sourceProfile.gamesPlayed; //TOOD Differentiate between different game modes
		foodsEaten = sourceProfile.foodsEaten;
		lifetimeScore = sourceProfile.lifetimeScore;
		bestScore = sourceProfile.bestScore;
		worstScore = sourceProfile.worstScore;
		tastiestFoodEaten = sourceProfile.tastiestFoodEaten;
		grossestFoodEaten = sourceProfile.grossestFoodEaten;
		tastiestFoodMissed = sourceProfile.tastiestFoodMissed;
		grossestFoodMissed = sourceProfile.grossestFoodMissed;

		this.pointRating = sourceProfile.pointRating;
	}
	
}

[System.Serializable] public class DeviceSettings {
	public Guid activeId = Guid.Empty;
	public DeviceSettings () {
		
	}
}



