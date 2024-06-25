using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Ocean : Biome
{

	private spawnChunkScript scScript;

	private int[] minAndMaxDepth = new int[] { 5, 20 };
	private int[] minAndMaxDepthStepSize = new int[] { 1, 10 }; // how deep the ocean can go between two vertical lines
	private int targetDepth;
	private int currentDepth; // how deep the water is, this corresponds to a position in the vertical line array
	private bool firstChunk = true;
	private bool goDeeper = true; // true when the depth should go deeper to reach the targetdepth
	private int oceanHeight; // this tells you where the highest water block is (the ceiling of the ocean), in the position in the array


	public Ocean() : base()
	{
		biomeLength = new int[] { 4, 20 };
		biomeType = "Ocean";
		scScript = GameObject.Find("Main Camera").GetComponent<spawnChunkScript>();
	}


	public override ChunkData renderLeftChunk(int chunkStart)
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<float[]> backgroundVisualBlocks = new List<float[]>();

		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine = SpawningChunkData.prevVerticalLineLeft;
		float height = SpawningChunkData.getLeftMostY();
		Hashtable prevSpawnedOresLeft = SpawningChunkData.getPrevSpawnedOresLeft();
		float[] verticalLineHeights = new float[10];
		int vLineHeightIndex = 0;
		int chunkIndex = chunkSize - 1;
		for (float i = chunkEnd - blockSize; i >= chunkStart; i -= blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer, entities, backgroundVisualBlocks }
			object[] returnValue = renderLine(height, i + blockSize / 2, chunkIndex, chunkStart, prevSpawnedOresLeft, height, vLine);
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
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresLeft, frontBackgroundBlocks, entities, verticalLineHeights, backgroundVisualBlocks, biomeType);
		SpawningChunkData.prevVerticalLineLeft = vLine;
		SaveChunk.save(chunkData);

		return chunkData;
	}

	public override ChunkData renderRightChunk(int chunkStart)
	{
		int[,] chunk = new int[blocksInChunk, maxAmountOfBlocksInLine]; // 2d array representing the blocks with the ID's of the blocks

		List<float[]> frontBackgroundBlocks = new List<float[]>();
		List<float[]> backgroundVisualBlocks = new List<float[]>();
		List<object[]> entities = new List<object[]>();

		float chunkEnd = chunkStart + chunkSize;
		int[] vLine = SpawningChunkData.prevVerticalLineRight;
		float height = SpawningChunkData.getRightMostY();
		float[] verticalLineHeights = new float[10];
		int vLineHeightIndex = 0;
		Hashtable prevSpawnedOresRight = SpawningChunkData.getPrevSpawnedOresRight();
		int chunkIndex = 0;
		for (float i = chunkStart + blockSize / 2; i < chunkEnd; i += blockSize)
		{
			// returns {vLine, blocksToAddToFrontBackgroundLayer, entities, backgroundVisualBlocks }
			object[] returnValue = renderLine(height, i, chunkIndex, chunkStart, prevSpawnedOresRight, height, vLine);
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
		}

		ChunkData chunkData = new ChunkData(chunkStart, chunk, height, prevSpawnedOresRight, frontBackgroundBlocks, entities, verticalLineHeights, backgroundVisualBlocks, biomeType);
		SpawningChunkData.prevVerticalLineRight = vLine;
		SaveChunk.save(chunkData);

		return chunkData;
	}

	/**
	 * renders a vertical line of blocks within a chunk
	 * startHeight: the height of the highest block in the line
	 * xPos: x position of the vertical line
	 * 
	 * returns: int[], an array of the blocks in the vertical line, represented by the blocks ID's
	 */
	public override object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine)
	{
		List<float[]> frontBackgroundLayerBlocks = new List<float[]>();
		int blockIndex = maxBuildHeight - (int)prevLineHeight; // start blockIndex for the first block
		float yPos = blockIndexToYPosition(blockIndex - 1);

		// if it was spawning a tree, then finish it.
		if (SpawnTreeScript.isSpawningTree(chunkPos >= 0)) frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(yPos, chunkPos, xPos);

		List<object[]> entities = new List<object[]>(); // entities in the cave, since this is an ocean biome and we havent implemented e.g. squids

		int[] verticalLine = new int[maxAmountOfBlocksInLine];
		List<float[]> backgroundVisualBlocks = new List<float[]>(); // list of type {[x,y, blockID], ...}, these are the blocks that are in the background of a cave
		int undergroundStartBlockIndex = createOcean(blockIndex, verticalLine, backgroundVisualBlocks, xPos); // put water blocks in the verticalLine array
		object[] returnValue = createUnderground(undergroundStartBlockIndex, verticalLine, backgroundVisualBlocks, prevLineOreSpawns, prevLineHeight, prevVerticalLine, xPos); // returns {verticalLine, backgroundVisualBlocks, entitiesInCave}

		foreach (object[] entity in (List<object[]>)returnValue[2])
		{
			entities.Add(entity);
		}

		return new object[] { returnValue[0], frontBackgroundLayerBlocks, entities, returnValue[1] };
	}



	private int createOcean(int blockIndex, int[] verticalLine, List<float[]> backgroundVisualBlocks, float xPos)
	{
		if (firstChunk)
		{
			oceanHeight = blockIndex;
			targetDepth = oceanHeight + Random.Range(minAndMaxDepth[0], minAndMaxDepth[1] + 1);
			currentDepth = oceanHeight;
			goDeeper = true;
			firstChunk = false;
		}
		// if this is the last ocean chunk
		if (scScript.biomeLength <= 1 && Mathf.Abs(xPos) % 10 > 5)
		{
			if (currentDepth > oceanHeight)
			{
				currentDepth = Mathf.Max(currentDepth - Random.Range(minAndMaxDepthStepSize[0], minAndMaxDepthStepSize[1] - 6), oceanHeight + 1); // go up
				if (Mathf.Abs(xPos) % 10 == 9.5f) // if its the last vertical line in the last chunk
				{
					// reset variables for the next ocean biome
					firstChunk = true;
					if (xPos < 0) SpawningChunkData.setLeftMostY(blockIndexToYPosition(oceanHeight));
					else
					{
						SpawningChunkData.setRightMostY(blockIndexToYPosition(oceanHeight));
						Debug.Log("set right height to: " + blockIndexToYPosition(oceanHeight));
					}
				}
			}
		}
		else if(currentDepth < targetDepth && goDeeper)
		{
			currentDepth = Mathf.Min(currentDepth + Random.Range(minAndMaxDepthStepSize[0], minAndMaxDepthStepSize[1] + 1), targetDepth); // go deeper
			if(currentDepth == targetDepth) goDeeper = false;
		}

		if(currentDepth > oceanHeight+1)currentDepth = getNewOceanDepth(currentDepth); // add some up/down variety to the depth

		for (int i = oceanHeight; i < currentDepth; i++)
		{
			verticalLine[i] = 61; // water
			backgroundVisualBlocks.Add(new float[] { xPos, blockIndexToYPosition(i), 3 }); // background block
		}

		return currentDepth;
	}

	private int getNewOceanDepth(int depth)
	{
		float rand = Random.value; // random value between 0 and 1

		int heightDifference = 2; // 1% chance

		if (rand < 0.79) // 79% chance
		{
			// height difference is 0, return prevHeight
			return depth;
		}
		else if (rand < 0.99) // 20% chance
		{
			heightDifference = 1;
		}

		rand = Random.value;
		// 50/50 change the height difference being up or down
		if (rand < 0.5) return depth - heightDifference; // depth goes up
		else return depth + heightDifference; // go deeper
	}

	/**
	 * 
	 * verticaLine: represents the blocks in the line with the blocks ID's (on the Default layer)
	 * backgroundVisualBlocks: list of type {[x,y, blockID], ...}, these are the blocks that are in the background of a cave,
	 *						   this function only adds to this list
	 */
	private object[] createUnderground(int startBlockIndex, int[] verticalLine, List<float[]> backgroundVisualBlocks, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine, float xPos)
	{
		// if (prevLineHeight > 0) prevLineHeight -= 1; // this is to fix some bug, idk why i need to -1 when its a positive number
		List<object[]> entitiesInCave = new List<object[]>();

		int[] caveSpawnInfo = new int[] { };
		int spawnCaveIn = -1; // if this is a positive number, then we should start spawning a cave in spawnCaveIn blocks.

		// now spawn in e.g. stone, ores, etc.
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

		return new object[] { verticalLine, backgroundVisualBlocks, entitiesInCave };
	}

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
	protected override object[] createVerticalLine(int topBlockID, int secondBlockID, int startBlockIndex, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine, float xPos)
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
			else if (spawnCaveIn != -1)
			{   // spawn cave
				spawnCaveIn = -1;
				CaveSpawnScript.spawnCave(caveSpawnInfo[2], xPos, startBlockIndex, i, topBlockID, secondBlockID, backgroundVisualBlocks);
				i += caveSpawnInfo[2];
				startBlockIndex += caveSpawnInfo[2];
				if (i >= 4)
				{

					break;
				}
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

		return new object[] { verticalLine, backgroundVisualBlocks, entitiesInCave };
	}

}
