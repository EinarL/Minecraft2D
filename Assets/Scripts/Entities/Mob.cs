using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Mob : Entity
{
	private AudioClip[] saySounds = new AudioClip[3];
	private AudioClip[] hurtSounds = new AudioClip[2]; // the sound that the mob makes when it takes damage
	private AudioClip deathSound;
	public AudioSource sayAudioSource;
	private float makeNoiseChance = 0.04f;
	protected bool isHunting = false; // this is true when the mob is hunting the player


	private new void Update()
	{
		if(!isHunting) base.Update();
		else
		{

		}
	}

	public override void takeDamage(float damage, float playerXPos)
	{
		if (anim.GetBool("isDead")) return;
		base.takeDamage(damage, playerXPos);
		if(health > 0) makeHurtNoise();
		else makeDeathNoise();
	}

	public IEnumerator decideIfMakeNoise()
	{
		while (true)
		{
			float rand = Random.value;
			if (rand < makeNoiseChance)
			{
				makeNoise();
			}
			yield return new WaitForSeconds(2.5f); // Wait
		}
	}

	private void makeDeathNoise()
	{
		sayAudioSource.clip = deathSound;
		sayAudioSource.Play();
	}

	private void makeHurtNoise()
	{
		var random = new System.Random();
		int randIndex = random.Next(hurtSounds.Length);
		AudioClip randClip = hurtSounds[randIndex];
		sayAudioSource.clip = randClip;
		sayAudioSource.Play();
	}

	protected void makeNoise()
	{
		var random = new System.Random();
		int randIndex = random.Next(saySounds.Length);
		AudioClip randClip = saySounds[randIndex];
		sayAudioSource.clip = randClip;
		sayAudioSource.Play();
	}

	// A coroutine that checks the walking condition every 3 seconds
	public override IEnumerator decideIfWalk()
	{
        while (true)
        {
			if (!isHunting)
			{
				// Generate a random number between 0 and 1
				float random = Random.value;

				// If the random number is less than the walk chance, start walking
				if (random < startWalkingChance && !isWalking && !justWalked)
				{
					isWalking = true;
					anim.SetBool("isWalking", true);
					float randomDirection = Random.value;
					// Generate a random direction for the entity's movement
					if (randomDirection < 0.5) direction = new Vector2(1, 0).normalized;
					else direction = new Vector2(-1, 0).normalized;
					faceDirection();

					yield return new WaitForSeconds(Random.Range(walkingTime[0], walkingTime[1])); // walk for a random amount of time
					justWalked = true;
				}
				else
				{
					yield return new WaitForSeconds(3f); // Wait for 3 seconds before checking again
					justWalked = false;
				}
				if(!isHunting) stopWalking();
			}
			else yield return new WaitForSeconds(3f);
		}
	}

	public virtual void initializeAudio()
	{
		gameObject.name = gameObject.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name

		for (int i = 0; i < saySounds.Length; i++)
		{
			saySounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\say{i+1}");
		}
		for (int i = 0; i < hurtSounds.Length; i++)
		{
			hurtSounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\hurt{i + 1}");
		}
		deathSound = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\death");
	}





}
