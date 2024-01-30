using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronPickOrBetter : ItemDropBehaviour
{
	public override GameObject dropItem(string gameObjectaName, ToolInstance usingTool)
	{
		// if pickaxe && not wood && not stone
		if(usingTool.getToolType().Equals(ToolType.Pickaxe) && !usingTool.getToolMaterial().Equals(ToolMaterial.Wood) && !usingTool.getToolMaterial().Equals(ToolMaterial.Stone))
		{
			return base.dropItem(gameObjectaName, usingTool);
		}
		return null;
	}
}
