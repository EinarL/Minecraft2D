using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDrop : ItemDropBehaviour
{

	public PickaxeDrop()
	{

	}
	// only drops the item if its mined with a pickaxe
	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool)
	{
		if (usingTool == null) return null;

		if (usingTool.getToolType().Equals(ToolType.Pickaxe))
		{
			if (gameObjectName == "CoalOre" || gameObjectName == "DiamondOre") gameObjectName = gameObjectName.Replace("Ore", "").Trim();
			return base.dropItem(gameObjectName, usingTool);
		}

		return null;
	}
}
