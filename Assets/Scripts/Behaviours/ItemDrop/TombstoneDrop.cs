using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TombstoneDrop : ItemDropBehaviour
{
	private Vector2 blockPos;
	private JsonDataService dataService = new JsonDataService();

	public TombstoneDrop(Vector2 blockPos)
	{
		this.blockPos = blockPos;
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool)
	{
		List<object[]> tombstones = dataService.loadData<List<object[]>>("tombstone.json"); // returns [[xPos, yPos, inv], [xPos, yPos, inv], ...]

		dataService.removeTombstoneData(blockPos.x, blockPos.y);

		foreach (object[] tombstone in tombstones)
		{
			if ((Double)tombstone[0] == blockPos.x && (Double)tombstone[1] == blockPos.y) // if we find the corresponding tombstone
			{
				InventorySlot[] inventory = JsonConvert.DeserializeObject<InventorySlot[]>(JsonConvert.SerializeObject(tombstone[2]));
				// we want to add this inventory to the players inventory, if there isnt space for some items, then we drop them
				InventorySlot[] remainingItems = InventoryScript.addItemsToInventory(inventory);
				if(remainingItems != null) // if there are remaining items we need to drop
				{
					List<GameObject> itemsToDrop = new List<GameObject>();
					foreach (InventorySlot item in remainingItems)
					{
						if (item.isEmpty()) continue;
						GameObject itemToDrop = GameObject.Instantiate(Resources.Load("Prefabs\\ItemContainer") as GameObject);

						itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + item.itemName); // change item texture 


						DroppedItemScript itemScript = itemToDrop.GetComponent<DroppedItemScript>();
						itemScript.tool = item.toolInstance;

						for(int i = 0; i < item.amount; i++)
						{
							itemsToDrop.Add(itemToDrop);
						}
					}
					return itemsToDrop;
				}
				return null;
			}
		}

		Debug.LogError("Did not find a corresponding tombstone to the one you broke :(");
		return null;
	}
}
