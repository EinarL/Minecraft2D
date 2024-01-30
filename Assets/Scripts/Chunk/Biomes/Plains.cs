using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Plains : Biome
{

	public Plains() : base()
	{

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
	public override object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns)
	{
		int[] verticalLine = new int[maxAmountOfBlocksInLine]; // represents the blocks in the line with the blocks ID's // on the Default layer
		List<float[]> frontBackgroundLayerBlocks = new List<float[]>();
		object[] animalToSpawn = null;

		int blockIndex = maxBuildHeight - (int)startHeight; // start blockIndex for the first block

		float yPos = blockIndexToYPosition(blockIndex - 1);

		frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(blockIndexToYPosition(blockIndex - 1), chunkPos, xPos); // for spawning trees
		float[] blockToAdd = SpawnGrassScript.decideIfSpawnGrass(xPos, yPos); // for spawning grass and flowers
		if (blockToAdd != null) frontBackgroundLayerBlocks.Add(blockToAdd);

		animalToSpawn = SpawnAnimalScript.decideIfSpawnAnimal(xPos, yPos); // maybe spawn animal

		int i;
		for (i = 0; i < 4; i++) // first, spawn in four blocks of dirt
		{
			if (i == 0)
			{
				verticalLine[blockIndex] = 2;
			}
			else verticalLine[blockIndex] = 1;
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
		return new object[] { verticalLine, frontBackgroundLayerBlocks, animalToSpawn };
	}

}
