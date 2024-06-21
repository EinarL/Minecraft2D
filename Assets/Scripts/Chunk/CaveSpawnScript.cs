using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CaveSpawnScript
{
	private static int[] caveStartHeights = new int[] { 3,4 }; // min and max height that the cave can start with
	private static int[] caveMaxHeights = new int[] { 2, 10 }; // min and max height that the cave can have
	private static float caveSpawnChance = 0.4f;
	private static int caveSpawnsAboveY = -40; // caves will only be above this y value
	private static int[] caveOffset = new int[] { 0, 4 }; // how differently placed in the y value the next part of the cave will be 
	private static int[] caveHeightDifference = new int[] { -2, 2 }; // how different the height in the cave can be
	private static float stopSpawningChance = 15f; // odds that a cave will stop spawning, the cave height will also need to be small for it to be able to stop spawning


	/**
	 * 
	 * checkAbove: do we check above this position on the previous line to check if we are in the process of spawning a cave,
	 *			   otherwise we just check 2 blocks below to see if we are spawning a cave or not
	 * 
	 * returns int[]{-1 if we shouldn't spawn a cave, caveOffset, caveHeight}
	 */
	public static int[] decideIfSpawnCave(int[] prevVerticalLine, int blockIndex, bool atTop, bool checkAbove = false)
	{
		int checkCaveOffset = atTop ? 0 : 2; // how many blocks below the "current height" will you check if there was a cave in the previous line
		// lets check if we are in the process of spawning in a cave 
		// if prevVerticalLine != null  && not by the end of the list (close to bedrock) && there is a cave on the prev line (we check 2 blocks below bcuz caves can go 2 blocks up) || ...
		if (prevVerticalLine != null && blockIndex + 3 < prevVerticalLine.Length)
		{
			int caveIndex = -1;
			if (checkAbove)
			{
				if (prevVerticalLine[blockIndex - 1] == 0) caveIndex = blockIndex - 1;
				else if (prevVerticalLine[blockIndex] == 0) caveIndex = blockIndex;
			}
			if (caveIndex == -1)
			{
				if(prevVerticalLine[blockIndex + checkCaveOffset] == 0) caveIndex = blockIndex + checkCaveOffset;
			}

			if(caveIndex >= 0)
			{
				int caveHeight = 0;
				while (caveIndex < prevVerticalLine.Length && prevVerticalLine[caveIndex] == 0) // find the height of the cave
				{
					caveHeight++;
					caveIndex++;
				}
				if (caveHeight <= 1) return new int[] { -1 };
				int[] caveOffsetAndHeight = continueSpawningCave(caveHeight, (int)blockIndexToYPosition(blockIndex + 2));
				if (caveOffsetAndHeight[0] == -1) return caveOffsetAndHeight;
				if (atTop && caveOffsetAndHeight[0] > 1)
				{
					caveOffsetAndHeight[0] = 0; // if were at the top then reduce offset
				}
				return new int[] { 1, caveOffsetAndHeight[0], caveOffsetAndHeight[1] };
			}
		}

		// otherwise we check if we should start spawning a cave
		int caveH = spawnNewCave(blockIndexToYPosition(blockIndex)); // returns -1 if we shouldn't spawn in a cave
		return new int[] { caveH, 0, caveH };
	}

	/**
	 * spawns the cave
	 * 
	 * caveHeight: height of the cave.
	 * xPos: the x position in the world of the vertical line being rendered.
	 * distanceFromTopBlock: the distance the current block is from the topmost block in the vertical line, 
	 *						 e.g. 0 if this is the grass block, 1 if this is the dirt block below the grass block, etc,
	 * topBlockID: the id of the block that is at the top (usually a grass block)
	 * secondBlockID: the id of the block that is below the top block (usually dirt)
	 * backgroundVisualBlocks: list of the background visual blocks that will show the walls of the cave
	 * 
	 */
	public static void spawnCave(int caveHeight, float xPos, int index, int distanceFromTopBlock, int topBlockID, int secondBlockID, List<float[]> backgroundVisualBlocks)
	{
		for (int _ = 0; _ < caveHeight; _++)
		{
			if (distanceFromTopBlock == 0) backgroundVisualBlocks.Add(new float[] { xPos, blockIndexToYPosition(index), topBlockID }); // e.g. grass block/sand
			else if (distanceFromTopBlock < 4) backgroundVisualBlocks.Add(new float[] { xPos, blockIndexToYPosition(index), secondBlockID }); // dirt/sand
			else backgroundVisualBlocks.Add(new float[] { xPos, blockIndexToYPosition(index), 3 }); // stone
			distanceFromTopBlock++;
			index++;
		}
	}

	// this is the same function as above, except it always puts stone as the background wall in the cave
	public static void spawnCave(int caveHeight, float xPos, int index, List<float[]> backgroundVisualBlocks)
	{
		for (int _ = 0; _ < caveHeight; _++)
		{
			backgroundVisualBlocks.Add(new float[] { xPos, blockIndexToYPosition(index), 3 }); // stone
			index++;
		}
	}


	/**
	 * returns the height of the cave if the cave should spawn, otherwise -1
	 */
	public static int spawnNewCave(float y)
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
				if (offset == 0 && Random.value < 0.8f) offset = 1; // we want the cave to go 2 blocks up/down rarely, so we will cancel it like 80% of the time
				if (offset == 4 && Random.value < 0.8f) offset = 3;
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


	private static float blockIndexToYPosition(int blockIndex)
	{
		return SpawningChunkData.maxBuildHeight - blockIndex - 0.5f;
	}
}
