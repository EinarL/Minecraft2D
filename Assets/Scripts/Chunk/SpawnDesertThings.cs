using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnDesertThings
{

	private static float spawnCactusChance = 10;
	private static float spawnDeadBushChance = 5;

	public static List<float[]> decideIfSpawnDesertThing(float bottomYPos, float xPos)
	{
		List<float[]> blockPosAndID = new List<float[]>(); // {{x, y, blockID}, {x, y, blockID}, ...}
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		if(rand < spawnCactusChance)
		{
			spawnCactus(blockPosAndID, bottomYPos, xPos);
		}
		else if (rand < spawnCactusChance + spawnDeadBushChance)
		{
			blockPosAndID.Add(new float[] { xPos, bottomYPos, 16 }); // spawn dead bush
		}

		return blockPosAndID;
	}

	private static void spawnCactus(List<float[]> blockPosAndID, float bottomYPos, float xPos)
	{
		System.Random rand = new System.Random();
		int height = rand.Next(2, 4); // get random height for the cactus

		//List<float[]> blockPosAndID = new List<float[]>();
		for (float i = bottomYPos; i < bottomYPos + height; i++)
		{
			blockPosAndID.Add(new float[] { xPos, i, 15 });
		}
	}

	
}
