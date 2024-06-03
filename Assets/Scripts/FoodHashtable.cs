using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FoodHashtable
{

	/**
	 * hashtable with all of the edible items, value is the amount of food bars it adds
	 * 1 corresponds to half a hunger, 2 to 1 hunger, 3 to 1.5 hunger, etc.
	 */
	private static Hashtable foodHashtable = new Hashtable() {
		{"Apple", 4},
		{"MuttonRaw", 1},
		{"MuttonCooked", 8},
		{"PorkchopRaw", 1},
		{"PorkchopCooked", 8},
		{"RottenFlesh", 1}
	};
	// returns the amount of food bars the food adds, returns -1 if it's not a food
	public static int getFoodAddition(string foodName)
	{
		return foodHashtable.ContainsKey(foodName) ? (int)foodHashtable[foodName] : -1;
	}
}
