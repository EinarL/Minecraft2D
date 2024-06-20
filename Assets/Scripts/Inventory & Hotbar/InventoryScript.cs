using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

/**
 * This class holds the information that is in the players inventory.
 * it is responsible for adding and removing items from the inventory.
 */
public static class InventoryScript
{

	private static InventorySlot[] inventory; //stores what is in each slot
	private static int inventorySlots = 36;
	private static int maxStack = 64; // maximum amount of items in one slot
	private static HotbarScript hotbarScript;
	private static bool isInUI = false; // is the player in inventory, crafting table, furnace, etc.
	private static bool hasItemsPickedUp = false; // true if the player has items picked up in the inventory interface
	private static InventorySlot itemsPickedUp = new InventorySlot(); // the items that the player has on his cursor in the inventory UI, ["", 0] if he hasnt picked up anything

	private static InventorySlotScript[] inventorySlotScripts; // shows images of the item, hvertexture, picking up and putting down item in the invenotry

	private static IDataService dataService = JsonDataService.Instance;

	public static void initializeInventory()
	{
		hotbarScript = GameObject.Find("Canvas").transform.Find("Hotbar").GetComponent<HotbarScript>();

		inventorySlotScripts = new InventorySlotScript[inventorySlots];

		Transform invPanel = GameObject.Find("Canvas").transform.Find("InventoryParent").Find("Inventory").Find("InventoryPanel");

		for (int i = 0; i < inventorySlotScripts.Length; i++)
		{
			inventorySlotScripts[i] = invPanel.Find("InventorySlots").Find("InventorySlot" + i).GetComponent<InventorySlotScript>();
		}

		// check if player has inventory data, then load that
		if (dataService.exists("inventory.json"))
		{
			inventory = dataService.loadData<InventorySlot[]>("inventory.json");
			for (int i = 0; i < inventory.Length; i++)
			{
				updateSlotVisually(i); // update inventory visually
			}
		}
		else // else create an empty inventory
		{
			inventory = new InventorySlot[inventorySlots];
			for (int i = 0; i < inventory.Length; i++)
			{
				inventory[i] = new InventorySlot();
			}
		}
	}

