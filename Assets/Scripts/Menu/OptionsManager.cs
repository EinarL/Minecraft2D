using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager
{
	private static readonly object myLock = new object();

	private static OptionsManager instance = null;

	public static OptionsManager Instance
	{
		get
		{
			lock (myLock)
			{
				if (instance == null)
				{
					instance = new OptionsManager();
				}
				return instance;
			}
		}
	}

	private AudioSource musicAudioSource;
	private int[] options; // [ musicVolume ]

	private OptionsManager()
	{
		options = JsonDataService.Instance.loadData<int[]>("options.json", true);
	}

	public void initializeMusicVolume(AudioSource musicAudioSource)
	{
		this.musicAudioSource = musicAudioSource;
		musicAudioSource.volume = options[0] / 100f;
	}

	public void setMusicVolume(float volume)
	{
		musicAudioSource.volume = volume;
	}



}
