using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnAnimalScript // Animal
{
	private static System.Random random = new System.Random();

	private static float animalClusterChance = 3f;
	private static float animalSpawnChance = 5f;
	private static int[] animalClusterSize = new int[] { 5, 10}; // will spawn animal cluster in a range from x1 to x2 blocks
	private static int spawningProcess = 0; // how much is left of spawning the animal cluster (0 is finished)
	private static string spawningAnimal = null; // what animal we are spawning in the cluster
	private static string[] animals = new string[] { "Sheep", "Pig" };

	//  this algorithm is used for when we are spawning in a new chunk 
	public static object[] decideIfSpawnAnimal(float xPos, float yPos)
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (spawningProcess > 0) // if we are in the process of spawning animals
		{
			spawningProcess--;
			if (rand < animalSpawnChance) return new object[] { xPos, yPos, spawningAnimal};
		}
		else if (rand < animalClusterChance)
		{
			spawningProcess = random.Next(animalClusterSize[0], animalClusterSize[1] + 1); // get random size for cluster
			spawningAnimal = animals[random.Next(0, animals.Length)]; // get random animal	
			if (spawningAnimal == null) Debug.LogError("spawningAnimal is null");
			return new object[] { xPos, yPos, spawningAnimal };
		}

		return null;
	}



	private static float spawningOnSavedChunkChance = 3f; // odds that animals spawn on a previously saved chunk (the chunk must have no animals in it to begin with)
	private static int[] savedChunkAnimalClusterSize = new int[] { 1, 4 }; // min and max amount of animals that can spawn on a saved chunk


	/**
	 * this algorithm is used when we are rendering a previously saved chunk
	 * 
	 * chunkLeftPosition: the x coordinate of the chunk (left side of the chunk)
	 * verticalLineHeights: the y coordinates of the top blocks in the chunk, beware that if its a right chunk then the
	 *						leftmost vertical line is at index 0 but if its a left chunk then its at index 9.
	 */
	public static List<object[]> decideIfSpawnAnimalsOnSavedChunk(float chunkLeftPosition, float[] verticalLineHeights) {
		List<object[]> animalsToSpawn = new List<object[]>();

		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (rand < spawningOnSavedChunkChance)
		{
			int animalCount = random.Next(savedChunkAnimalClusterSize[0], savedChunkAnimalClusterSize[1] + 1); // get random size for cluster
			string animalName = animals[random.Next(0, animals.Length)]; // get random animal	

			for (int _ = 0; _ < animalCount; _++) // add animals to the list, with random x position
			{
				float xPos = getRandomFloatBetweenTwoValues(chunkLeftPosition, chunkLeftPosition + SpawningChunkData.blocksInChunk);
				float yPos = getYValueBasedOnX(xPos, verticalLineHeights);
				animalsToSpawn.Add(new object[] { xPos, yPos, animalName });
			}
			
		}

		return animalsToSpawn;
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
		return verticalLineHeights[Mathf.Abs(Mathf.FloorToInt(x)) % SpawningChunkData.blocksInChunk] + 2; // + 2 to spawn the animal above the block
	}
}
