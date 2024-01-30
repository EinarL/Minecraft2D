using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BreakBehaviour
{
	public List<AudioClip> digBlockSound = new List<AudioClip>(); // sound for while digging block
	public List<AudioClip> breakBlockSound = new List<AudioClip>(); // sound which plays when breaking block, picked at random
	public float breakingSpeed; // higher is faster breaking speed

	public BreakBehaviour(string soundName, int amountOfStepSounds, float breakingSpeed, string breakSound = null)
	{
		if (breakSound == null) breakSound = soundName;

		for (int i = 1; i <= 4; i++)
		{
			digBlockSound.Add(Resources.Load("Sounds\\Dig\\" + soundName + "" + i) as AudioClip);
		}
		for (int i = 1; i <= amountOfStepSounds; i++)
		{
			breakBlockSound.Add(Resources.Load("Sounds\\Steps\\" + breakSound + "\\" + breakSound + "" + i) as AudioClip);
		}

		this.breakingSpeed = breakingSpeed;
	}

	public virtual AudioClip getDigSound()
	{
		AudioClip randClip = digBlockSound[getRandomIndex(digBlockSound.Count)];
		return randClip;
	}

	public virtual AudioClip getBreakSound()
	{
		AudioClip randClip = breakBlockSound[getRandomIndex(breakBlockSound.Count)];
		return randClip;
	}

	public virtual float getBreakingSpeed(ToolInstance usingTool)
	{
		return breakingSpeed;
	}

	public int getRandomIndex(int listLength)
	{
		var rand = new System.Random();
		int randIndex = rand.Next(listLength);
		return randIndex;
	}
}
