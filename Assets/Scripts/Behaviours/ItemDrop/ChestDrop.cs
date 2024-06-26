using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : ItemDropBehaviour
{
	private Vector2 blockPos;
	private JsonDataService dataService = JsonDataService.Instance;
	private OpenChestScript openChestScript;

	public ChestDrop(Vector2 blockPos)
	{
		this.blockPos = blockPos;
		openChestScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenChestScript>();
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		List<Chest> chests = dataService.loadData<List<Chest>>("chests.json"); 

		foreach (Chest chest in chests)
		{
			if (chest.x == blockPos.x && chest.y == blockPos.y) // if we find the corresponding chest
			{
				InventorySlot[] chestContent = chest.inventorySlots;

				foreach (InventorySlot item in chestContent) // drop items
				{
					dropItem(item);
				}

				dataService.removeChestData(blockPos.x, blockPos.y);
				openChestScript.removeChest(blockPos.x, blockPos.y);
				break;
			}
		}
		dropItem(new InventorySlot("Chest"));
		return null;
	}

	private void dropItem(InventorySlot item)
	{
		if (item.isEmpty()) return;
		GameObject itemToDrop = Resources.Load<GameObject>("Prefabs\\ItemContainer");
		GameObject itemObject = itemToDrop.transform.Find("Item").gameObject; // get item within itemContainer

		itemObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + item.itemName); // change item texture 

		for (int i = 0; i < item.amount; i++)
		{
			GameObject itemInstance = GameObject.Instantiate(itemToDrop, blockPos, Quaternion.identity);

			DroppedItemScript itemScript = itemInstance.GetComponent<DroppedItemScript>();
			itemScript.tool = item.toolInstance;
			itemScript.armor = item.armorInstance;
		}
	}
}
