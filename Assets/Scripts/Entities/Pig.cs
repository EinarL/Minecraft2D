using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pig : Animal
{

	void Start()
	{
		jumpPower = 11f;
		initializeEntity();
		initializeAudio();
		StartCoroutine(decideIfMakeNoise());
	}


	/**
	 * drops loot and destroys the gameobject
	 */
	public override IEnumerator destroyEntity()
	{
		yield return new WaitForSeconds(2f);

		// particle effect?
		dropLoot();
		Destroy(gameObject);
	}

	public override void takeDamage(float damage, float playerXPos)
	{
		if (anim.GetBool("isDead")) return;
		base.takeDamage(damage, playerXPos);
		if (health <= 0)
		{
			makeDeathSound();
			GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x, 0.05f);
		}
	}

	private void dropLoot()
	{
		dropItem("PorkchopRaw");
	}

	private void makeDeathSound()
	{
		AudioClip deathClip = Resources.Load<AudioClip>($"Sounds\\Entities\\{gameObject.name}\\death");
		sayAudioSource.clip = deathClip;
		sayAudioSource.Play();
	}

	/**
	 * checks if there is are two blocks in front of the animal, which the animal can't jump over
	 */
	public override bool isPathBlocked()
	{
		return Physics2D.OverlapCircle(higherBlockCheck.position, 0.05f, LayerMask.GetMask("Default")) && Physics2D.OverlapCircle(lowerBlockCheck.position, 0.05f, LayerMask.GetMask("Default"));
	}
}
