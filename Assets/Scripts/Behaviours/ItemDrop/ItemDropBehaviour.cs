using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropBehaviour
{
	
	/**
	 * returns a gameObject item to drop
	 */
	public virtual List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool)
	{
		GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
		itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + gameObjectName); // change item texture to the block we were breaking

		return new List<GameObject> { itemToDrop };
	}
}
