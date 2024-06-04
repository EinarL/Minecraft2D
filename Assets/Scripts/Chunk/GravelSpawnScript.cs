using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GravelSpawnScript
{
	private static int[] minAndMaxGravelHeight = new int[] { 2, 6 }; // how tall the gravel can be 
	private static float gravelSpawnChance = 0.001f;
	private static float continueSpawningGravelChance = 0.6f;

	/**
	 * returns the number where the algorithm should continue spawning blocks - 1
	 */
	public static int decideIfSpawnGravel(int[] verticalLine, int blockIndex)
	{
		if (Random.value < gravelSpawnChance)
		{
			int height = new System.Random().Next(minAndMaxGravelHeight[0], minAndMaxGravelHeight[1] + 1); // height for the gravel
			int spawnBlocksUntil = Mathf.Min(blockIndex + height, verticalLine.Length); // we will spawn gravel until we reach this depth
			for (int i = blockIndex; i < spawnBlocksUntil; i++)
			{
				verticalLine[i] = 57; // gravel
			}
			return spawnBlocksUntil - 1;
		}
		else return blockIndex;
	}

	/**
	 * returns blockIndex + gravelHeight - 1 (this is so that the algorithm that uses this function can know where to continue spawning blocks)
	 */
	public static int decideIfContinueSpawnGravel(int[] verticalLine, int[] prevVerticalLine, int blockIndex)
	{
		if (Random.value < continueSpawningGravelChance)
		{
			int prevHeight = 1; // first lets find the prev line height of the gravel
			int index = blockIndex + 1;
			while (index < prevVerticalLine.Length && prevVerticalLine[index] == 57)
			{
				prevHeight++;
				index++;
			}
			int newHeight = new System.Random().Next(prevHeight - 2, prevHeight + 2 + 1); // new height will be prevHeight +/- 2
			int offset = new System.Random().Next(0, 3);
			int spawnBlocksUntil = Mathf.Min(blockIndex + newHeight + offset, verticalLine.Length); // we will spawn stone/gravel until we reach this depth
			for (int i = blockIndex; i < spawnBlocksUntil; i++)
			{
				if (offset > 0)
				{
					offset--;
					verticalLine[i] = 3; // put stone for the offset
					continue;
				}
				verticalLine[i] = 57; // gravel
			}
			return spawnBlocksUntil - 1;
		}
		else
		{
			verticalLine[blockIndex] = 3;
			return blockIndex;
		}
	}
}
