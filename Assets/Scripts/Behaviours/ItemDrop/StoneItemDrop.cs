using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneItemDrop : ItemDropBehaviour
{

	public StoneItemDrop()
	{

	}

	public override GameObject dropItem(string gameObjectName, ToolInstance usingTool)
	{
		if (usingTool == null) return null;

		if (usingTool.getToolType().Equals(ToolType.Pickaxe))
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\Cobblestone"); // change item texture
			return itemToDrop;
		}
		
		return null;
	}
}
