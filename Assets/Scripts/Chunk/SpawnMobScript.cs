using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnMobScript
{
	private static System.Random random = new System.Random();

	private static float mobSpawnChance = 25f; // x% chance to spawn in each chunk
	private static float mobCaveSpawnChance = 5f; // x% chance to spawn in each vertical line in a chunk (which is 10 per chunk)
	private static string[] mobs = new string[] { "Zombie", "Spider", "Skeleton" };
	private static Dictionary<string, int> mobMaxClusterSize = new Dictionary<string, int>(){ // {mob, how many of them can spawn in a cluster}
		{"Zombie", 3},
		{"Spider", 2},
		{"Skeleton", 2},
	};

	// runs on saved chunks (also new)
	public static List<object[]> decideIfSpawnMob(float chunkLeftPosition, float[] verticalLineHeights)
	{
		List<object[]> mobsToSpawn = new List<object[]>();

		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (rand < mobSpawnChance) {
			string mobName = mobs[random.Next(0, mobs.Length)];
			int mobCount = random.Next(1, mobMaxClusterSize[mobName] + 1);

			for (int _ = 0; _ < mobCount; _++) // add mobs to the list, with random x position
			{
				float xPos = getRandomFloatBetweenTwoValues(chunkLeftPosition, chunkLeftPosition + SpawningChunkData.blocksInChunk);
				float yPos = getYValueBasedOnX(xPos, verticalLineHeights);
				mobsToSpawn.Add(new object[] { xPos, yPos, mobName });
			}
		}
		return mobsToSpawn;
	}


	public static object[] decideIfSpawnMobInCave(float xPos, float yPos)
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (rand < mobCaveSpawnChance)
		{
			return new object[] { xPos, yPos + 0.5f, mobs[random.Next(0, mobs.Length)] }; // spawn random mob
		}
		return null;
	}

	private static float getRandomFloatBetweenTwoValues(float minValue, float maxValue)
	{
		// Generate a random double between 0.0 and 1.0
		double randomDouble = random.NextDouble();

		// Scale and convert to float
		return (float)(minValue + (randomDouble * (maxValue - minValue)));
	}
	// based on an x position in the world, it will find the corresponding index for the place in the verticalLineHeights array and return the height
	private static float getYValueBasedOnX(float x, float[] verticalLineHeights)
	{
		return verticalLineHeights[Mathf.Abs(Mathf.FloorToInt(x)) % SpawningChunkData.blocksInChunk] + 1; // + 1 to spawn the animal above the block
	}

	public static string[] getMobs()
	{
		return mobs;
	}
}
