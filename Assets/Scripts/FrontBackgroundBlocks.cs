using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FrontBackgroundBlocks
{
	// only blocks that you can place which need to go on the FrontBackground layer have to be here
	public static HashSet<string> frontLayerBlocks = new HashSet<string>()
	{
		{"SaplingOak"},
		{"Rose"},
		{"Dandelions"}
	};

	public static bool isFrontBackgroundBlock(string blockName)
	{
		return frontLayerBlocks.Contains(blockName);
	}
}
