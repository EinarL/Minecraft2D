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

		int blockIndex = maxBuildHeight - (int)startHeight; // start blockIndex for the first block

		float yPos = blockIndexToYPosition(blockIndex - 1);

		frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(blockIndexToYPosition(blockIndex - 1), chunkPos, xPos); // for spawning trees
		float[] blockToAdd = SpawnGrassScript.decideIfSpawnGrass(xPos, yPos); // for spawning grass and flowers
		if (blockToAdd != null) frontBackgroundLayerBlocks.Add(blockToAdd);

		object[] animalToSpawn = SpawnAnimalScript.decideIfSpawnAnimal(xPos, yPos); // maybe spawn animal TODO: prob remove this

		List<object[]> entities = new List<object[]>();
		if(animalToSpawn != null) entities.Add(animalToSpawn);

		object[] returnValue = createVerticalLine(blockIndex, prevLineOreSpawns, prevLineHeight, prevVerticalLine, xPos); // returns {verticalLine, backgroundVisualBlocks, entitiesInCave}

		foreach (object[] entity in (List<object[]>)returnValue[2])
		{
			entities.Add(entity);
		}

		return new object[] { returnValue[0], frontBackgroundLayerBlocks, entities, returnValue[1] };
	}

}
