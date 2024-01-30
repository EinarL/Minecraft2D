using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this script decides where grass and flowers spawn
 */
public static class SpawnGrassScript
{
	private static float grassSpawnChance = 30;
	private static float roseClusterSpawnChance = 3;
	private static float dandelionClusterSpawnChance = 5;
	private static float flowerSpawnChance = 25;
	private static int[] flowerClusterSize = new int[] { 3, 15 }; // max and min size (in blocks) of a flower cluster
	
	private static int roseProcess = 0; // how much is left of spawning the rose cluster (0 is finished)
	private static int dandelionProcess = 0;

	public static float[] decideIfSpawnGrass( float xPos, float yPos)
	{
		float[] frontBackgroundBlock = null;

		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if(roseProcess > 0) // continue spawning rose cluster
		{
			roseProcess--;
			if (rand < flowerSpawnChance) return new float[] { xPos, yPos, 18 }; // spawn rose
		}
		else if(dandelionProcess > 0)
		{
			dandelionProcess--;
			if (rand < flowerSpawnChance) return new float[] { xPos, yPos, 19 }; // spawn dandelion
		}
		if (rand < grassSpawnChance) // spawn grass
		{
			frontBackgroundBlock = new float[] { xPos, yPos, 17 };
		}
        else if(rand < grassSpawnChance + roseClusterSpawnChance && roseProcess <= 0) // begin spawning rose cluster
        {
			System.Random random = new System.Random();
			roseProcess = random.Next(flowerClusterSize[0], flowerClusterSize[1] + 1); // get random size for cluster
			frontBackgroundBlock = new float[] { xPos, yPos, 18 };
		}
		else if (rand < grassSpawnChance + roseClusterSpawnChance + dandelionClusterSpawnChance && dandelionProcess <= 0) // begin spawning dandelion cluster
		{
			System.Random random = new System.Random();
			dandelionProcess = random.Next(flowerClusterSize[0], flowerClusterSize[1] + 1); // get random size for cluster
			frontBackgroundBlock = new float[] { xPos, yPos, 19 };
		}
		return frontBackgroundBlock;
	}
}
