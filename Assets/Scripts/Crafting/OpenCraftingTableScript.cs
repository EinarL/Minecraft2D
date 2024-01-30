using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCraftingTableScript : MonoBehaviour
{
    private GameObject craftingMenu;
	private Transform craftingMenuPanel;
	private Transform inventoryPanel;
	private GameObject darkBackground;
	private GameObject inventorySlots;

	// Start is called before the first frame update
	void Start()
    {
		craftingMenu = transform.Find("CraftingMenu").gameObject;
		darkBackground = transform.Find("DarkBackground").gameObject;
		craftingMenu.SetActive(false);
		darkBackground.SetActive(false);

		inventoryPanel = transform.Find("Inventory").Find("InventoryPanel");
		craftingMenuPanel = craftingMenu.transform.Find("CraftingMenuPanel");
		inventorySlots = inventoryPanel.Find("InventorySlots").gameObject;
	}

    // Update is called once per frame
    void Update()
    {
		if (craftingMenu.activeSelf) // if crafting menu is open
		{
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			{
				// close crafting menu
				craftingMenu.SetActive(false);
				darkBackground.SetActive(false);
				InventoryScript.setIsInUI(false);
				inventorySlots.transform.SetParent(inventoryPanel); // put inventory slots back to inventory

				Craft.deleteItemFromResultSlot(false); // remove items from result slot, if any
				Craft.dropItemsFromSlots(false); // drop items from crafting slots, if any
			}

		}
    }

	public void openCraftingMenu()
	{
		if (InventoryScript.getIsInUI()) return; // if already in some UI, then dont open the crafting menu

		craftingMenu.SetActive(true);
		darkBackground.SetActive(true);
		InventoryScript.setIsInUI(true);

		inventorySlots.transform.SetParent(craftingMenuPanel); // put inventory slots in the crafting table
	}
}
