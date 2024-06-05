using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : Animal
{

	protected AudioClip[] hurtSounds = new AudioClip[2];

	void Start()
	{
		health = 7;
		jumpPower = 4.5f;
		initializeEntity();
		initializeAudio();
		StartCoroutine(decideIfMakeNoise());
	}

	public override void takeDamage(float damage, float playerXPos)
	{
		if (anim.GetBool("isDead")) return;
		base.takeDamage(damage, playerXPos);
		makeHurtNoise();
		if (health <= 0)
		{
			GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x, 0.05f);
		}
	}

	protected void makeHurtNoise()
	{
		var random = new System.Random();
		int randIndex = random.Next(hurtSounds.Length);
		AudioClip randClip = hurtSounds[randIndex];
		sayAudioSource.clip = randClip;
		sayAudioSource.Play();
	}

	public override void die()
	{
		base.die();
		GetComponent<Rigidbody2D>().gravityScale = 5f;
	}

	protected override void dropLoot()
	{
		dropItem("Feather");
		dropItem("ChickenRaw");
	}

	public override void run()
	{
		if (!isPathBlocked())
		{
			if (isWalkingOffTheEdge())
			{
				direction = new Vector3(direction.x * -1, 0, 0);
				faceDirection();
			}
			if (!isBlockInPath()) rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
			else rb.velocity = new Vector2(0, rb.velocity.y);

		}
		else // if path is blocked
		{
			direction = new Vector3(direction.x * -1, 0, 0); // change direction
			faceDirection();
		}

		if (isBlockInPath()) jump();
	}


	/**
	 * checks if there is are two blocks in front of the animal, which the animal can't jump over
	 */
	public override bool isPathBlocked()
	{
		return Physics2D.OverlapCircle(higherBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap")) && Physics2D.OverlapCircle(lowerBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
	}

	public override void initializeAudio()
	{
		base.initializeAudio();
		for (int i = 0; i < hurtSounds.Length; i++)
		{
			hurtSounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\Chicken\\hurt{i + 1}");
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		anim.SetBool("isGliding", false);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		anim.SetBool("isGliding", true);
	}
}
