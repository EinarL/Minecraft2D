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
	private AudioSource stepsAudioSource;
	private int[] options; // [ musicVolume, stepsVolume ]

	private OptionsManager()
	{
		setOptions();
	}

	public void setOptions()
	{
		if(JsonDataService.Instance.exists("options.json", true)) options = JsonDataService.Instance.loadData<int[]>("options.json", true);
	}

	public void initializeMusicVolume(AudioSource musicAudioSource)
	{
		this.musicAudioSource = musicAudioSource;
		if(options != null) musicAudioSource.volume = options[0] / 100f;
	}

	public void initializeStepsVolume(AudioSource stepsAudioSource)
	{
		this.stepsAudioSource = stepsAudioSource;
		if (options != null) stepsAudioSource.volume = options[1] / 100f;
	}

	public void setMusicVolume(float volume)
	{
		musicAudioSource.volume = volume;
	}

	public void setStepsVolume(float volume)
	{
		if (stepsAudioSource == null) return;
		stepsAudioSource.volume = volume;
	}



}
