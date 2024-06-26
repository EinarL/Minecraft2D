using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChestScript : MonoBehaviour
{
    private GameObject ChestMenu;
    private GameObject darkBackground;
	private Transform inventoryPanel;
	private GameObject inventorySlots;


	// Start is called before the first frame update
	void Start()
    {
		ChestMenu = transform.Find("Chest").gameObject;
		darkBackground = transform.Find("DarkBackground").gameObject;
		ChestMenu.SetActive(false);

		inventoryPanel = transform.Find("Inventory").Find("InventoryPanel");
		inventorySlots = inventoryPanel.Find("InventorySlots").gameObject;
	}

    // Update is called once per frame
    void Update()
    {
		if (ChestMenu.activeSelf) // if chest is open
		{
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			{
				// close chest
				ChestMenu.SetActive(false);
				darkBackground.SetActive(false);
				InventoryScript.setIsInUI(false);
				inventorySlots.transform.SetParent(inventoryPanel); // put inventory slots back to inventory
			}

		}
	}
}
