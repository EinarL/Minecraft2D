using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Desert : Biome
{
	public Desert() : base()
	{
		biomeLength = new int[] { 5, 20 };
	}

	/**
	 * renders a vertical line of blocks within a chunk
	 * startHeight: the height of the highest block in the line
	 * xPos: x position of the vertical line
	 * 
	 * returns: int[], an array of the blocks in the vertical line, represented by the blocks ID's
	 */
	public override object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns)
	{
		List<float[]> frontBackgroundLayerBlocks = new List<float[]>();
		int[] verticalLine = new int[maxAmountOfBlocksInLine]; // represents the blocks in the line with the blocks ID's
		int blockIndex = maxBuildHeight - (int)startHeight; // start blockIndex for the first block

		// if it was spawning a tree, then finish it.
		if (SpawnTreeScript.isSpawningTree(chunkPos >= 0)) frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(blockIndexToYPosition(blockIndex - 1), chunkPos, xPos);
		else frontBackgroundLayerBlocks = SpawnDesertThings.decideIfSpawnDesertThing(blockIndexToYPosition(blockIndex - 1), xPos);

		int i;
		for (i = 0; i < 4; i++) // first, spawn in four blocks of sand
		{
			verticalLine[blockIndex] = 14;
			blockIndex++;
		}

		// now spawn in the rest, i.e. stone, ores and etc.
		while (blockIndex < verticalLine.Length - 1) // place stone, ores, etc. up until the last block
		{
			GameObject aboveGameObject = BlockHashtable.getBlockByID(verticalLine[blockIndex - 1]);

			// if the block next to this one is an ore, then maybe spawn that same ore again
			if (prevLineOreSpawns[blockIndex] != null) verticalLine[blockIndex] = OreSpawnScript.chanceAtSpawningSameOre((int)prevLineOreSpawns[blockIndex]);
			else if (aboveGameObject.gameObject.tag == "Ore") verticalLine[blockIndex] = OreSpawnScript.chanceAtSpawningSameOre(verticalLine[blockIndex - 1]); // if the above block is an ore
			else verticalLine[blockIndex] = OreSpawnScript.spawnOre(blockIndexToYPosition(blockIndex));

			blockIndex++;
		}
		verticalLine[blockIndex] = 4; // bedrock is last block
		return new object[] { verticalLine, frontBackgroundLayerBlocks, null };
	}

}
