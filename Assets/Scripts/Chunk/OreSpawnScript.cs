using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OreSpawnScript
{
	public static readonly HashSet<int> oreIDs = new HashSet<int>() {12, 13, 5 };


	private static int coalSpawnsAboveY = -40; // the y value where coal spawns only above this y value

	private static int ironSpawnsAboveY = -50; // the y value where iron spawns only above this y value

	private static int diamondSpawnsBelowY = -40; // the y value where diamonds will only spawn below this y value

	private static float[] coalIDAndSpawnChance = new float[] { 12, 1.5f }; // [id, spawn chance]
	private static float[] ironIDAndSpawnChance = new float[] { 13, 1.5f };
	private static float[] diamondIDAndSpawnChance = new float[] { 5, .4f };

	private static float clusterChance = 40; // chance that it will spawn the same ore again

	/**
	 * Given the height of the block to be spawned it decides if an ore will be spawned and what ore it is.
	 * 
	 * returns int: blockID of ore to be spawned, 3 (stone ID) if no ore will be spawned
	 */
	public static int spawnOre(float y)
	{
		List<float[]> possibleOreSpawns = new List<float[]>();
		if (y > coalSpawnsAboveY) possibleOreSpawns.Add(coalIDAndSpawnChance);
		if (y > ironSpawnsAboveY) possibleOreSpawns.Add(ironIDAndSpawnChance);
		if (y < diamondSpawnsBelowY) possibleOreSpawns.Add(diamondIDAndSpawnChance);

		for(int i = 0; i < possibleOreSpawns.Count; i++)
		{
			float rand = Random.value * 100;
			if (rand <= possibleOreSpawns[i][1]) return (int)possibleOreSpawns[i][0];
		}

		return 3; // otherwise spawn stone
	}

	/**
	 * this is used for the creation of clusters of ores.
	 * 
	 * returns prevOreID by some chance, otherwise stone (with ID 3)
	 */
	public static int chanceAtSpawningSameOre(int prevOreID)
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (rand < clusterChance) return prevOreID;
		return 3;
	}
}
