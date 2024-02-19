using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondItemDrop : ItemDropBehaviour
{
	public override GameObject dropItem(string gameObjectName, ToolInstance usingTool)
	{
		// if pickaxe && not wood && not stone
		if (usingTool.getToolType().Equals(ToolType.Pickaxe) && !usingTool.getToolMaterial().Equals(ToolMaterial.Wood) && !usingTool.getToolMaterial().Equals(ToolMaterial.Stone))
		{
			return base.dropItem("Diamond", usingTool);
		}
		return null;
	}
}
