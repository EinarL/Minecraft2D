using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockBehaviourData
{

	private static Hashtable digBehaviours = new Hashtable() { // hashtable that holds the class for the different block sounds and speed when breaking them
		{ "Dirt", new BreakDirt() },
		{ "LogOak", new BreakWood() },
		{ "LogSpruce", new BreakWood() },
		{ "LeavesOak", new BreakLeaves() },
		{ "LeavesSpruce", new BreakLeaves() },
		{ "SaplingOak", new BreakInstantly() },
		{ "SaplingSpruce", new BreakInstantly() },
		{ "PlankOak", new BreakWood() },
		{ "CraftingTable", new BreakWood() },
		{ "Stone", new BreakStone() },
		{ "Cobblestone", new BreakStone() },
		{ "CoalOre", new BreakStone() },
		{ "IronOre", new BreakStone() },
		{ "DiamondOre", new BreakStone() },
		{ "Sand", new BreakSand() },
		{ "Cactus", new BreakCloth() },
		{ "DeadBush", new BreakInstantly() },
		{ "Grass", new BreakInstantly() },
		{ "Rose", new BreakInstantly() },
		{ "Dandelion", new BreakInstantly() },
		{ "MushroomBrown", new BreakInstantly() },
		{ "MushroomRed", new BreakInstantly() },
		{ "Wool", new BreakCloth() },
		{ "Furnace", new BreakStone() },
		{ "Glass", new BreakGlass() },
		{ "Bedrock", new BreakBedrock() },
		{ "Tombstone", new BreakTombstone() },
		{ "Torch", new BreakWoodInstantly() },
		{ "TorchWall", new BreakWoodInstantly() },
		{ "TorchRight", new BreakWoodInstantly() },
		{ "TorchLeft", new BreakWoodInstantly() },
		{ "SnowBlock", new BreakSnow() },
		{ "SnowBlockThin", new BreakSnow() },
	};

	private static Hashtable itemDropBehaviours = new Hashtable()
	{
		{ "LeavesOak", new LeafItemDrop("Oak") },
		{ "LeavesSpruce", new LeafItemDrop("Spruce") },
		{ "Stone", new StoneItemDrop() },
		{ "IronOre", new StonePickOrBetter() },
		{ "DiamondOre", new DiamondItemDrop()},
		{ "DeadBush", new DeadBushItemDrop() },
		{ "Grass", new DropNothing() },
		{ "Furnace", new PickaxeDrop() },
		{ "CoalOre", new PickaxeDrop() },
		{ "Glass", new DropNothing() },
		{ "Tombstone", "Tombstone" },
		{ "TorchWall", new TorchDrop() },
		{ "TorchRight", new TorchDrop() },
		{ "TorchLeft", new TorchDrop() },
		{ "SnowBlock", new SnowDrop(3,5)},
		{ "SnowBlockThin", new SnowDrop(1,1)},
	};

	private static Hashtable rightClickBehaviours = new Hashtable()
	{
		{ "CraftingTable", new OpenCraftingTableBehaviour() },
		{ "Furnace", new OpenFurnaceBehaviour()}
	};

	// hashtable that maps block names to the corresponding step sound folder, default is dirt sound so its unneccesary to have those blocks here
	private static Hashtable blockToStepSoundFolder = new Hashtable()
	{
		{ "Stone", new object[]{ "stone", 6 } }, // value is: {folder name, amount of sound files in the folder }
		{ "Cobblestone", new object[]{ "stone", 6 } },
		{ "CoalOre", new object[]{ "stone", 6 } },
		{ "IronOre", new object[]{ "stone", 6 } },
		{ "DiamondOre", new object[]{ "stone", 6 } },
		{ "LogOak", new object[]{ "wood", 6 }},
		{ "PlankOak", new object[]{ "wood", 6 }},
		{ "CraftingTable", new object[] { "wood", 6 } },
		{ "Sand", new object[]{ "sand", 5} },
		{ "Wool", new object[] { "cloth", 4} },
		{ "Furnace", new object[]{ "stone", 6 } },
		{ "Glass", new object[]{ "stone", 6 } },
		{ "Bedrock", new object[]{ "stone", 6 } },
		{ "SnowBlock", new object[] { "cloth", 4} },
		{ "SnowyGrassBlock", new object[] { "cloth", 4} },

	};
	private static object[] prevStepSound = new object[] { "dirt", 4 };


	private static Hashtable stepSoundPitches = new Hashtable()
	{
		{ "Sand", new float[]{.9f, 1 } } // {block name, {walking sound pitch, running sound pitch}}
	};
	// for getting walking and running pitch for each block
	public static float[] getStepSoundPitch(string blockName)
	{
		if(blockName == null) return new float[] { 1.1f, 1.4f };
		float[] stepPitch = (float[])stepSoundPitches[blockName];
		if (stepPitch != null) return stepPitch;
		return new float[] { 1.1f, 1.4f };
	}

	public static object[] getSoundFolder(string blockName)
	{
		if (blockName == null) return prevStepSound;
		object[] folderInfo = (object[])blockToStepSoundFolder[blockName];

		if (folderInfo == null) prevStepSound = new object[] { "dirt", 4 };
		else prevStepSound = folderInfo;
		return prevStepSound;
	}

	public static BreakBehaviour getBreakBehaviour(string blockName)
	{
		return digBehaviours[blockName] as BreakBehaviour;
	}

	public static ItemDropBehaviour getItemDropBehaviour(string blockName, Vector2 blockPos) 
	{
		object dropBehaviour = itemDropBehaviours[blockName];

		if (dropBehaviour == null) return new ItemDropBehaviour();
		// blocks like tombstones and chests need their block positions passed in so we need to do a special case here
		if (dropBehaviour.Equals("Tombstone")) return new TombstoneDrop(blockPos);
		return dropBehaviour as ItemDropBehaviour;
	}

	public static RightClickBlockBehaviour getRightClickBehaviour(string blockName)
	{
		RightClickBlockBehaviour rcBehaviour = rightClickBehaviours[blockName] as RightClickBlockBehaviour;
		return rcBehaviour;
	}
}
