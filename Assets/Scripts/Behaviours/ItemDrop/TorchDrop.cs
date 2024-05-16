using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchDrop : ItemDropBehaviour
{
	public TorchDrop()
	{
	}


	public override List<GameObject> dropItem(string gameObjectaName, ToolInstance usingTool)
	{
		return base.dropItem("Torch", null);
	}
}
