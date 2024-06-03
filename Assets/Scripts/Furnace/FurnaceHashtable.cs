using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FurnaceHashtable
{

	// this hashtable contains all of the burnable items, along with the burn time
	// the items in this table can be placed in the bottom slot in the furnace to burn the item in the top slot
	private static Hashtable burnableItems = new Hashtable() {
		{"Coal", 7.69f },   // higher burn time means the item is better at burning stuff
		{"Charcoal", 7.96f },
		{"PlankOak", 3.85f }, // 4 items worth
		{"PlankSpruce", 3.85f },
		{"LogOak", 5.77f }, // 6 items worth
		{"LogSpruce", 5.77f },
		{"CraftingTable", 5.77f},
		{"Stick", 1.93f}, // 2 items worth
		{"WoodAxe", 5.77f},
		{"WoodPickaxe", 5.77f},
		{"WoodSword", 5.77f},
		{"WoodShovel", 5.77f},
	};

	// the items in this hashtable are the items that can be burned in the top slot of the furnace
	private static Hashtable furnaceItems = new Hashtable() {
		{ "LogOak", "Charcoal"}, // {item that burns, the result item}
		{ "LogSpruce", "Charcoal"},
		{ "Cobblestone", "Stone"},
		{ "MuttonRaw", "MuttonCooked"},
		{ "PorkchopRaw", "PorkchopCooked"},
		{ "Sand", "Glass"},
		{ "IronOre", "IronIngot"}
	};
	/**
	 * returns the burnTime if the item is burnable, otherwise -1
	 */
	public static float getBurnTime(string itemName)
	{	
		float burnMultiplier = burnableItems[itemName] != null ? (float)burnableItems[itemName] : -1f;

		return burnMultiplier;
	}

	/**
	 * returns the item that is the result of item "itemName" burning in the furnace,
	 * returns the empty string if the item is not burnable
	 */
	public static string getBurnedItem(string itemName)
	{
		string resultItem = (string)furnaceItems[itemName] ?? "";

		return resultItem;
	}
}
