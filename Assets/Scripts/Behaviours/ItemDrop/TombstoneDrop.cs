using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TombstoneDrop : ItemDropBehaviour
{
	private Vector2 blockPos;
	private JsonDataService dataService = JsonDataService.Instance;
	private ArmorScript armorScript = GameObject.Find("Canvas").transform.Find("Armorbar").GetComponent<ArmorScript>();

	public TombstoneDrop(Vector2 blockPos)
	{
		this.blockPos = blockPos;
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		List<object[]> tombstones = dataService.loadData<List<object[]>>("tombstone.json"); // returns [[xPos, yPos, inv], [xPos, yPos, inv], ...]

		dataService.removeTombstoneData(blockPos.x, blockPos.y);

		foreach (object[] tombstone in tombstones)
		{
			if ((Double)tombstone[0] == blockPos.x && (Double)tombstone[1] == blockPos.y) // if we find the corresponding tombstone
			{
				InventorySlot[] inventory = JsonConvert.DeserializeObject<InventorySlot[]>(JsonConvert.SerializeObject(tombstone[2]));
				InventorySlot[] armorSlots = JsonConvert.DeserializeObject<InventorySlot[]>(JsonConvert.SerializeObject(tombstone[3]));
				// we want to add this inventory to the players inventory, if there isnt space for some items, then we drop them
				List<InventorySlot> remainingItems = InventoryScript.addItemsToInventory(inventory);

				List<InventorySlot> remainingArmors = armorScript.addArmor(armorSlots); // we want to add this to the players armor slots, otherwise inventory, othewise drop them
				if(remainingItems != null)
				{
					remainingArmors.AddRange(remainingItems); // add the items to the remaining armors, remainingArmors now contains all of the items we need to drop
				}
				else // if there is space in the inventory (where we can put the armor instead of in the armor slot which is full)
				{
					remainingArmors = InventoryScript.addItemsToInventory(remainingArmors.ToArray());
				}

				if (remainingArmors != null && remainingArmors.Count > 0) // if there are remaining items we need to drop // (ignore the remainingArmors variable name, this list contains all of the items that dont fit in the inv)
				{
					List<GameObject> itemsToDrop = new List<GameObject>();
					foreach (InventorySlot item in remainingArmors)
					{
						if (item.isEmpty()) continue;
						GameObject itemToDrop = Resources.Load<GameObject>("Prefabs\\ItemContainer");
						GameObject itemObject = itemToDrop.transform.Find("Item").gameObject; // get item within itemContainer

						itemObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + item.itemName); // change item texture 


						DroppedItemScript itemScript = itemToDrop.GetComponent<DroppedItemScript>();
						itemScript.tool = item.toolInstance;
						itemScript.armor = item.armorInstance;

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
