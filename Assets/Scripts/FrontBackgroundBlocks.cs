using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FrontBackgroundBlocks
{
	// only blocks that you can place which need to go on the FrontBackground layer have to be here
	private static HashSet<string> frontLayerBlocks = new HashSet<string>()
	{
		{"SaplingOak"},
		{"Rose"},
		{"Dandelion"},
		{"Torch"},
		{"Tombstone"},
		{"Cactus"},
		{"DeadBush"},
		{"Grass"},
		{"MushroomBrown"},
		{"MushroomRed"},
	};
	// you can place blocks next to these blocks
	private static HashSet<string> frontLayerBlocksThatCanBePlacedNextTo = new HashSet<string>()
	{
		{"Tombstone"},
		{"Cactus"},
	};

	private static HashSet<string> fallTypes = new HashSet<string>()
	{
		{"Sand"},
		{"Gravel"}
	};

	public static bool isFallType(string blockName)
	{
		return fallTypes.Contains(blockName);
	}

	public static bool isFrontBackgroundBlock(string blockName)
	{
		return frontLayerBlocks.Contains(blockName);
	}

	public static bool isFrontBackgroundBlockPlaceableNextTo(string blockName)
	{
		return frontLayerBlocksThatCanBePlacedNextTo.Contains(blockName);
	}
}
