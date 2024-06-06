using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/**
 * this class has access to the crafting slots 
 * this class does the crafting logic, i.e. checking if you can craft some item based on what is in the crafting slots
 */
public static class Craft
{
	private static CraftingSlotScript[] inventoryCraftingSlots;
	private static ResultCraftingSlot inventoryResultSlot;
	private static CraftingSlotScript[] craftingTableCraftingSlots;
	private static ResultCraftingSlot craftingTableResultSlot;

	public static void initializeCrafting()
	{
		Transform inventoryParent = GameObject.Find("Canvas").transform.Find("InventoryParent");

		Transform invPanel = inventoryParent.Find("Inventory").Find("InventoryPanel");
		inventoryResultSlot = invPanel.Find("InventoryCrafting").Find("ResultCraftingSlot").GetComponent<ResultCraftingSlot>();

		inventoryCraftingSlots = new CraftingSlotScript[4];
		for (int i = 0; i < inventoryCraftingSlots.Length; i++)
		{
			inventoryCraftingSlots[i] = invPanel.Find("InventoryCrafting").Find("CraftingSlot" + i).GetComponent<CraftingSlotScript>();
		}

		Transform craftingMenuPanel = inventoryParent.Find("CraftingMenu").Find("CraftingMenuPanel");
		craftingTableResultSlot = craftingMenuPanel.Find("CraftingSlots").Find("ResultCraftingSlot").GetComponent<ResultCraftingSlot>();

		craftingTableCraftingSlots = new CraftingSlotScript[9];
		for(int i = 0; i < craftingTableCraftingSlots.Length; i++)
		{
			craftingTableCraftingSlots[i] = craftingMenuPanel.Find("CraftingSlots").Find("CraftingSlot" + i).GetComponent<CraftingSlotScript>();
		}
	}

	/**
	 * this function gets called when the player exits the inventory
	 * if the crafting slots have items in them, then they are dropped
	 * 
	 * bool inInventory: true if the player is crafting in the inventory, false if crafting in the crafting table
	 */
	public static void dropItemsFromSlots(bool inInventory)
	{
		CraftingSlotScript[] craftingSlots;
		if (inInventory) craftingSlots = inventoryCraftingSlots;
		else craftingSlots = craftingTableCraftingSlots;

		foreach (CraftingSlotScript craftingSlot in craftingSlots)
		{
			craftingSlot.dropFromSlot();
		}
	}

	/**
	 * this function gets called when player has crafted, i.e. taken an item from the resultSlot
	 * removes one amount from each crafting slot in the inventory
	 * 
	 * bool inInventory: true if the player is crafting in the inventory, false if crafting in the crafting table
	 */
	public static void deleteItemsFromSlots(bool inInventory)
	{
		CraftingSlotScript[] craftingSlots;
		if (inInventory) craftingSlots = inventoryCraftingSlots;
		else craftingSlots = craftingTableCraftingSlots;

		foreach (CraftingSlotScript craftingSlot in craftingSlots)
		{
			craftingSlot.removeOne();
		}
	}

	/**
	 * this function gets called when player exits the crafting menu or inventory
	 * it deletes the item in the result slot, if there is an item there
	 * 
	 * bool inInventory: true if the player is crafting in the inventory, false if crafting in the crafting table
	 */
	public static void deleteItemFromResultSlot(bool inInventory)
	{
		if (inInventory) inventoryResultSlot.setItemInSlot(new InventorySlot());
		else craftingTableResultSlot.setItemInSlot(new InventorySlot());
	}

