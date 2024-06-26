using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class defines what happens when you right click a crafting table
 */
public class OpenChestBehaviour : RightClickBlockBehaviour
{
	private OpenCraftingTableScript ctScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenCraftingTableScript>();

	public void rightClickBlock()
	{
		// TODO: open chest
	}
}
