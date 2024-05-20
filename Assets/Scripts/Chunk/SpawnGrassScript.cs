using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

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

	// dictionary that maps blockID to its spawn chance
	private static Dictionary<int, float> spawnChances = new Dictionary<int, float>() {
		{17, 0.25f}, // Grass
		{18, 0.25f}, // Rose
		{19, 0.25f}, // Dandelion
		{33, 0.15f}, // MushroomBrown
		{34, 0.10f}, // MushroomRed
	};

	// maps blockID to how its spawn process (0 is finished) 
	private static Dictionary<int, int> spawningProcess = new Dictionary<int, int>() {
		{18, 0}, // Rose
		{19, 0}, // Dandelion
		{33, 0}, // MushroomBrown
		{34, 0}, // MushroomRed
	};

	// spawns grass and flowers
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

	// spawns a thing from the thingsThatCanSpawn array, or nothing
	public static float[] decideIfSpawnGrass(float xPos, float yPos, int[] thingsThatCanSpawn)
	{
		foreach (int blockID in  thingsThatCanSpawn)
		{
			if (!spawningProcess.ContainsKey(blockID)) continue;

			if (spawningProcess[blockID] > 0) // if we are in the process of spawning a thing from the array
			{
				spawningProcess[blockID]--;
				float rand = Random.value * 100;
				if(rand < 30) {
					return new float[]{ xPos, yPos, blockID }; // spawn the thing that we are in the process of spawning
				}
				else if (rand < 40)
				{
					return new float[] { xPos, yPos, 17 }; // spawn grass
				}
				return null;
			}
		}
		return getRandomThingToSpawn(xPos, yPos, thingsThatCanSpawn);
	}

	// returns a random thing to spawn from the thingsThatCanSpawn array, or it might return null (nothing to spawn)
	private static float[] getRandomThingToSpawn(float xPos, float yPos, int[] thingsThatCanSpawn)
	{
		// Calculate the total weight
		float totalWeight = 0;
		foreach (int blockID in thingsThatCanSpawn)
		{
			totalWeight += spawnChances[blockID];
		}

		// Generate a random number between 0 and the total weight
		System.Random random = new System.Random();
		float randomValue = (float)(random.NextDouble() * Mathf.Max(1, totalWeight));

		// Determine which item to spawn or if no item should spawn
		float cumulativeWeight = 0;
		foreach (int blockID in thingsThatCanSpawn)
		{
			cumulativeWeight += spawnChances[blockID];
			if (randomValue <= cumulativeWeight)
			{
				spawningProcess[blockID] = random.Next(flowerClusterSize[0], flowerClusterSize[1] + 1); // get random size for cluster
				return new float[] { xPos, yPos, blockID };
			}
		}

		return null;
	}

}
