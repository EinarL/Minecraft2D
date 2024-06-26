using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class OpenChestScript : MonoBehaviour
{
    private GameObject ChestMenu;
	private Transform ChestPanel;
	private GameObject darkBackground;
	private Transform inventoryPanel;
	private GameObject inventorySlots;
	private Camera cam;

	private ChestSlotScript[] chestSlots;

	private IDataService dataService = JsonDataService.Instance;

	private List<Chest> chests = new List<Chest>();
	private Chest openedChest = null;

	// Start is called before the first frame update
	void Start()
    {
		ChestMenu = transform.Find("Chest").gameObject;
		ChestPanel = ChestMenu.transform.Find("ChestPanel");
		darkBackground = transform.Find("DarkBackground").gameObject;
		ChestMenu.SetActive(false);

		inventoryPanel = transform.Find("Inventory").Find("InventoryPanel");
		inventorySlots = inventoryPanel.Find("InventorySlots").gameObject;

		cam = Camera.main;

		chestSlots = ChestPanel.GetComponentsInChildren<ChestSlotScript>();

		// load chest data if it exists
		if (dataService.exists("chests.json"))
		{
			chests = dataService.loadData<List<Chest>>("chests.json");
		}
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

				if (openedChest != null)
				{
					for(int i = 0; i < chestSlots.Length; i++)
					{
						openedChest.inventorySlots[i] = chestSlots[i].itemInSlot;
					}

					openedChest = null;
				}
				else Debug.LogError("opened chest is null, so changes are not saved");

				for (int i = 0; i < chestSlots.Length; i++)
				{
					chestSlots[i].updateSlot(new InventorySlot()); // add the items to the chest
				}
				saveChests();
			}
		}
	}

	public void openChest()
	{
		if (InventoryScript.getIsInUI()) return; // if already in some UI, then dont open the furnace
		ChestMenu.SetActive(true);
		darkBackground.SetActive(true);
		InventoryScript.setIsInUI(true);

		inventorySlots.transform.SetParent(ChestPanel); // put inventory slots in the chest menu

		Vector2 mousePos = getRoundedMousePos();

		foreach (Chest chest in chests) // find the corresponding chest that we right clicked
		{
			if (chest.x == mousePos.x && chest.y == mousePos.y)
			{
				openedChest = chest;
				break;
			}
		}
		if (openedChest != null)
		{
			for(int i = 0; i < openedChest.inventorySlots.Length; i++)
			{
				chestSlots[i].updateSlot(openedChest.inventorySlots[i]); // add the items to the chest
			}
		}
		else // if we are opening a chest for the first time, then it wont be in the chests array, so we will add a new chest
		{
			openedChest = new Chest(mousePos.x, mousePos.y);
			chests.Add(openedChest);
		}
	}

	public void saveChests()
	{
		if (!dataService.saveData("chests.json", chests)) // save furnaces
		{
			Debug.LogError("Could not save chests data file :(");
		}
	}

	public void removeChest(float x, float y)
	{
		chests.RemoveAll(chest => chest.x == x && chest.y == y);
	}

	private Vector2 getRoundedMousePos()
	{
		Vector3 mousePos = Input.mousePosition;
		Vector3 worldMousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
		Vector2 RoundedMousePos = new Vector2((float)Math.Round(worldMousePos.x + 0.5f) - 0.5f, (float)Math.Round(worldMousePos.y + 0.5f) - 0.5f); // round it to the closes possible "block position"
		return RoundedMousePos;
	}
}


[Serializable]
public class Chest
{
	public float x { get; set; }
	public float y { get; set; }
	public InventorySlot[] inventorySlots { get; set; }

	public Chest(float x, float y)
	{
		this.x = x;
		this.y = y;

		inventorySlots = new InventorySlot[27];
		for(int i = 0; i < inventorySlots.Length; i++)
		{
			inventorySlots[i] = new InventorySlot();
		}
	}
}