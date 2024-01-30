using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnAnimalScript // Animal
{
	private static System.Random random = new System.Random();

	private static float animalClusterChance = 3;
	private static float animalSpawnChance = 10; // lower this? maybe not
	private static int[] animalClusterSize = new int[] { 5, 20};
	private static int spawningProcess = 0; // how much is left of spawning the animal cluster (0 is finished)
	private static string spawningAnimal = null; // what animal we are spawning in the cluster
	private static string[] animals = new string[] { "Sheep", "Pig" };

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
}
