using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderDrop : ItemDropBehaviour
{
	public override List<GameObject> dropItem(string gameObjectaName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		return base.dropItem("Ladder", null);
	}
}
