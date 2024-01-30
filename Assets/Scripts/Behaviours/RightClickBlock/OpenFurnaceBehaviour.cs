using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class defines what happens when you right click a crafting table
 */
public class OpenFurnaceBehaviour : RightClickBlockBehaviour
{
	private OpenFurnaceScript fScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();

	public void rightClickBlock()
	{
		fScript.openFurnaceMenu();
	}
}
