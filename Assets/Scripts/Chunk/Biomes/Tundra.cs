using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Tundra : Biome
{
	private int canSpawnSnowBlockIn = 2; // this counts down until you can spawn in a snow block above the snowy grass block
	private int canStopSpawningSnowBlockIn = 0; // this counts down until you can stop spawning in a snow block
	private int spawningSnowBlockChance = 10; // odds that a snow block will spawn 
	private int stopSpawningSnowBlockChance = 30; // odds of stopping to spawn snow blocks 
	private bool snowBlockOnPrevVerticalLine = false; // was there a snow block on the previous vertical line


	public Tundra() : base()
	{
		biomeType = "Tundra";
		topBlockID = 28;
		secondBlockID = 1;
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

		// TODO: change tree 
		frontBackgroundLayerBlocks = SpawnTreeScript.decideIfSpawnTree(yPos, chunkPos, xPos, "spruce"); // for spawning trees

		// TODO: add snow to frontBackground layer

		object[] animalToSpawn = SpawnAnimalScript.decideIfSpawnAnimal(xPos, yPos); // maybe spawn animal

		List<object[]> entities = new List<object[]>();
		if(animalToSpawn != null) entities.Add(animalToSpawn);

		object[] returnValue = createVerticalLine(blockIndex, prevLineOreSpawns, prevLineHeight, prevVerticalLine, xPos); // returns {verticalLine, backgroundVisualBlocks, entitiesInCave}
		bool didAddSnowBlock = decideIfAddSnowBlock((int[])returnValue[0], blockIndex - 1, yPositionToBlockIndex(prevLineHeight) - 1);

		if(!didAddSnowBlock)
		{
			float[] blockToAdd = SpawnGrassScript.decideIfSpawnGrass(xPos, yPos, new int[] {17,33,34}); // for spawning grass and mushrooms
			if (blockToAdd != null) frontBackgroundLayerBlocks.Add(blockToAdd);
		}

		foreach (object[] entity in (List<object[]>)returnValue[2])
		{
			entities.Add(entity);
		}

		return new object[] { returnValue[0], frontBackgroundLayerBlocks, entities, returnValue[1] };
	}

	/**
	 * decides if it should add a snow block above the snowy grass block.
	 * 
	 * verticalLine: the vertical line we are spawning
	 * height: the index of the snow block in the vertical line if it were to be spawned
	 * 
	 * return true if it added a snow block, otherwise false.
	 */
	private bool decideIfAddSnowBlock(int[] verticalLine, int height, int prevLineHeight)
	{
		if(height != prevLineHeight && !snowBlockOnPrevVerticalLine)
		{
			return false;
		}

		if(canSpawnSnowBlockIn > 0)
		{
			canSpawnSnowBlockIn--;
			return false;
		}
		else if (canStopSpawningSnowBlockIn > 0)
		{
			canStopSpawningSnowBlockIn--;
			verticalLine[height] = 29; // 29 is snow block ID
			return true;
		}
		else if (canStopSpawningSnowBlockIn == 0 && height != prevLineHeight)
		{
			canStopSpawningSnowBlockIn = 3;
			verticalLine[height] = 29;
			return true;
		}
		float rand = Random.value * 100; // Generate a random value between 0 and 100
		if (!snowBlockOnPrevVerticalLine)
		{
			if (rand < spawningSnowBlockChance) // start spawning snow blocks
			{
				verticalLine[height] = 29; // 29 is snow block ID
				snowBlockOnPrevVerticalLine = true;
				canStopSpawningSnowBlockIn = 5;
				return true;
			}
			else return false;
		}
		else // if there was a snow block on the previous line
		{
			if (rand < stopSpawningSnowBlockChance) // stop spawning snow blocks
			{
				canSpawnSnowBlockIn = 5;
				snowBlockOnPrevVerticalLine = false;
				return false;
			}
			else
			{
				verticalLine[height] = 29; // 29 is snow block ID
				return true;
			}
		}
	}

}
