using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public abstract class Biome
{
	public float blockSize = 1;
	public int blocksInChunk = 10; // width of chunk, in blocks
	public int maxBuildHeight = 80;
	private int lowestBlockPos = -60;
	public int maxAmountOfBlocksInLine; // max amount of blocks that can be in a vertical line (from maxBuildHeight to lowesBlockPos)
	public int chunkSize;
	public int highestStartSpawnY = 20;
	public int lowestStartSpawnY = -15;
	public int[] biomeLength = new int[] {3,30}; // min and max biome length, in chunks; (both inclusive)

	public Biome()
	{
		maxAmountOfBlocksInLine = Math.Abs(lowestBlockPos) + maxBuildHeight;
		chunkSize = (int)(blockSize * blocksInChunk);
	}

	public virtual ChunkData renderLeftChunk(int chunkStart)
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine;
		float height = SpawningChunkData.getLeftMostY();
		Hashtable prevSpawnedOresLeft = SpawningChunkData.getPrevSpawnedOresLeft();
		int chunkIndex = chunkSize - 1;
		for (float i = chunkEnd - blockSize; i >= chunkStart; i -= blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer {{[x,y], blockID}, {[x,y], blockID}, ...}, entity }
			object[] returnValue = renderLine(height, i + blockSize / 2, chunkIndex, chunkStart, prevSpawnedOresLeft);
			vLine = (int[])returnValue[0];
			prevSpawnedOresLeft = getOreSpawnsFromVerticalLine(vLine);
			foreach (float[] value in (List<float[]>)returnValue[1]) // for each value in frontBackgroundBlocksToAdd
			{
				frontBackgroundBlocks.Add(value); // add block info to the list
			}
			if (returnValue[2] != null) entities.Add((object[])returnValue[2]); // add entity to list

			addVerticalLineToChunk(chunk, chunkIndex, vLine);

			chunkIndex--;
			height = getNewHeight(height, false);
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresLeft, frontBackgroundBlocks, entities);
		SaveChunk.save(chunkData);

		return chunkData;
	}

	public virtual ChunkData renderRightChunk(int chunkStart) 
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine;
		float height = SpawningChunkData.getRightMostY();
		Hashtable prevSpawnedOresRight = SpawningChunkData.getPrevSpawnedOresRight();
		int chunkIndex = 0;
		for (float i = chunkStart + blockSize / 2; i < chunkEnd; i += blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer {{[x,y], blockID}, {[x,y], blockID}, ...}, entity }
			object[] returnValue = renderLine(height, i, chunkIndex, chunkStart, prevSpawnedOresRight);
			vLine = (int[])returnValue[0];
			prevSpawnedOresRight = getOreSpawnsFromVerticalLine(vLine);
			foreach (float[] value in (List<float[]>)returnValue[1]) // for each value in frontBackgroundBlocksToAdd
			{
				frontBackgroundBlocks.Add(value); // add block info to the list
			}
			if (returnValue[2] != null) entities.Add((object[])returnValue[2]); // add entity to list

			addVerticalLineToChunk(chunk, chunkIndex, vLine);

			chunkIndex++;
			height = getNewHeight(height, true);
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresRight, frontBackgroundBlocks, entities);
		SaveChunk.save(chunkData);

		return chunkData;
	}

	private void addVerticalLineToChunk(int[,] chunk, int chunkIndex, int[] vLine)
	{
		for (int j = 0; j < vLine.Length; j++)
		{
			chunk[chunkIndex, j] = vLine[j];
		}
	}

	/**
     * gets a height for the vertical line that is about to be spawned
     * prevHeight: height of the vertical line that is next to it
     * goingRight: true if spawning a chunk on the right, otherwise false
     */
	public virtual float getNewHeight(float prevHeight, bool goingRight)
	{
		if (goingRight)
		{
			SpawningChunkData.setDontGoDownRight(Math.Max(SpawningChunkData.getDontGoDownRight() - 1, 0));
			SpawningChunkData.setDontGoUpRight(Math.Max(SpawningChunkData.getDontGoUpRight() - 1, 0));
		}
		else
		{
			SpawningChunkData.setDontGoDownLeft(Math.Max(SpawningChunkData.getDontGoDownLeft() - 1, 0));
			SpawningChunkData.setDontGoUpLeft(Math.Max(SpawningChunkData.getDontGoUpLeft() - 1, 0));
		}

		float rand = UnityEngine.Random.value * 100; // Generate a random value between 0 and 100

		int heightDifference = 2; // 1% chance

		if (rand < 79) // 79% chance
		{
			// height difference is 0, return prevHeight
			return prevHeight;
		}
		else if (rand < 99) // 20% chance
		{
			heightDifference = 1;
		}

		rand = UnityEngine.Random.value * 100;

		// 50/50 chance of height change being up or down
		if (rand < 50 && prevHeight - heightDifference > lowestStartSpawnY)
		{

			if (goingRight && SpawningChunkData.getDontGoDownRight() == 0) // right chunk
			{
				SpawningChunkData.setDontGoUpRight(3); // dont go up after going down
				return prevHeight - heightDifference; // go down x blocks
			}
			else if (!goingRight && SpawningChunkData.getDontGoDownLeft() == 0) // left chunk
			{
				SpawningChunkData.setDontGoUpLeft(3);
				return prevHeight - heightDifference; // go down x blocks
			}

		}
		else if (prevHeight + heightDifference < highestStartSpawnY)
		{
			if (goingRight && SpawningChunkData.getDontGoUpRight() == 0) // right chunk
			{
				SpawningChunkData.setDontGoDownRight(3);
				return prevHeight + heightDifference; //  go up x blocks
			}
			else if (!goingRight && SpawningChunkData.getDontGoUpLeft() == 0)// left chunk
			{
				SpawningChunkData.setDontGoDownLeft(3);
				return prevHeight + heightDifference; //  go up x blocks
			}

		}
		return prevHeight;
	}


	private Hashtable getOreSpawnsFromVerticalLine(int[] vLine)
	{
		GameObject block;
		Hashtable oreSpawns = new Hashtable();
		for (int i = 0; i < vLine.Length; i++)
		{
			block = BlockHashtable.getBlockByID(vLine[i]);
			if (block != null && block.gameObject.tag == "Ore")
			{
				oreSpawns[i] = vLine[i];
			}
		}
		return oreSpawns;
	}

	/**
	 * renders a vertical line of blocks within a chunk
	 * startHeight: the height of the highest block in the line
	 * xPos: x position of the vertical line
	 * 
	 * returns: object[] array with:
	 *			int[], an array of the blocks in the vertical line on the Default layer, represented by the blocks ID's
	 *			List<object[]> a list of blocks that go in the frontBackground layer
	 *			the list is of type: {{[x,y], blockID}, {[x,y], blockID}, ...}
	 */
	public abstract object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns);

	// converts a index from a list to a y position in the world where the block should spawn
	public float blockIndexToYPosition(int blockIndex)
	{
		return maxBuildHeight - blockIndex - 0.5f;
	}

}
