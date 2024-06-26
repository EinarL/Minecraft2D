using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventorySlot
{
    public ArmorInstance armorInstance;
	public ToolInstance toolInstance; // if there not a tool in this slot then this will be null but itemName will have a name of the block that is in here
    public string itemName = ""; // itemName will never be "" except when the slot is empty, even when this slot has a tool, itemName will have the name of the tool
    public int amount = 0;
    public bool isFood = false;

    public InventorySlot()
    {
		toolInstance = null;
        itemName = "";
    }


	public InventorySlot(ToolInstance tool, string itemName)
	{
		toolInstance = tool;
		this.itemName = itemName;
		amount = 1;
        isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

	public InventorySlot(ArmorInstance armor, string itemName)
	{
		armorInstance = armor;
		this.itemName = itemName;
		amount = 1;
        toolInstance = null;
        isFood = false;
	}

	public InventorySlot(string itemName)
	{
        this.itemName = itemName;
        amount = 1;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

	public InventorySlot(string itemName, int amount)
	{
		this.itemName = itemName;
		this.amount = amount;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

	public InventorySlot(string itemName, ToolInstance tool, int amount)
	{
		this.itemName = itemName;
		this.amount = amount;
		toolInstance = tool;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

	public InventorySlot(string itemName, ToolInstance tool, ArmorInstance armor, int amount)
	{
		this.itemName = itemName;
		this.amount = amount;
		toolInstance = tool;
        armorInstance = armor;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

	public bool isEmpty()
    {
        return itemName.Equals("");
    }

    // returns true if there is a tool in this slot
    public bool isTool()
    {
        return toolInstance != null;
    }

	public bool isArmor()
	{
		return armorInstance != null;
	}

	public void putToolInSlot(ToolInstance tool, string itemName)
    {
        toolInstance = tool;
        this.itemName = itemName;
        amount = 1;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}


	public void putItemOrToolInSlot(string itemName, ToolInstance tool, ArmorInstance armor, int amount)
    {
		this.itemName = itemName;
		this.amount = amount;
        toolInstance = tool;
        armorInstance = armor;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
	}

    /**
     * returns true if it did put the item in the slot, otherwise false
     */
	public bool putItemInSlot(string itemName)
	{
        if(this.itemName.Equals(itemName))
        {
            if (amount >= 64 || BlockHashtable.isNotStackable(itemName)) return false;
            amount++;
            return true;
        }
		this.itemName = itemName;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;
		toolInstance = null;
        armorInstance = null;
		amount = 1;
        return true;
	}

	public void setAmount(int amount)
    {
        this.amount = amount;
    }

    /**
     * adds the items to the slot and returns the amount of items that werent able to go into the slot
     */
    public int addItemsToSlot(string itemName, int amount)
    {
        this.itemName = itemName;
        int totalAmount = this.amount + amount;
        if(totalAmount <= 64)
        {
            this.amount += amount;
            return 0;
        }
        this.amount = 64;
		isFood = FoodHashtable.getFoodAddition(itemName) >= 0;

		return totalAmount - this.amount;
    }


	public void incrementAmount()
    {
        amount++;
    }

	public void decrementAmount()
	{
		amount--;
        if (amount <= 0)
        {
            amount = 0;
            itemName = "";
            toolInstance = null;
            armorInstance = null;
            isFood = false;
        }
	}

	public void removeFromSlot(int amountToRemove)
    {
        amount -= amountToRemove;
        if (amount <= 0) {
            amount = 0;
			toolInstance = null;
            armorInstance = null;
            itemName = "";
            isFood = false;
        }
    }

    public void removeEverythingFromSlot()
    {
        amount = 0;
		toolInstance = null;
        armorInstance = null;
        itemName = "";
        isFood = false;
    }

    public InventorySlot copy()
    {
        return new InventorySlot(itemName, toolInstance, armorInstance, amount);
    }
}
