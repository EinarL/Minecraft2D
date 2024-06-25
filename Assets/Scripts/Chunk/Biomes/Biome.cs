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
	public int[] biomeLength = new int[] { 5, 30 }; // min and max biome length, in chunks; (both inclusive)
	protected string biomeType = "Plains";
	public int topBlockID { get; protected set; } = 2;
	public int secondBlockID { get; protected set; } = 1;

	public Biome()
	{
		maxAmountOfBlocksInLine = Math.Abs(lowestBlockPos) + maxBuildHeight;
		chunkSize = (int)(blockSize * blocksInChunk);
	}

	public virtual ChunkData renderLeftChunk(int chunkStart)
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<float[]> backgroundVisualBlocks = new List<float[]>();

		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine = SpawningChunkData.prevVerticalLineLeft;
		float height = SpawningChunkData.getLeftMostY();
		float prevLineHeight = height;
		Hashtable prevSpawnedOresLeft = SpawningChunkData.getPrevSpawnedOresLeft();
		float[] verticalLineHeights = new float[10];
		int vLineHeightIndex = 0;
		int chunkIndex = chunkSize - 1;
		for (float i = chunkEnd - blockSize; i >= chunkStart; i -= blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer, entities, backgroundVisualBlocks }
			object[] returnValue = renderLine(height, i + blockSize / 2, chunkIndex, chunkStart, prevSpawnedOresLeft, prevLineHeight, vLine);
			vLine = (int[])returnValue[0];
			prevSpawnedOresLeft = getOreSpawnsFromVerticalLine(vLine);
			foreach (float[] value in (List<float[]>)returnValue[1]) // for each value in frontBackgroundBlocksToAdd
			{
				frontBackgroundBlocks.Add(value); // add block info to the list
			}
			foreach (float[] value in (List<float[]>)returnValue[3]) // for each value in backgroundVisualBlocks
			{
				backgroundVisualBlocks.Add(value); // add block info to the list
			}
			addEntitesToList(entities, (List<object[]>)returnValue[2]); // add entities to list

			addVerticalLineToChunk(chunk, chunkIndex, vLine);

			chunkIndex--;
			verticalLineHeights[vLineHeightIndex] = height < 0 ? height : height - 1; // idk why i need to do height-1 when its a positive number but it fixes a bug
			vLineHeightIndex++;
			prevLineHeight = height;
			height = getNewHeight(height, false);
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresLeft, frontBackgroundBlocks, entities, verticalLineHeights, backgroundVisualBlocks, biomeType);
		SpawningChunkData.prevVerticalLineLeft = vLine;
		SaveChunk.save(chunkData);

		return chunkData;
	}

	public virtual ChunkData renderRightChunk(int chunkStart) 
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<float[]> backgroundVisualBlocks = new List<float[]>();
		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine = SpawningChunkData.prevVerticalLineRight;
		float height = SpawningChunkData.getRightMostY();
		float prevLineHeight = height;
		float[] verticalLineHeights = new float[10];
		int vLineHeightIndex = 0;
		Hashtable prevSpawnedOresRight = SpawningChunkData.getPrevSpawnedOresRight();
		int chunkIndex = 0;
		for (float i = chunkStart + blockSize / 2; i < chunkEnd; i += blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer, entities, backgroundVisualBlocks }
			object[] returnValue = renderLine(height, i, chunkIndex, chunkStart, prevSpawnedOresRight, prevLineHeight, vLine);
			vLine = (int[])returnValue[0];
			prevSpawnedOresRight = getOreSpawnsFromVerticalLine(vLine);
			foreach (float[] value in (List<float[]>)returnValue[1]) // for each value in frontBackgroundBlocksToAdd
			{
				frontBackgroundBlocks.Add(value); // add block info to the list
			}
			foreach (float[] value in (List<float[]>)returnValue[3]) // for each value in backgroundVisualBlocks
			{
				backgroundVisualBlocks.Add(value); // add block info to the list
			}
			addEntitesToList(entities, (List<object[]>)returnValue[2]); // add entities to list

			addVerticalLineToChunk(chunk, chunkIndex, vLine);

			chunkIndex++;
			verticalLineHeights[vLineHeightIndex] = height < 0 ? height : height - 1; // idk why i need to do height-1 when its a positive number but it fixes a bug
			vLineHeightIndex++;
			prevLineHeight = height;
			height = getNewHeight(height, true);
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresRight, frontBackgroundBlocks, entities, verticalLineHeights, backgroundVisualBlocks, biomeType);
		SpawningChunkData.prevVerticalLineRight = vLine;
		SaveChunk.save(chunkData);

		return chunkData;
	}

	protected void addVerticalLineToChunk(int[,] chunk, int chunkIndex, int[] vLine)
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


	protected Hashtable getOreSpawnsFromVerticalLine(int[] vLine)
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
	public abstract object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine);

	/**
	 * gets in the ID of the top block and the ID of the next 3 blocks, i.e. for plains biome it would be grass block and dirt block.
	 * 
	 * int topBlockID - id of the top block, i.e. grass block ID
	 * int secondBlockID - id of the next 3 blocks, i.e. dirt block ID
	 * int startBlockIndex - this will define how high the vertical line in the chunk will be
	 * Hashtable prevLineOreSpawns - the key is the height of the block and the value is the ID of the ore if there is an ore in this position, otherwise null
	 *								 this is to know if there should spawn more of the same ore in this vertical line or not.
	 * float prevLineHeight - height of the previous vertical line, this is used to help the spawning of caves
	 * 
	 * returns a list of block ID's where each id represents a block in that vertical line, also returns background visual blocks for the background in the cave
	 * 
	 */
	protected virtual object[] createVerticalLine(int startBlockIndex, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine, float xPos)
	{
		if (prevLineHeight > 0) prevLineHeight -= 1; // this is to fix some bug, idk why i need to -1 when its a positive number
		List<float[]> backgroundVisualBlocks = new List<float[]>(); // list of type {[x,y, blockID], ...}, these are the blocks that are in the background of a cave
		List<object[]> entitiesInCave = new List<object[]>();

		int[] verticalLine = new int[maxAmountOfBlocksInLine]; // represents the blocks in the line with the blocks ID's // on the Default layer

		int[] caveSpawnInfo = new int[] { };
		int spawnCaveIn = -1; // if this is a positive number, then we should start spawning a cave in spawnCaveIn blocks.

		int i;
		for (i = 0; i < 4; i++) // first, spawn in four blocks of dirt/sand
		{
			if (spawnCaveIn == -1) // if were not currently spawning a cave
			{
				caveSpawnInfo = CaveSpawnScript.decideIfSpawnCave(prevVerticalLine, startBlockIndex, i == 0); // returns int[]{-1 if we shouldn't spawn a cave, caveOffset, caveHeight}
				if (caveSpawnInfo[0] != -1) spawnCaveIn = caveSpawnInfo[1];
			}
			if (spawnCaveIn > 0) spawnCaveIn--;
			else if(spawnCaveIn != -1)
			{	// spawn cave
				spawnCaveIn = -1;
				CaveSpawnScript.spawnCave(caveSpawnInfo[2], xPos, startBlockIndex, i, topBlockID, secondBlockID, backgroundVisualBlocks);
				i += caveSpawnInfo[2];
				startBlockIndex += caveSpawnInfo[2];
				if (i >= 4) break;
			}

			if (i == 0)
			{
				verticalLine[startBlockIndex] = topBlockID;
			}
			else verticalLine[startBlockIndex] = secondBlockID;
			startBlockIndex++;
		}

		// now spawn in the rest, i.e. stone, ores and etc.
		while (startBlockIndex < verticalLine.Length - 1) // place stone, ores, etc. up until the last block
		{
			if (spawnCaveIn == -1)
			{
				caveSpawnInfo = CaveSpawnScript.decideIfSpawnCave(prevVerticalLine, startBlockIndex, false); // returns int[]{-1 if we shouldn't spawn a cave, caveOffset, caveHeight}
				if (caveSpawnInfo[0] != -1) spawnCaveIn = caveSpawnInfo[1];
			}
			if (spawnCaveIn > 0) spawnCaveIn--;
			else if (spawnCaveIn != -1) // spawn cave
			{
				spawnCaveIn = -1;
				CaveSpawnScript.spawnCave(caveSpawnInfo[2], xPos, startBlockIndex, backgroundVisualBlocks);
				startBlockIndex += caveSpawnInfo[2]; 
				AddToListIfNotNull(entitiesInCave, SpawnMobScript.decideIfSpawnMobInCave(xPos, blockIndexToYPosition(startBlockIndex))); // maybe spawn mob in cave
				if (startBlockIndex >= verticalLine.Length - 1) break;
			}

			int aboveBlockID = verticalLine[startBlockIndex - 1];

			// if the block next to this one is an ore, then maybe spawn that same ore again
			if (prevLineOreSpawns[startBlockIndex] != null) verticalLine[startBlockIndex] = OreSpawnScript.chanceAtSpawningSameOre((int)prevLineOreSpawns[startBlockIndex]);
			else if (OreSpawnScript.oreIDs.Contains(aboveBlockID)) verticalLine[startBlockIndex] = OreSpawnScript.chanceAtSpawningSameOre(verticalLine[startBlockIndex - 1]); // if the above block is an ore
			else if (prevVerticalLine != null && prevVerticalLine[startBlockIndex] == 57) startBlockIndex = GravelSpawnScript.decideIfContinueSpawnGravel(verticalLine, prevVerticalLine, startBlockIndex);
			else verticalLine[startBlockIndex] = OreSpawnScript.spawnOre(blockIndexToYPosition(startBlockIndex)); // maybe spawn ores, otherwise stone

			if (verticalLine[startBlockIndex] == 3) startBlockIndex = GravelSpawnScript.decideIfSpawnGravel(verticalLine, startBlockIndex); // maybe spawn gravel

			startBlockIndex++;
		}

		verticalLine[139] = 4; // bedrock is last block

		return new object[] { verticalLine, backgroundVisualBlocks, entitiesInCave};
	}

	// converts a index from a list to a y position in the world where the block should spawn
	public float blockIndexToYPosition(int blockIndex)
	{
		return maxBuildHeight - blockIndex - 0.5f;
	}

	protected int yPositionToBlockIndex(float yPos)
	{
		return (int)(maxBuildHeight - yPos - 0.5f);
	}

	protected void addEntitesToList(List<object[]> entities, List<object[]> entitiesToAdd)
	{
		foreach (object[] entity in entitiesToAdd)
		{
			entities.Add(entity);
		}
	}

	protected void AddToListIfNotNull(List<object[]> list, object[] toAdd)
	{
		if(toAdd != null) list.Add(toAdd);
	}

}
