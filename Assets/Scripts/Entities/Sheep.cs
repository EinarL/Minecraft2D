using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : Animal
{
	private float eatChance = .2f;

	new void Start()
	{
		base.Start();
		initializeEntity();
		initializeAudio();
		StartCoroutine(decideIfEat());
		StartCoroutine(decideIfMakeNoise());
	}


	// A coroutine that checks checks if the sheep should eat every 2 seconds
	private IEnumerator decideIfEat()
	{
		// Loop indefinitely
		while (true)
		{
			float rand = Random.value; // random number between 0 and 1
			if (!isWalking && !isRunning && rand < eatChance) anim.SetBool("isEating", true); 
			yield return new WaitForSeconds(2f); // Wait for 2 seconds before checking again
			anim.SetBool("isEating", false);
		}
	}

	protected override void dropLoot()
	{
		dropItem("Wool");
		dropItem("MuttonRaw");
	}
}
