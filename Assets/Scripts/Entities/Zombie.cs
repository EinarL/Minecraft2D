using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Mob
{

	protected override void dropLoot()
	{
		dropItem("RottenFlesh");
	}

	public override void die()
	{
		base.die();
		CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
		collider.size = new Vector2 (0.0979299f, 0.12f);
	}
}
