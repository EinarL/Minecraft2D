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
	
	private int huntPlayerWithinRange = 20; // can see player from within this radius, does not apply to y axis, only x
	private int huntPlayerWithinYAxis = 5; // if the enemy is x blocks above/below the player then it wont see the player
	protected float canHurtPlayerWithin = 2; // can damage the player when its within x blocks, applies to x axis
	protected int damage = 6; // how much the mob damages the player when it hits him, 1 heart is 2 hp
	protected bool isDamageCoroutineRunning = false;

	protected Transform playerPos;
	protected HealthbarScript healthbarScript; // script for the health of the player

	// Start is called before the first frame update
	void Start()
	{
		initializeEntity();
		initializeAudio();
		StartCoroutine(decideIfMakeNoise());
		playerPos = GameObject.Find("SteveContainer").transform;
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		StartCoroutine(checkIfHuntPlayer());
	}

	private new void Update()
	{
		base.Update();
		if (isHunting) huntPlayer();
	}

	/**
	 * checks if the player is close, if so then it starts hunting the player
	 */
	private IEnumerator checkIfHuntPlayer()
	{
		while (true)
		{
			yield return new WaitForSeconds(2f); // check if player is in range every x seconds
			if (isPlayerInRange() && !isHunting) startHunting(); // if the mob isnt hunting, then check if it should start hunting
			else if (isHunting && !isPlayerInRange()) stopHunting(); // if the mob is hunting, then check if it should stop
		}
	}

	private void startHunting()
	{
		isHunting = true;
		StopCoroutine(walkingCoroutine); // stop roaming
		isWalking = false;
	}

	private void stopHunting()
	{
		isHunting = false;
		anim.SetBool("isWalking", false);
		walkingCoroutine = StartCoroutine(decideIfWalk()); // start roaming randomly
	}
	/**
	 * returns true if the player is in the range of the mobs visibility
	 */
	private bool isPlayerInRange()
	{
		float distance = Vector2.Distance(playerPos.position, transform.position);

		// if the player is within range of the mob && the difference in the y axis is <= huntPlayerWithinYAxis
		return distance <= huntPlayerWithinRange && Mathf.Abs(playerPos.position.y - transform.position.y) <= huntPlayerWithinYAxis;
	}

	protected virtual bool canHurtPlayer()
	{
		float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
		float playerDistanceY = Mathf.Abs(playerPos.position.y - transform.position.y);
		return playerDistanceX <= canHurtPlayerWithin && playerDistanceY <= 1.5f;
	}

	protected virtual void huntPlayer()
	{
		float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
		if (canHurtPlayer() && isDamageCoroutineRunning == false)
		{
			StartCoroutine(damagePlayer());
		}
		
		if(playerDistanceX <= 1) // if player is really close, then dont move
		{
			facePlayer();
			anim.SetBool("isWalking", false);
			return;
		}
		// if we reach this point then we want to move to the player
		facePlayer(); // turn towards player
		bool isPlayerOnRightSide = playerPos.position.x > transform.position.x;
		if (isPlayerOnRightSide) makeDirectionRight(); // make the direction variable be to the right
		else makeDirectionLeft();

		if (!isPathBlocked())
		{
			rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
			anim.SetBool("isWalking", true);
		}
		else // if path is blocked
		{
			anim.SetBool("isWalking", false);
		}

		if (isBlockInPath()) jump();
	}

	protected void facePlayer()
	{
		bool isPlayerOnRightSide = playerPos.position.x > transform.position.x;
		if (isPlayerOnRightSide && !isFacingRight()) turnRight();
		else if (!isPlayerOnRightSide && isFacingRight()) turnLeft();
	}

	protected virtual IEnumerator damagePlayer()
	{
		isDamageCoroutineRunning = true;
		while (true)
		{
			if (!canHurtPlayer())
			{
				isDamageCoroutineRunning = false;
				yield break;
			}
			if (healthbarScript.getHealth() > 0)
			{
				anim.Play("punch"); // play punch animation
				healthbarScript.takeDamage(damage); // make player take damage
				yield return new WaitForSeconds(1.5f);
			}
			else
			{
				isDamageCoroutineRunning = false;
				yield break;
			}
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
