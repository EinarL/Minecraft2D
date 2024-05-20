using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CaveSpawnScript
{
	private static int[] caveStartHeights = new int[] { 3,4 }; // min and max height that the cave can start with
	private static int[] caveMaxHeights = new int[] { 2, 10 }; // min and max height that the cave can have
	private static float caveSpawnChance = 0.1f;
	private static int caveSpawnsAboveY = -40; // caves will only be above this y value
	private static int[] caveOffset = new int[] { 0, 4 }; // how differently placed in the y value the next part of the cave will be 
	private static int[] caveHeightDifference = new int[] { -2, 2 }; // how different the height in the cave can be
	private static float stopSpawningChance = 30f; // odds that a cave will stop spawning, the cave height will also need to be small for it to be able to stop spawning


	/**
	 * returns the height of the cave if the cave should spawn, otherwise -1
	 */
	public static int spawnCave(float y)
	{
		if (y < caveSpawnsAboveY) return -1;
		float rand = Random.value * 100;
		if (rand < caveSpawnChance)
		{
			int randomHeight = Random.Range(caveStartHeights[0], caveStartHeights[1] + 1);
			return randomHeight;
		}
		return -1;
	}

	/**
	 * gets the cave height (how tall the cave is), and cave world Y position
	 * 
	 * decides if the cave should continue spawning, if so then it returns the cave offset and height.
	 * the offset is a number between 0 and 4:
	 *	* 0: cave goes up
	 *	* 1: cave goes a little up
	 *	* 2: cave goes straight
	 *	* 3: cave goes a little down
	 *	* 4: cave goes down
	 * 
	 */
	public static int[] continueSpawningCave(int caveHeight, int caveY)
	{
		if(caveHeight <= 2 && caveY < 20) // check if the cave should stop spawning
		{
			float random = Random.value * 100;
			if (random < stopSpawningChance) return new int[] { -1 };
		}
		
		int offset = 2;

		float rand = Random.value * 100;
		if(caveHeight > 2)
		{
			if (rand < 20) // 20% chance that the cave offset might go up or down
			{
				offset = Random.Range(caveOffset[0], caveOffset[1] + 1);
				if (offset > 2 && caveY < caveSpawnsAboveY) offset = 1; // if the cave was going down and the cave is lower than the y value its supposed to spawn in, then go up
			}
		}
		if (caveHeight == 3) 
		{
			if (offset == 0) offset = 1;
			else if (offset == 4) offset = 3;
		}

		int newHeight = caveHeight;

		rand = Random.value * 100;
		if(rand < 30) // chance that the cave will have different height
		{
			newHeight += Random.Range(caveHeightDifference[0], caveHeightDifference[1] + 1); // get new height

			// make sure the cave is not too high or low
			if (newHeight > caveMaxHeights[1]) newHeight = caveMaxHeights[1];
			else if (newHeight < caveMaxHeights[0]) newHeight = caveMaxHeights[0];
		}

		return new int[] { offset, newHeight };	
	}
}
