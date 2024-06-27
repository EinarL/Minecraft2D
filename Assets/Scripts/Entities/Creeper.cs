using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creeper : Mob
{
	private float blastRadius = 4f;
	private float blastBaseDamage = 45f; // damage taken when really close to creeper, this diminishes the further you are away from it
	private ParticleSystem explosionParticleSystem;

	private AudioClip fuseSound; // sound when the creeper is going to explode

	private new void Start()
	{
		saySounds = new AudioClip[4];
		hurtSounds = new AudioClip[0];
		explosionParticleSystem = Resources.Load<ParticleSystem>("Particle Systems\\Explosion Particle System");
		fuseSound = Resources.Load<AudioClip>("Sounds\\Random\\fuse");
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
		playFuseSound();
		anim.SetBool("isBlowingUp", true);
		while (true)
		{
			float playerDistanceX = Mathf.Abs(playerPos.position.x - transform.position.x);
			if (playerDistanceX > 3f && blowUpCounter < 2) break;
			if(blowUpCounter >= 2)
			{
				blowUp();
				break;
			}
			yield return new WaitForSeconds(0.7f);
			blowUpCounter++;
		}
		isDamageCoroutineRunning = false;
		anim.SetBool("isBlowingUp", false);
	}

	private void blowUp()
	{
		int explosionSound = Random.Range(1, 5);
		AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>($"Sounds\\Random\\explode{explosionSound}"), transform.position);

		Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
		List<Collider2D> withinRadius = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Player") | LayerMask.GetMask("Entity") | LayerMask.GetMask("Tilemap") | LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("BackBackground"));

		// Check for overlaps
		Physics2D.OverlapCircle(transform.position, blastRadius, filter, withinRadius);
		foreach (Collider2D collider in withinRadius)
		{
			if(collider.gameObject.layer == LayerMask.NameToLayer("Entity") || collider.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				if (ReferenceEquals(collider.gameObject, gameObject)) continue; // Dont let this creeper take damage

				float distance = Vector2.Distance(transform.position, collider.transform.position);

				float divisor = Mathf.Max(1f, distance - 0.5f);
				float damageTaken = blastBaseDamage / divisor;

				if (collider.gameObject.layer == LayerMask.NameToLayer("Entity")) collider.GetComponent<Entity>().takeDamage(damageTaken, transform.position.x);
				else GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>().takeDamage((int)damageTaken);
			}
		}

		Destroy(gameObject);
	}

	protected override void dropLoot()
	{
		//dropItem("GunPowder");
	}

	private void playFuseSound()
	{
		AudioSource.PlayClipAtPoint(fuseSound, transform.position);
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
