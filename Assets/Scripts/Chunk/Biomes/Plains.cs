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
	public override object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine)
	{
		List<float[]> frontBackgroundLayerBlocks = new List<float[]>();
		object[] animalToSpawn = null;

		int blockIndex = maxBuildHeight - (int)startHeight; // start blockIndex for the first block

		float yPos = blockIndexToYPosition(blockIndex - 1);

		frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(blockIndexToYPosition(blockIndex - 1), chunkPos, xPos); // for spawning trees
		float[] blockToAdd = SpawnGrassScript.decideIfSpawnGrass(xPos, yPos); // for spawning grass and flowers
		if (blockToAdd != null) frontBackgroundLayerBlocks.Add(blockToAdd);

		animalToSpawn = SpawnAnimalScript.decideIfSpawnAnimal(xPos, yPos); // maybe spawn animal

		object[] returnValue = createVerticalLine(2, 1, blockIndex, prevLineOreSpawns, prevLineHeight, prevVerticalLine, xPos); // returns {verticalLine, backgroundVisualBlocks}

		return new object[] { returnValue[0], frontBackgroundLayerBlocks, animalToSpawn, returnValue[1] };
	}

}
