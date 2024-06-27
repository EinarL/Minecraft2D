using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creeper : Mob
{
	private float blastRadius = 4f;


	private new void Start()
	{
		saySounds = new AudioClip[4];
		hurtSounds = new AudioClip[0];
		base.Start();
	}

	protected override void huntPlayer()
	{
		float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
		if (canHurtPlayer())
		{
			if (isDamageCoroutineRunning == false) StartCoroutine(damagePlayer());
			facePlayer();
			anim.SetBool("isWalking", false);
			return;
		}
		if (isDamageCoroutineRunning) return;
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

	protected override IEnumerator damagePlayer()
	{
		isDamageCoroutineRunning = true;
		int blowUpCounter = 0;
		while (true)
		{
			float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
			if (playerDistanceX > 3f && blowUpCounter < 2) break;
			if(blowUpCounter >= 3)
			{
				blowUp();
				break;
			}
			//anim.Play("punch"); // play some "going to blow up" animation
			yield return new WaitForSeconds(1f);
			blowUpCounter++;
		}
		isDamageCoroutineRunning = false;
	}

	private void blowUp()
	{
		Debug.Log("blow up!!!!");
	}

	protected override void dropLoot()
	{
		//dropItem("GunPowder");
	}

	protected override void makeHurtNoise()
	{
		var random = new System.Random();
		int randIndex = random.Next(saySounds.Length);
		AudioClip randClip = saySounds[randIndex];
		sayAudioSource.clip = randClip;
		sayAudioSource.Play();
	}

	public override void die()
	{
		base.die();
		CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
		collider.size = new Vector2 (0.0979299f, 0.12f);
	}
}