	public static void setEmptyInventory()
	{
		inventory = new InventorySlot[inventorySlots];
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i] = new InventorySlot();
			updateSlotVisually(i);
		}
	}

	public static void saveInventory()
	{
		if (!dataService.saveData("inventory.json", inventory)) // save inventory
		{
			Debug.LogError("Could not save file :(");
		}
	}

	/**
	 * adds the item to the inventory if there is space for it.
	 * 
	 * returns: true if it found a spot for the item, false otherwise
	 */
	public static bool addToInventory(string itemName)
	{
		object[] ret = searchForSpot(itemName); // [didAddToInventory, which slot it added the item to]
		bool didAddToInventory = (bool)ret[0];
		int changedSlot = (int)ret[1];

		if(didAddToInventory) updateSlotVisually(changedSlot);

		return didAddToInventory;
	}

	public static bool addToInventory(ToolInstance tool, ArmorInstance armor, string itemName)
	{
		object[] ret = searchForSpot(tool, armor, itemName); // [didAddToInventory, which slot it added the item to]
		bool didAddToInventory = (bool)ret[0];
		int changedSlot = (int)ret[1];

		if (didAddToInventory) updateSlotVisually(changedSlot);

		return didAddToInventory;
	}
	/**
	 * adds the items into the players inventory but doesnt override items that are already in the inventory
	 * returns the items that don't fit into the inventory.
	 */
	public static List<InventorySlot> addItemsToInventory(InventorySlot[] items)
	{
		int itemIndex = 0;
		for(int i = 0; i < inventory.Length; i++)
		{
			while(itemIndex < items.Length && items[itemIndex].isEmpty())
			{
				itemIndex++;
			}
			if (itemIndex == items.Length) return null; // we have gone through all items, so return null
			if (!inventory[i].isEmpty()) continue;

			inventory[i] = items[itemIndex];
			updateSlotVisually(i);
			itemIndex++;
		}
		// Convert the array to a list
		List<InventorySlot> itemList = items.ToList();

		// return the sublist from itemIndex to the end of the list
		return itemList.Skip(itemIndex) as List<InventorySlot>;
	}

	/**
	 * adds the items in "itemsPickedUp" to the slot
	 * if addAll is false, then it only adds one of the items it is holding
	 * 
	 * returns: true if it is still holding items, but false if it got rid of all the items in itemsPickedUp
	 */

	public static bool addPickedUpItemsToSlot(int slotNumber, bool addAll)
	{
		if (!hasItemsPickedUp)
		{
			Debug.LogError("addPickedUpItemsToSlot() was called, but there arent any items that are picked up.");
			return true;
		}

		if (itemsPickedUp.isTool() || itemsPickedUp.isArmor() || BlockHashtable.isNotStackable(itemsPickedUp.itemName))
		{
			if (!inventory[slotNumber].isEmpty()) // if the slot is not empty
			{
				switchHeldItemsAndItemInSlot(slotNumber); // switch the items being held and the items in the slot
				return true;
			}
			else // otherwise just put the held tool/armor in the slot
			{
				inventory[slotNumber].putItemOrToolInSlot(itemsPickedUp.itemName, itemsPickedUp.toolInstance, itemsPickedUp.armorInstance, 1);
				updateSlotVisually(slotNumber);
			}

			return false;
		}

		if (addAll)
		{
			if (itemsPickedUp.itemName.Equals(inventory[slotNumber].itemName)) // if we're putting the same item type that already is in this slot
			{

				int leftOver = inventory[slotNumber].addItemsToSlot(itemsPickedUp.itemName, itemsPickedUp.amount);
				itemsPickedUp.amount = leftOver;
				updateSlotVisually(slotNumber);
				if (leftOver > 0) return true;
				return false;
			}
			if (!inventory[slotNumber].isEmpty()) // if there is already a tool or an item in this slot
			{
				// here we switch the items that are being held and the items that are in the slot
				switchHeldItemsAndItemInSlot(slotNumber);
				return true;
			}
			else // if the slot is empty
			{
				inventory[slotNumber].putItemInSlot(itemsPickedUp.itemName);
				inventory[slotNumber].setAmount(itemsPickedUp.amount);
				updateSlotVisually(slotNumber);
				return false;
			}
		}
		else // only add one
		{ 
			if (inventory[slotNumber].putItemInSlot(itemsPickedUp.itemName)) // if it did add the item to the slot
			{
				itemsPickedUp.amount--;
				updateSlotVisually(slotNumber);
				if (itemsPickedUp.amount > 0) return true; // return true if still holding items
			}

		}

		return false;
	}

	/**
	 * runs when the player has items held by the cursor and he left-clicks on a non-empty slot, then we switch the items
	 */
	private static void switchHeldItemsAndItemInSlot(int slotNumber)
	{
		string itemNameInSlot = inventory[slotNumber].itemName;
		ToolInstance toolInSlot = inventory[slotNumber].toolInstance;
		ArmorInstance armorInSlot = inventory[slotNumber].armorInstance;
		int amountInSlot = inventory[slotNumber].amount;
		inventory[slotNumber].putItemOrToolInSlot(itemsPickedUp.itemName, itemsPickedUp.toolInstance, itemsPickedUp.armorInstance, itemsPickedUp.amount);
		itemsPickedUp = new InventorySlot(itemNameInSlot, toolInSlot, armorInSlot, amountInSlot);
		updateSlotVisually(slotNumber);
	}

	
	public static void removeFromInventory(int slotNumber, int amount)
	{
		if (inventory[slotNumber].isEmpty()) return;

		inventory[slotNumber].removeFromSlot(amount);

		updateSlotVisually(slotNumber);
	}

	/**
	 * gets called when the tool that the player is using breaks
	 * this function deletes the tool being held from the inventory
	 */
	public static void breakToolBeingHeld()
	{
		int selectedSlot = hotbarScript.getSelectedSlot();

		if(inventory[selectedSlot].toolInstance == null)
		{
			Debug.LogError("breakToolBeingHeld() was executed but the player is not holding a tool, he's holding: " + inventory[selectedSlot].itemName);
			return;
		}

		inventory[selectedSlot] = new InventorySlot();
		updateSlotVisually(selectedSlot);
	}
	/*
	public static void removeFromInventory(int slotNumber, int amount)
	{
		if (inventory[slotNumber, 0].Equals("")) return; // nothing in this slot

		if(amount >= (int)inventory[slotNumber, 1]) // remove all of the items in this slot
		{
			inventory[slotNumber, 0] = "";
			inventory[slotNumber, 1] = 0;

		}
		else // remove some amount of items from this slot
		{
			inventory[slotNumber, 1] = (int)inventory[slotNumber, 1] - amount;
		}
		updateSlotVisually(slotNumber);
	}
	*/

	/**
	 * returns the tool that the player is holding, null if the player is not holding a tool
	 */
	public static ToolInstance getHeldTool()
	{
		ToolInstance heldTool = inventory[getSelectedSlot()].toolInstance;
		return heldTool;
	}

	// returns the item name of the item that the player is holding in the hotbar
	public static string getHeldItemName()
	{
		return inventory[getSelectedSlot()].itemName;
	}

	public static bool isHoldingFood()
	{
		return inventory[getSelectedSlot()].isFood;
	}

	public static string getItemBySlot(int slotNumber)
	{
		return inventory[slotNumber].itemName;
	}

	public static InventorySlot getItemsInSlot(int slotNumber)
	{
		return inventory[slotNumber];
	}

	public static int getSelectedSlot()
	{
		return hotbarScript.getSelectedSlot();
	}

	public static bool getIsInUI()
	{
		return isInUI;
	}

	public static void setIsInUI(bool value)
	{
		isInUI = value;
	}

	public static bool getHasItemsPickedUp()
	{
		return hasItemsPickedUp;
	}

	public static void setHasItemsPickedUp(bool value)
	{
		hasItemsPickedUp = value;
	}

	public static InventorySlot getItemsPickedUp()
	{
		return itemsPickedUp;
	}

	public static void setItemsPickedUp(InventorySlot item)
	{
		itemsPickedUp = item;
	}

	public static void setSelectedSlotItem(InventorySlot item)
	{
		int selectedSlot = hotbarScript.getSelectedSlot();
		inventory[selectedSlot] = item;
		updateSlotVisually(selectedSlot);
	}

	/**
	 * Searches the inventory for a spot for the tool
	 * 
	 * returns: true if it found a spot for the item, false otherwise; and the spot that it put the item in
	 */
	private static object[] searchForSpot(ToolInstance tool, ArmorInstance armor, string itemName)
	{
		for(int i = 0; i < inventorySlots; i++)
		{
			if (inventory[i].isEmpty()) // if the slot is empty
			{
				inventory[i].putItemOrToolInSlot(itemName, tool, armor, 1);
				return new object[] {true, i};
			}

		}
		return new object[] { false, -1 };
	}
	/**
	 * Searches the inventory for a spot for the item
	 * 
	 * returns: true if it found a spot for the item, false otherwise; and the spot that it put the item in
	 */
	private static object[] searchForSpot(string itemName)
	{
		int nullSpot = -1; // the first slot in the inventory that is empty
		for (int i = 0; i < inventory.Length; i++)
		{
			if (inventory[i].isEmpty() && nullSpot == -1) // if the slot is empty and we havent found an empty slot yet
			{
				nullSpot = i;
			}
			else if (inventory[i].itemName.Equals(itemName) && inventory[i].amount < maxStack) // if item is the same as in this slot
			{
				inventory[i].incrementAmount();
				return new object[] {true, i};
			}
		}
		// if we dont find a slot where the item already exists in the inventory, then can we add it to a slot that is null?
		if (nullSpot != -1) // if we found a slot that is empty
		{
			inventory[nullSpot].putItemInSlot(itemName); // add item to slot
			return new object[] { true, nullSpot }; 
		}
		return new object[] { false, -1 };
	}

	private static void updateSlotVisually(int slotNumber)
	{
		if (slotNumber < 9) hotbarScript.updateHotbarSlot(inventory[slotNumber].toolInstance != null ? inventory[slotNumber].toolInstance : inventory[slotNumber].armorInstance, slotNumber); // update hotbar visually
		inventorySlotScripts[slotNumber].updateSlot(inventory[slotNumber].toolInstance != null ? inventory[slotNumber].toolInstance : inventory[slotNumber].armorInstance); // update inventory visually
	}

	public static void updateHeldToolDurability(ToolInstance tool)
	{
		int selectedSlot = hotbarScript.getSelectedSlot();

		inventorySlotScripts[selectedSlot].updateDurabilityBar(tool);
		hotbarScript.updateDurabilityBar(tool, selectedSlot);
	}


	public static void decrementSlot(int slotNumber)
	{
		if (inventory[slotNumber].isEmpty()) return;

		inventory[slotNumber].removeFromSlot(1);

		updateSlotVisually(slotNumber);
	}

	public static bool hasArrow()
	{
		foreach(InventorySlot slot in inventory)
		{
			if (slot.itemName.Equals("Arrow")) return true;
		}
		return false;
	}

	public static void removeArrow()
	{
		for(int i = 0; i < inventory.Length; i++)
		{
			if (inventory[i].itemName.Equals("Arrow"))
			{
				inventory[i].removeFromSlot(1);
				updateSlotVisually(i);
				return;
			}
		}
		Debug.LogWarning("No arrow was found in the inventory");
	}

	public static bool hasSpaceFor(string itemName)
	{
		foreach (InventorySlot slot in inventory)
		{
			if ((slot.itemName.Equals(itemName) && slot.amount < 64) || slot.isEmpty()) return true;
		}
		return false;
	}

	public static InventorySlot[] getInventory()
	{
		return inventory;
	}
}
