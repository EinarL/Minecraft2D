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
	private static Hashtable blockNameToPlaceAudioIndex; // what audio plays when a block is placed
	private static Hashtable blockToID = new Hashtable();
	private static Hashtable placeBlockBehaviours = new Hashtable();
	private static HashSet<string> unstackableItems = new HashSet<string>();
	private static Object[] blockObjects;
	private static Tile[] blockTileTextures;
	private static List<AudioClip[]> placeBlockAudio = new List<AudioClip[]>();

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
			{ 28, getBlockTileWithName("SnowyGrassBlock")},
			{ 29, getBlockTileWithName("SnowBlock")},
			{ 30, getBlockTileWithName("LogSpruce")},
			{ 31, getBlockTileWithName("LeavesSpruce")},
			{ 36, getBlockTileWithName("PlankSpruce")},
			{ 37, getBlockTileWithName("BedUpperLeft")},
			{ 38, getBlockTileWithName("BedLowerLeft")},
			{ 39, getBlockTileWithName("BedUpperRight")},
			{ 40, getBlockTileWithName("BedLowerRight")},
			{ 41, getBlockTileWithName("DoorOakTopRight")},
			{ 42, getBlockTileWithName("DoorOakBottomRight")},
			{ 43, getBlockTileWithName("DoorOakTopSideRight")},
			{ 44, getBlockTileWithName("DoorOakBottomSideRight")},
			{ 45, getBlockTileWithName("DoorOakTopLeft")},
			{ 46, getBlockTileWithName("DoorOakBottomLeft")},
			{ 47, getBlockTileWithName("DoorOakTopSideLeft")},
			{ 48, getBlockTileWithName("DoorOakBottomSideLeft")},
			{ 49, getBlockTileWithName("DoorSpruceTopRight")},
			{ 50, getBlockTileWithName("DoorSpruceBottomRight")},
			{ 51, getBlockTileWithName("DoorSpruceTopSideRight")},
			{ 52, getBlockTileWithName("DoorSpruceBottomSideRight")},
			{ 53, getBlockTileWithName("DoorSpruceTopLeft")},
			{ 54, getBlockTileWithName("DoorSpruceBottomLeft")},
			{ 55, getBlockTileWithName("DoorSpruceTopSideLeft")},
			{ 56, getBlockTileWithName("DoorSpruceBottomSideLeft")},
			{ 57, getBlockTileWithName("Gravel")},
			{ 58, getBlockTileWithName("Ladder")},
			{ 59, getBlockTileWithName("LadderLeft")},
			{ 60, getBlockTileWithName("LadderRight")},
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
			{ 28, getBlockWithName("Dirt")}, // 28 is SnowyGrassBlock (we just add a snowy grass block texture to dirt object)
			{ 29, getBlockWithName("SnowBlock")},
			{ 30, getBlockWithName("LogSpruce")},
			{ 31, getBlockWithName("LeavesSpruce")},
			{ 32, getBlockWithName("SaplingSpruce")},
			{ 33, getBlockWithName("MushroomBrown")},
			{ 34, getBlockWithName("MushroomRed")},
			{ 35, getBlockWithName("SnowBlockThin")},
			{ 36, getBlockWithName("PlankSpruce")},
			{ 37, getBlockWithName("BedUpperLeft")},
			{ 38, getBlockWithName("BedLowerLeft")},
			{ 39, getBlockWithName("BedUpperRight")},
			{ 40, getBlockWithName("BedLowerRight")},
			{ 41, getBlockWithName("DoorOakTopRight")},
			{ 42, getBlockWithName("DoorOakBottomRight")},
			{ 43, getBlockWithName("DoorOakTopSideRight")},
			{ 44, getBlockWithName("DoorOakBottomSideRight")},
			{ 45, getBlockWithName("DoorOakTopLeft")},
			{ 46, getBlockWithName("DoorOakBottomLeft")},
			{ 47, getBlockWithName("DoorOakTopSideLeft")},
			{ 48, getBlockWithName("DoorOakBottomSideLeft")},
			{ 49, getBlockWithName("DoorSpruceTopRight")},
			{ 50, getBlockWithName("DoorSpruceBottomRight")},
			{ 51, getBlockWithName("DoorSpruceTopSideRight")},
			{ 52, getBlockWithName("DoorSpruceBottomSideRight")},
			{ 53, getBlockWithName("DoorSpruceTopLeft")},
			{ 54, getBlockWithName("DoorSpruceBottomLeft")},
			{ 55, getBlockWithName("DoorSpruceTopSideLeft")},
			{ 56, getBlockWithName("DoorSpruceBottomSideLeft")},
			{ 57, getBlockWithName("Gravel")},
			{ 58, getBlockWithName("Ladder")},
			{ 59, getBlockWithName("LadderLeft")},
			{ 60, getBlockWithName("LadderRight")},
			{ 61, getBlockWithName("Water")}
		};

		// contains behaviours for blocks that need special functionality upon placing the block, e.g. torches need to be rotated to be placed on right/left wall
		placeBlockBehaviours = new Hashtable()
		{
			{"Torch", new PlaceTorch()},
			{"Ladder", new PlaceLadder()},
			{"BedUpperLeft", new PlaceBed()},
			{"DoorOakTopRight", new PlaceDoor()},
			{"DoorSpruceTopRight", new PlaceDoor()},
		};

		// maps blocks to which index in the placeBlockAudio list is the place block sound for the block
		blockNameToPlaceAudioIndex = new Hashtable() // stone sound is default so it's unnecesary to have those blocks here // use -1 if there is no sound for placing the block
		{
			{"PlankOak", 0}, // 0 is wood sound
			{"PlankSpruce", 0},
			{"LogOak", 0},
			{"LogSpruce", 0},
			{"CraftingTable", 0},
			{"DoorOakTopRight", 0},
			{"DoorSpruceTopRight", 0},
			{"Ladder", 0},
			{"Torch", 0},
			{"Dirt", 1}, // 1 is dirt
			{"Gravel", 1},
			{"SaplingOak", 1}, // 3 is grass
			{"SaplingSpruce", 1},
			{"MushroomBrown", 1},
			{"MushroomRed", 1},
			{"Rose", 1},
			{"Dandelion", 1},
			{"Sand", 4}, // 4 is sand
			{"Cactus", 5}, // 5 is cloth
			{"Wool", 5},
			{"SnowBlock", 5},
			{"BedUpperLeft", 5},
			{"BedLowerLeft", 5},
			{"BedUpperRight", 5},
			{"BedLowerRight", 5},
		};

		unstackableItems = new HashSet<string>() // tools and armors dont have to be in here despite being unstackable
		{
			{"Bucket"},
			{"WaterBucket"}
		};



		foreach (DictionaryEntry entry in blocks) // create the opposite type of hashtable, i.e. block to ID
		{
			if((int)entry.Key != 2) blockToID[((GameObject)entry.Value).name] = entry.Key;
		}
		initializePlaceAudio();
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

	// gets audio when player places a block
	public static AudioClip getBlockPlacingAudio(string name)
	{
		int? placeAudioIndex = (int?)blockNameToPlaceAudioIndex[name]; // int? means that its an integer but possibly null

		// if its null then return stone sound by default
		if (placeAudioIndex == null) return getRandomPlaceAudio(2);
		else if (placeAudioIndex == -1) return null; // if its -1 then we dont want to play any sound
		return getRandomPlaceAudio((int)placeAudioIndex);
	}

	private static AudioClip getRandomPlaceAudio(int index)
	{
		var random = new System.Random();
		int randIndex = random.Next(placeBlockAudio[index].Length);
		AudioClip randClip = placeBlockAudio[index][randIndex];
		return randClip;
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

	public static bool isNotStackable(string itemName)
	{
		return unstackableItems.Contains(itemName);
	}

	// gets the block id that will be behind the mined block
	public static int getBackgroundVisualBlock(string blockName)
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

	private static void initializePlaceAudio()
	{
		string[] allSounds = { "wood", "dirt", "stone", "grass", "sand", "cloth" };

		foreach (string sound in allSounds)
		{
			List<AudioClip> soundList = new List<AudioClip>();
			int index = 1;
			AudioClip placeAudio = Resources.Load<AudioClip>($"Sounds\\Dig\\{sound}{index}");
			while(placeAudio != null)
			{
				soundList.Add(placeAudio);

				index++;
				placeAudio = Resources.Load<AudioClip>($"Sounds\\Dig\\{sound}{index}");
			}
			placeBlockAudio.Add( soundList.ToArray() );
		}
	}
}
