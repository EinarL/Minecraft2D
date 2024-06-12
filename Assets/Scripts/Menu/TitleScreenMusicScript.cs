using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenMusicScript : MonoBehaviour
{
	private AudioClip[] mainMenuMusic;
	private AudioSource audioSource;
	private int lastPlayedIndex = -1;

	// Start is called before the first frame update
	void Start()
	{
		mainMenuMusic = Resources.LoadAll<AudioClip>("Sounds\\Music\\Menu");
		audioSource = GetComponent<AudioSource>();
		OptionsManager.Instance.initializeMusicVolume(audioSource);

		PlayRandomMusic();
	}

	// Method to play random music from the list
	void PlayRandomMusic()
	{
		if (mainMenuMusic.Length == 0)
			return;

		int newIndex;
		newIndex = Random.Range(0, mainMenuMusic.Length);
		if(newIndex == lastPlayedIndex)
		{
			newIndex = lastPlayedIndex >= 1 ? lastPlayedIndex-1 : 1;
		}
		

		lastPlayedIndex = newIndex;
		audioSource.clip = mainMenuMusic[newIndex];
		audioSource.Play();
		StartCoroutine(CheckForEndOfMusic());
	}

	// Coroutine to check when the current music ends
	IEnumerator CheckForEndOfMusic()
	{
		while (audioSource.isPlaying)
		{
			yield return new WaitForSeconds(5);
		}
		PlayRandomMusic();
	}
}
