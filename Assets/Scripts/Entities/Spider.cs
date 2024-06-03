using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Spider : Mob
{

	private float canJumpToPlayerWithin = 4f; // can do a jump attack on player within this range
	private bool isAttacking = false;
	private Coroutine resetAttackVariableCoroutine = null;
	private bool isGrounded = true;

	private new void Start()
	{
		base.Start();
		saySounds = new AudioClip[4];
		canHurtPlayerWithin = 1f;
		huntPlayerWithinYAxis = 10;
		damage = 5;
	}

	private new void Update()
	{
		// If the entity is walking, move it in the walk direction
		if (isWalking)
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
				stopWalking();
			}

			if (isBlockInPath()) jump();
		}

		if (isHunting) huntPlayer();

		if (isAttacking)
		{
			if (base.canHurtPlayer())
			{
				healthbarScript.takeDamage(damage); // make player take damage
				isAttacking = false;
				if (resetAttackVariableCoroutine != null)
				{
					StopCoroutine(resetAttackVariableCoroutine);
					resetAttackVariableCoroutine = null;
				}
			}
		}
	}


	public override void die()
	{
		base.die();
		//CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
		//collider.size = new Vector2 (0.0979299f, 0.12f);
	}

	protected override void dropLoot()
	{
		dropItem("String");
	}

	private bool canJumpTowardsPlayer()
	{
		float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
		float playerDistanceY = Mathf.Abs(playerPos.position.y - transform.position.y);
		return playerDistanceX <= canJumpToPlayerWithin && playerDistanceY <= 1.5f && !anim.GetBool("isDead");
	}

	protected override void huntPlayer()
	{
		float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
		float playerDistanceY = Mathf.Abs(playerPos.position.y - transform.position.y);
		if (canJumpTowardsPlayer() && isDamageCoroutineRunning == false)
		{
			StartCoroutine(damagePlayer());
		}

		if ((playerDistanceX <= 2 && playerDistanceY <= 2) || playerDistanceX <= 0.2f) // if player is really close, then dont move
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
			if(isGrounded) rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
			anim.SetBool("isWalking", true);
		}
		else // if path is blocked make the spider climb up the wall
		{
			rb.velocity = new Vector2(0, 5);
			anim.SetBool("isWalking", false);
		}

		if (isBlockInPath()) jump();
	}

	/**
	 * override this function to make the spider jump towards the player
	 */
	protected override IEnumerator damagePlayer()
	{
		isDamageCoroutineRunning = true;
		while (true)
		{
			if (!canJumpTowardsPlayer()) break;
			if (healthbarScript.getHealth() > 0)
			{
				//anim.Play("jump"); // play jump animation?
				jumpAttack(); // make the spider jump towards player
				yield return new WaitForSeconds(1.5f);
			}
			else break;
		}
		isDamageCoroutineRunning = false;
	}

	private IEnumerator resetAttackVariable()
	{
		yield return new WaitForSeconds(3f);
		isAttacking = false;
	}

	private void jumpAttack()
	{
		isAttacking = true;
		if (resetAttackVariableCoroutine != null) StopCoroutine(resetAttackVariableCoroutine);
		resetAttackVariableCoroutine = StartCoroutine(resetAttackVariable());

		isGrounded = false;
		bool jumpLeft = playerTransform.position.x < transform.position.x;
		if (jumpLeft) rb.AddForce(new Vector2(-10, 10), ForceMode2D.Impulse);
		else rb.AddForce(new Vector2(10, 10), ForceMode2D.Impulse);
	}

	public override void initializeAudio()
	{
		gameObject.name = gameObject.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name

		for (int i = 0; i < saySounds.Length; i++)
		{
			saySounds[i] = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\say{i + 1}");
		}
		deathSound = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\death");
		hurtSounds = saySounds;
	}

	/**
	 * checks if there is are two blocks in front of the animal, which the animal can't jump over
	 * 
	 * for the spider when the path is "blocked" then the spider can climb the wall
	 */
	public override bool isPathBlocked()
	{
		return Physics2D.OverlapCircle(higherBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap")) && Physics2D.OverlapCircle(lowerBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		isGrounded = true;	
	}


}
