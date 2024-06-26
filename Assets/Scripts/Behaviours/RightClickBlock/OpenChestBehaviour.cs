using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class defines what happens when you right click a crafting table
 */
public class OpenChestBehaviour : RightClickBlockBehaviour
{
	private OpenChestScript openChestScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenChestScript>();

	public void rightClickBlock()
	{
		openChestScript.openChest();
	}
}
