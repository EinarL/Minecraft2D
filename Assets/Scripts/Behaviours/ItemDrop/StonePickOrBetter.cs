using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonePickOrBetter : ItemDropBehaviour
{
	public override List<GameObject> dropItem(string gameObjectaName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		// if pickaxe && not wood
		if(usingTool.getToolType().Equals(ToolType.Pickaxe) && !usingTool.getToolMaterial().Equals(ToolMaterial.Wood))
		{
			return base.dropItem(gameObjectaName, usingTool);
		}
		return null;
	}
}
