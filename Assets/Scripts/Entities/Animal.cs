using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Animal : Entity
{
	private float walkSpeed = 4;
	protected float runSpeed = 6;
	private float runTime = 10; // how many seconds the animal runs after being hit
	private float runCounter = 0;
	protected bool isRunning = false;
	private float runningChangeDirectionChance = 0.6f;


	private new void Update()
	{
		if (!isRunning) base.Update();
		else // if the animal is running
		{
			run();
			

			runCounter += Time.deltaTime;
			if(runCounter > runTime)
			{
				isRunning = false;
				stopWalking();
				anim.SetFloat("movementSpeed", 1); // change animation back to walking speed
				runCounter = 0;
				speed = walkSpeed;
				StopCoroutine(decideIfChangeDirectionWhenRunning());
			}
		}
	}
	/*
	public override IEnumerator makeStepSound()
	{
		// Loop indefinitely
		
		while (true)
		{
			if (isWalking && !stepAudioSource.isPlaying) // play walking sound
			{
				// get random clip to play
				var rand = new System.Random();
				int randIndex = rand.Next(stepSounds.Length);
				AudioClip randClip = stepSounds[randIndex];
				stepAudioSource.clip = randClip;
				Debug.Log(randClip);
				stepAudioSource.Play();
			}
			yield return new WaitForSeconds(.15f); // Wait
		}
		
	}
	*/
	public override void takeDamage(float damage, float playerXPos)
	{
		if (anim.GetBool("isDead")) return;
		base.takeDamage(damage, playerXPos);
		makeNoise();
		if (health > 0)
		{
			// code to start running:
			if (transform.position.x < playerXPos) // if the animal is on the left side of the player
			{
				direction = direction = new Vector3(-1, 0, 0); // run left
			}
			else direction = new Vector3(1, 0, 0); // run right
			faceDirection();

			isRunning = true;
			isWalking = true; // this bool makes the animal 
			anim.SetFloat("movementSpeed", 1.4f); // make animation faster
			anim.SetBool("isWalking", true);
			StartCoroutine(decideIfChangeDirectionWhenRunning());
			speed = runSpeed;
		}
		else
		{
			isRunning = false; // if dead

			// prob need to make a takeDamage func in concrete class that does the height differently for all animals (or maybe just have a variable?)
			GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x, 0.05f);
		}
	}



	public void run()
	{
		if (!isPathBlocked())
		{
			if (isWalkingOffTheEdge())
			{
				direction = new Vector3(direction.x * -1, 0, 0);
				faceDirection();
			}
			rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

		}
		else // if path is blocked
		{
			direction = new Vector3(direction.x * -1, 0, 0); // change direction
			faceDirection();
		}

		if (isBlockInPath()) jump();
	}

	protected IEnumerator decideIfChangeDirectionWhenRunning()
	{
		while (true)
		{
			yield return new WaitForSeconds(2.5f); // Wait
			float rand = Random.value;
			if (rand < runningChangeDirectionChance)
			{
				direction = new Vector2(direction.x * -1, direction.y).normalized;
				faceDirection();
			}
		}
	}


	// A coroutine that checks the walking condition every 3 seconds
	public override IEnumerator decideIfWalk()
	{
        while (true)
        {
			if (!isRunning)
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
				if(!isRunning) stopWalking();
			}
			else yield return new WaitForSeconds(3f);
		}
	}

	public virtual void initializeAudio()
	{
		gameObject.name = gameObject.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
		/*
		for (int i = 0; i < stepSounds.Length; i++)
		{
			stepSounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\step{i}");
		}
		*/

		for (int i = 0; i < saySounds.Length; i++)
		{
			saySounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\say{i+1}");
		}
	}





}
