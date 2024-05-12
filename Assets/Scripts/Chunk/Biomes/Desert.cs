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
	public override object[] renderLine(float startHeight, float xPos, int xIndex, int chunkPos, Hashtable prevLineOreSpawns, float prevLineHeight, int[] prevVerticalLine)
	{
		List<float[]> frontBackgroundLayerBlocks = new List<float[]>();
		int blockIndex = maxBuildHeight - (int)startHeight; // start blockIndex for the first block

		// if it was spawning a tree, then finish it.
		if (SpawnTreeScript.isSpawningTree(chunkPos >= 0)) frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(blockIndexToYPosition(blockIndex - 1), chunkPos, xPos);
		else frontBackgroundLayerBlocks = SpawnDesertThings.decideIfSpawnDesertThing(blockIndexToYPosition(blockIndex - 1), xPos);

		object[] returnValue = createVerticalLine(14, 14, blockIndex, prevLineOreSpawns, prevLineHeight, prevVerticalLine, xPos); // returns {verticalLine, backgroundVisualBlocks}


		return new object[] { returnValue[0], frontBackgroundLayerBlocks, null, returnValue[1] };
	}

}
