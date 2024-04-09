using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockBehaviourData
{
	private static Hashtable digBehaviours = new Hashtable() { // hashtable that holds the class for the different block sounds and speed when breaking them
		{ "Dirt", new BreakDirt() },
		{ "LogOak", new BreakWood() },
		{ "LeavesOak", new BreakLeaves() },
		{ "SaplingOak", new BreakInstantly() },
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
		{ "Wool", new BreakCloth() },
		{ "Furnace", new BreakStone() },
		{ "Glass", new BreakGlass() },
		{ "Bedrock", new BreakBedrock() }

	};

	private static Hashtable itemDropBehaviours = new Hashtable()
	{
		{ "LeavesOak", new LeafItemDrop() },
		{ "Stone", new StoneItemDrop() },
		{ "IronOre", new StonePickOrBetter() },
		{ "DiamondOre", new DiamondItemDrop()},
		{ "DeadBush", new DeadBushItemDrop() },
		{ "Grass", new DropNothing() },
		{ "Furnace", new PickaxeDrop() },
		{ "CoalOre", new PickaxeDrop() },
		{ "Glass", new DropNothing() }
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
		{ "Bedrock", new object[]{ "stone", 6 } }

	};

	private static Hashtable stepSoundPitches = new Hashtable()
	{
		{ "Sand", new float[]{.9f, 1 } } // {block name, {walking sound pitch, running sound pitch}}
	};
	// for getting walking and running pitch for each block
	public static float[] getStepSoundPitch(string blockName)
	{
		float[] stepPitch = (float[])stepSoundPitches[blockName];
		if (stepPitch != null) return stepPitch;
		return new float[] { 1.1f, 1.4f };
	}

	public static object[] getSoundFolder(string blockName)
	{
		object[] folderInfo = (object[])blockToStepSoundFolder[blockName];

		if (folderInfo == null) return new object[] { "dirt", 4 };
		return folderInfo;
	}

	public static BreakBehaviour getBreakBehaviour(string blockName)
	{
		return digBehaviours[blockName] as BreakBehaviour;
	}

	public static ItemDropBehaviour getItemDropBehaviour(string blockName) 
	{
		ItemDropBehaviour dropBehaviour = itemDropBehaviours[blockName] as ItemDropBehaviour;

		if (dropBehaviour == null) return new ItemDropBehaviour();
		return dropBehaviour;
	}

	public static RightClickBlockBehaviour getRightClickBehaviour(string blockName)
	{
		RightClickBlockBehaviour rcBehaviour = rightClickBehaviours[blockName] as RightClickBlockBehaviour;
		return rcBehaviour;
	}
}
