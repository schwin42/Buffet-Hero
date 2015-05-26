using UnityEngine;
using System.Collections;

public enum SoundEffect{
	None = -1,
	Click = 0,
	OrderFood = 1,
	Swoop = 2
}



public class AudioController : MonoBehaviour {

	public static AudioController Instance;

	public float volume = 1f;
	//AudioSource audioSource;

	//Inspector
	public AudioClip[] audioClips;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
	//	audioSource = GetComponent<AudioSource>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlaySound(SoundEffect soundEffect)
	{
		AudioSource.PlayClipAtPoint(audioClips[(int)soundEffect], transform.position, volume);
	}
}
