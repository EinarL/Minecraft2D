using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Mob
{

	// Start is called before the first frame update
	void Start()
    {
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

	private void dropLoot()
	{
		//dropItem("RottenFlesh");
	}

	public override void die()
	{
		base.die();
		CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
		collider.size = new Vector2 (0.0979299f, 0.12f);
	}
}
