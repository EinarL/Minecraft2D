using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class CraftingRecipes
{
	/**
	 * make sure that the "list of items and their distances from bottom-left most item" is ordered by bottom-left most item first, going right and then up-left
	 * not incliding the bottom-left most item of course.
	 */
	private static object[] recipes = new object[]
	{
		// [left-bottom most item, list of items and their distances from bottom-left most item, crafting result]

		// planks
		new object[]{"LogOak", new object[] {}, new InventorySlot("PlankOak",4) },
		new object[]{"LogSpruce", new object[] {}, new InventorySlot("PlankSpruce",4) },

		// sticks
		new object[]{"PlankOak", new object[] { new object[] { "PlankOak", new Vector2(0, 1) } }, new InventorySlot("Stick", 4) },
		new object[]{"PlankSpruce", new object[] { new object[] { "PlankSpruce", new Vector2(0, 1) } }, new InventorySlot("Stick", 4) },

		// crafting table
		new object[]{ "PlankOak", new object[] {
			new object[] { "PlankOak", new Vector2(1, 0) },
			new object[] { "PlankOak", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(1, 1) }
		}, new InventorySlot("CraftingTable", 1)
		},
		new object[]{ "PlankSpruce", new object[] {
			new object[] { "PlankSpruce", new Vector2(1, 0) },
			new object[] { "PlankSpruce", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(1, 1) }
		}, new InventorySlot("CraftingTable", 1)
		},

		// doors
		new object[]{ "PlankOak", new object[] {
			new object[] { "PlankOak", new Vector2(1, 0) },
			new object[] { "PlankOak", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(1, 1) },
			new object[] { "PlankOak", new Vector2(0, 2) },
			new object[] { "PlankOak", new Vector2(1, 2) },
		}, new InventorySlot("DoorOak", 1)
		},
		new object[]{ "PlankSpruce", new object[] {
			new object[] { "PlankSpruce", new Vector2(1, 0) },
			new object[] { "PlankSpruce", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(1, 1) },
			new object[] { "PlankSpruce", new Vector2(0, 2) },
			new object[] { "PlankSpruce", new Vector2(1, 2) },
		}, new InventorySlot("DoorSpruce", 1)
		},

		// wooden tools
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodShovel")), "WoodShovel")
		},
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodShovel")), "WoodShovel")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(1, 1) },
			new object[] { "PlankOak", new Vector2(0, 2) },
			new object[] { "PlankOak", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodAxe")), "WoodAxe")
		},
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(1, 1) },
			new object[] { "PlankSpruce", new Vector2(0, 2) },
			new object[] { "PlankSpruce", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodAxe")), "WoodAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "PlankOak", new Vector2(-1, 1) },
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(-1, 2) },
			new object[] { "PlankOak", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodAxe")), "WoodAxe")
		},
		new object[]{ "Stick", new object[] {
			new object[] { "PlankSpruce", new Vector2(-1, 1) },
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(-1, 2) },
			new object[] { "PlankSpruce", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodAxe")), "WoodAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(-1, 2) },
			new object[] { "PlankOak", new Vector2(0, 2) },
			new object[] { "PlankOak", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodPickaxe")), "WoodPickaxe")
		},
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(-1, 2) },
			new object[] { "PlankSpruce", new Vector2(0, 2) },
			new object[] { "PlankSpruce", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodPickaxe")), "WoodPickaxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "PlankOak", new Vector2(0, 1) },
			new object[] { "PlankOak", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodSword")), "WoodSword")
		},
		new object[]{ "Stick", new object[] {
			new object[] { "PlankSpruce", new Vector2(0, 1) },
			new object[] { "PlankSpruce", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("WoodSword")), "WoodSword")
		},

		// stone tools
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("StoneShovel")), "StoneShovel")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(1, 1) },
			new object[] { "Cobblestone", new Vector2(0, 2) },
			new object[] { "Cobblestone", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("StoneAxe")), "StoneAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Cobblestone", new Vector2(-1, 1) },
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(-1, 2) },
			new object[] { "Cobblestone", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("StoneAxe")), "StoneAxe")
		},
		
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(-1, 2) },
			new object[] { "Cobblestone", new Vector2(0, 2) },
			new object[] { "Cobblestone", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("StonePickaxe")), "StonePickaxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Cobblestone", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("StoneSword")), "StoneSword")
		},

		// iron tools
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "IronIngot", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("IronShovel")), "IronShovel")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "IronIngot", new Vector2(1, 1) },
			new object[] { "IronIngot", new Vector2(0, 2) },
			new object[] { "IronIngot", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("IronAxe")), "IronAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "IronIngot", new Vector2(-1, 1) },
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "IronIngot", new Vector2(-1, 2) },
			new object[] { "IronIngot", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("IronAxe")), "IronAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "IronIngot", new Vector2(-1, 2) },
			new object[] { "IronIngot", new Vector2(0, 2) },
			new object[] { "IronIngot", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("IronPickaxe")), "IronPickaxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "IronIngot", new Vector2(0, 1) },
			new object[] { "IronIngot", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("IronSword")), "IronSword")
		},

		// diamond tools
		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Diamond", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("DiamondShovel")), "DiamondShovel")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Diamond", new Vector2(1, 1) },
			new object[] { "Diamond", new Vector2(0, 2) },
			new object[] { "Diamond", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("DiamondAxe")), "DiamondAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Diamond", new Vector2(-1, 1) },
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Diamond", new Vector2(-1, 2) },
			new object[] { "Diamond", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("DiamondAxe")), "DiamondAxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Stick", new Vector2(0, 1) },
			new object[] { "Diamond", new Vector2(-1, 2) },
			new object[] { "Diamond", new Vector2(0, 2) },
			new object[] { "Diamond", new Vector2(1, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("DiamondPickaxe")), "DiamondPickaxe")
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Diamond", new Vector2(0, 1) },
			new object[] { "Diamond", new Vector2(0, 2) }
		}, new InventorySlot( new ToolInstance(getToolScriptable("DiamondSword")), "DiamondSword")
		},

		new object[]{ "Cobblestone", new object[] {
			new object[] { "Cobblestone", new Vector2(1, 0) },
			new object[] { "Cobblestone", new Vector2(2, 0) },
			new object[] { "Cobblestone", new Vector2(0, 1) },
			new object[] { "Cobblestone", new Vector2(2, 1) },
			new object[] { "Cobblestone", new Vector2(0, 2) },
			new object[] { "Cobblestone", new Vector2(1, 2) },
			new object[] { "Cobblestone", new Vector2(2, 2) }
		}, new InventorySlot("Furnace", 1)
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Coal", new Vector2(0, 1) }
		}, new InventorySlot("Torch", 4)
		},

		new object[]{ "Stick", new object[] {
			new object[] { "Charcoal", new Vector2(0, 1) }
		}, new InventorySlot("Torch", 4)
		},

		new object[]{ "SnowBall", new object[] {
			new object[] { "SnowBall", new Vector2(1, 0) },
			new object[] { "SnowBall", new Vector2(0, 1) },
			new object[] { "SnowBall", new Vector2(1, 1) }
		}, new InventorySlot("SnowBlock", 1)
		},

		// bed
		new object[]{ "PlankOak", new object[] {
			new object[] { "PlankOak", new Vector2(1, 0) },
			new object[] { "PlankOak", new Vector2(2, 0) },
			new object[] { "Wool", new Vector2(0, 1) },
			new object[] { "Wool", new Vector2(1, 1) },
			new object[] { "Wool", new Vector2(2, 1) }
		}, new InventorySlot("Bed", 1)
		},
		new object[]{ "PlankSpruce", new object[] {
			new object[] { "PlankSpruce", new Vector2(1, 0) },
			new object[] { "PlankSpruce", new Vector2(2, 0) },
			new object[] { "Wool", new Vector2(0, 1) },
			new object[] { "Wool", new Vector2(1, 1) },
			new object[] { "Wool", new Vector2(2, 1) }
		}, new InventorySlot("Bed", 1)
		},
	};

	public static List<object[]> getRecipesByLeftBottomMostItem(string itemName)
	{
		List<object[]> recipesToReturn = new List<object[]>();

		foreach (object[] recipe in recipes)
		{
			if (recipe[0].Equals(itemName))
			{
				recipesToReturn.Add(recipe);
			}
		}
		return recipesToReturn;
	}

	private static Tool getToolScriptable(string toolName)
	{
		return Resources.Load<Tool>("ToolScriptables\\" + toolName);
	}
}
