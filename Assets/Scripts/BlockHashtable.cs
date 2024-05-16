using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Collections.AllocatorManager;

/**
 * has a hashtable for every single placeable block in the game
 * 
 * maybe the tools as well, in another hashtable here?
 */
public static class BlockHashtable
{

	private static Hashtable blocks;
	private static Hashtable blockTiles;
	private static Hashtable blockToID = new Hashtable();
	private static Hashtable placeBlockBehaviours = new Hashtable();
	private static Object[] blockObjects;
	private static Tile[] blockTileTextures;

	public static void initializeBlockHashtable()
	{

		blockObjects = Resources.LoadAll("Prefabs\\Blocks");
		blockTileTextures = Resources.LoadAll<Tile>("Palletes\\Tile sprites");

		
		blockTiles = new Hashtable()
		{
			{ 1, getBlockTileWithName("Dirt")},
			{ 2, getBlockTileWithName("GrassBlock")},
            { 3, getBlockTileWithName("Stone")},
			{ 4, getBlockTileWithName("Bedrock")},
			{ 5, getBlockTileWithName("DiamondOre")},
			{ 6, getBlockTileWithName("LogOak")},
			{ 7, getBlockTileWithName("LeavesOak")},
			{ 9, getBlockTileWithName("PlankOak")},
			{ 10, getBlockTileWithName("CraftingTable")},
			{ 11, getBlockTileWithName("Cobblestone")},
			{ 12, getBlockTileWithName("CoalOre")},
			{ 13, getBlockTileWithName("IronOre")},
			{ 14, getBlockTileWithName("Sand")},
			{ 15, getBlockTileWithName("Cactus")},
			{ 18, getBlockTileWithName("Rose")},
			{ 19, getBlockTileWithName("Dandelion")},
			{ 20, getBlockTileWithName("Wool")},
			{ 21, getBlockTileWithName("Furnace")},
			{ 22, getBlockTileWithName("Glass")},
			{ 23, getBlockTileWithName("Tombstone")},
			{ 24, getBlockTileWithName("Torch")},
			{ 25, getBlockTileWithName("TorchWall")},
			{ 26, getBlockTileWithName("TorchLeft")},
			{ 27, getBlockTileWithName("TorchRight")},
		};
		// id's in blockTiles and blocks hashtable need to be the same for each block
		blocks = new Hashtable()
		{
			{ 1, getBlockWithName("Dirt")},
			{ 2, getBlockWithName("Dirt")}, // 2 is GrassBlock (we just add a grass block texture to dirt object)
            { 3, getBlockWithName("Stone")},
			{ 4, getBlockWithName("Bedrock")},
			{ 5, getBlockWithName("DiamondOre")},
			{ 6, getBlockWithName("LogOak")},
			{ 7, getBlockWithName("LeavesOak")},
			{ 8, getBlockWithName("SaplingOak")},
			{ 9, getBlockWithName("PlankOak")},
			{ 10, getBlockWithName("CraftingTable")},
			{ 11, getBlockWithName("Cobblestone")},
			{ 12, getBlockWithName("CoalOre")},
			{ 13, getBlockWithName("IronOre")},
			{ 14, getBlockWithName("Sand")},
			{ 15, getBlockWithName("Cactus")},
			{ 16, getBlockWithName("DeadBush")},
			{ 17, getBlockWithName("Grass")},
			{ 18, getBlockWithName("Rose")},
			{ 19, getBlockWithName("Dandelion")},
			{ 20, getBlockWithName("Wool")},
			{ 21, getBlockWithName("Furnace") },
			{ 22, getBlockWithName("Glass")},
			{ 23, getBlockWithName("Tombstone")},
			{ 24, getBlockWithName("Torch")},
			{ 25, getBlockWithName("TorchWall")},
			{ 26, getBlockWithName("TorchLeft")},
			{ 27, getBlockWithName("TorchRight")},
		};

		// contains behaviours for blocks that need special functionality upon placing the block, e.g. torches need to be rotated to be placed on right/left wall
		placeBlockBehaviours = new Hashtable()
		{
			{"Torch", new PlaceTorch()}
		};

		foreach(DictionaryEntry entry in blocks) // create the opposite type of hashtable, i.e. block to ID
		{
			if((int)entry.Key != 2) blockToID[((GameObject)entry.Value).name] = entry.Key;
		}

	}

	private static GameObject getBlockWithName(string name)
	{
		for (int i = 0; i < blockObjects.Length; i++)
		{
			if (blockObjects[i].name == name) return blockObjects[i] as GameObject;
		}

		Debug.LogError("Did not find block with name: " + name + " in the Blocks folder");
		return null;
	}

	private static Tile getBlockTileWithName(string name)
	{
		for (int i = 0; i < blockTileTextures.Length; i++)
		{
			if (blockTileTextures[i].name == name) return blockTileTextures[i];
		}

		Debug.LogError("Did not find block with name: " + name + " in the Tile sprites folder");
		return null;
	}

	public static PlaceBlockBehaviour getPlaceBlockBehaviour(string blockName)
	{
		return placeBlockBehaviours[blockName] as PlaceBlockBehaviour;
	}

	public static GameObject getBlockByID(int id)
	{
		return blocks[id] as GameObject;
	}

	public static Tile getTileByID(int id)
	{
		return blockTiles[id] as Tile;
	}

	public static int getIDByBlockName(string blockName)
	{
		return (int)blockToID[blockName];
	}

	// gets the block id that will be behind the mined block
	public static int getBackBackgroundBlock(string blockName)
	{
		switch (blockName)
		{
			case "Dirt":
				return 1; // dirt
			case "Sand":
				return 14; // sand 
			default:
				return 3; // stone
		}
	}
}
