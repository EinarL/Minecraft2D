using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSoundScript : MonoBehaviour
{
    private List<AudioClip> steppingSound;
	private float walkingSoundSpeed = 1.1f;
	private float runSoundSpeed = 1.4f;
    private string prevFolderName = ""; // the audio folder name for the block that player was previously stepping on

    private AudioSource stepAudioSource;

    // Start is called before the first frame update
    void Start()
    {
		stepAudioSource = GetComponent<AudioSource>();
        stepAudioSource.volume = 0.1f;
        OptionsManager.Instance.initializeStepsVolume(stepAudioSource);
        //loadSound("Dirt");
	}

    /**
     * plays step sound
     */
    public void playSound(string blockSteppingOn, bool isRunning)
    {
        if (stepAudioSource.isPlaying) return;

        loadSound(blockSteppingOn); // make sure its the correct sound

		float speed = walkingSoundSpeed; // walking sound speed
		if (isRunning) speed = runSoundSpeed; // running sound speed

		// get random clip to play
		var rand = new System.Random();
		int randIndex = rand.Next(steppingSound.Count);
		AudioClip randClip = steppingSound[randIndex];

        stepAudioSource.pitch = speed;
		stepAudioSource.clip = randClip;
        stepAudioSource.Play();
    }

    private void loadSound(string blockSteppingOn)
    {
        object[] folderInfo = BlockBehaviourData.getSoundFolder(blockSteppingOn); // returns {folder name, amount of sound files in the folder }
        string folderName = (string)folderInfo[0];
        int amountOfFiles = (int)folderInfo[1];
		if (folderName.Equals(prevFolderName)) return;
        steppingSound = new List<AudioClip>();
        prevFolderName = folderName;
		for (int i = 1; i <= 4; i++)
		{
			steppingSound.Add(Resources.Load($"Sounds\\Steps\\{folderName}\\{folderName}{i}") as AudioClip);
		}
        float[] pitch = BlockBehaviourData.getStepSoundPitch(blockSteppingOn); // {walking pitch, running pitch}
        walkingSoundSpeed = pitch[0];
        runSoundSpeed = pitch[1];
	}
}