	/**
	 * gets called each time an item is put in the crafting slots
	 * 
	 * checks whether its possible to craft anything based on what is in the inventory crafting slots
	 * if so, it displays what is possible to craft in the resultCraftingSlot
	 * 
	 * bool isCraftingInInventory: if this is true, then the player is crafting in the inventory, otherwise crafting in the crafting table
	 */
	public static void checkIfCanCraft(bool isCraftingInInventory)
	{
		CraftingSlotScript[] craftingSlots;
		ResultCraftingSlot resultSlot;
		if (isCraftingInInventory)
		{
			craftingSlots = inventoryCraftingSlots;
			resultSlot = inventoryResultSlot;
		}
        else // crafting in crafting table
        {
			craftingSlots = craftingTableCraftingSlots;
			resultSlot = craftingTableResultSlot;
        }

        for (int i = 0; i < craftingSlots.Length; i++)
		{
			if (!craftingSlots[i].itemInSlot.isEmpty()) // found the left-bottom most item
			{
				Vector2 bottomLeftMostSlotPos = craftingSlots[i].slotPosition;
				List<object[]> possibleRecipes = CraftingRecipes.getRecipesByLeftBottomMostItem(craftingSlots[i].itemInSlot.itemName);
				foreach (object[] recipe in possibleRecipes)
				{
					bool correctRecipe = true;
					int itemNeededIndex = 0;
					object[] itemsNeeded = (object[])recipe[1];
					if (itemsNeeded.Length == 0)
					{
						for (int k = i + 1; k < craftingSlots.Length; k++)
						{
							if (!craftingSlots[k].itemInSlot.isEmpty()) // if there are more items in the crafting slots, then its not a correct recipe
							{
								Debug.Log("there are more items in the slots that are not a part of the recipe :(");
								correctRecipe = false;
								break;
							}
						}
						if (correctRecipe)
						{
							showCraftingResult(recipe[2] as InventorySlot, resultSlot);
							return;
						}
						continue;
					}

					object[] currentItemNeeded = (object[])itemsNeeded[itemNeededIndex];
					for (int j = i + 1; j < craftingSlots.Length; j++)
					{
						if (!craftingSlots[j].itemInSlot.isEmpty()) // if slot is not empty
						{

							// if the item in the slot is the same item that is needed in the recipe
							if (craftingSlots[j].itemInSlot.itemName.Equals(currentItemNeeded[0]))
							{
								// check if the distance from the bottom-left most item is the same
								Vector2 diff = craftingSlots[j].slotPosition - bottomLeftMostSlotPos;
								if (diff != (Vector2)currentItemNeeded[1])
								{
									correctRecipe = false;
									break;
								}
								else // if the item in this slot is the correct one for the recipe
								{
									itemNeededIndex++;
									if(itemNeededIndex == itemsNeeded.Length) // if we have reached the end of the recipe
									{
										for(int k = j+1; k < craftingSlots.Length; k++)
										{
											if (!craftingSlots[k].itemInSlot.isEmpty()) // if there are more items in the crafting slots, then its not a correct recipe
											{
												correctRecipe = false;
												break;
											}
										}
										break;
									}
									currentItemNeeded = (object[])itemsNeeded[itemNeededIndex];
								}
							}
							else
							{
								correctRecipe = false;
								break;
							}
						}

					}
					if (correctRecipe && itemNeededIndex == itemsNeeded.Length)
					{
						showCraftingResult(recipe[2] as InventorySlot, resultSlot);
						return;
					}
				}
				// if the code reaches here, then we know that there doesnt exist a recipe for what is in the crafting menu
				break;
			}

		}
		resultSlot.setItemInSlot(new InventorySlot());
	}

	/**
	 * shows the crafting result in the "resultSlot" result crafting slot
	 */
	private static void showCraftingResult(InventorySlot items, ResultCraftingSlot resultSlot)
	{
		InventorySlot craftingResult;
		if (items.isTool()) // if we crafted a tool
		{
			craftingResult = new InventorySlot(items.toolInstance, items.itemName);
			Debug.Log("can craft " + items.toolInstance.getToolMaterial() + " " + items.toolInstance.getToolType());
		}
		else if (items.isArmor()) // if its an armor
		{
			craftingResult = new InventorySlot(items.armorInstance, items.itemName);
			Debug.Log("can craft " + items.armorInstance.getArmorMaterial() + " " + items.armorInstance.getArmorType());
		}
		else // if we did not craft a tool
		{
			craftingResult = new InventorySlot(items.itemName, items.amount);
			Debug.Log("can craft " + craftingResult.itemName);
		}
		resultSlot.setItemInSlot(craftingResult);
	}
}
