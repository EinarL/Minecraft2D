using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static UnityEditor.Progress;

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
	private Animator openedChestAnimator = null; // the animator for the opened chest
	private AudioClip openChestSound;
	private AudioClip closeChestSound;

	// Start is called before the first frame update
	void Start()
    {
		ChestMenu = transform.Find("Chest").gameObject;
		ChestPanel = ChestMenu.transform.Find("ChestPanel");
		darkBackground = transform.Find("DarkBackground").gameObject;
		ChestMenu.SetActive(false);

		inventoryPanel = transform.Find("Inventory").Find("InventoryPanel");
		inventorySlots = inventoryPanel.Find("InventorySlots").gameObject;

		openChestSound = Resources.Load<AudioClip>("Sounds\\Random\\chestopen");
		closeChestSound = Resources.Load<AudioClip>("Sounds\\Random\\chestclosed");

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
					AudioSource.PlayClipAtPoint(closeChestSound, new Vector2(openedChest.x, openedChest.y)); // play close sound
					if (openedChestAnimator != null) openedChestAnimator.SetBool("isOpen", false);
					openedChest = null;
					openedChestAnimator = null;
				}
				else Debug.LogError("opened chest is null, so changes are not saved");

				for (int i = 0; i < chestSlots.Length; i++)
				{
					chestSlots[i].updateSlot(new InventorySlot(), false); // add the items to the chest
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
		setChestAnimator(openedChest.x, openedChest.y);
		if (openedChestAnimator != null) openedChestAnimator.SetBool("isOpen", true);
		AudioSource.PlayClipAtPoint(openChestSound, new Vector2(openedChest.x, openedChest.y)); // play open sound
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

	private void setChestAnimator(float x, float y)
	{
		List<Collider2D> colliders = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "BackBackground"

		// Check for overlaps
		Physics2D.OverlapCircle(new Vector2(x,y), 0.1f, filter, colliders);

		foreach(Collider2D collider in colliders)
		{
			if (collider.gameObject.name.StartsWith("Chest"))
			{
				openedChestAnimator = collider.GetComponent<Animator>();
				return;
			}
		}
		Debug.LogError("Did not find the chest at position: " + x + ", " + y);
	}

	private Vector2 getRoundedMousePos()
	{
		Vector3 mousePos = Input.mousePosition;
		Vector3 worldMousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
		Vector2 RoundedMousePos = new Vector2((float)Math.Round(worldMousePos.x + 0.5f) - 0.5f, (float)Math.Round(worldMousePos.y + 0.5f) - 0.5f); // round it to the closes possible "block position"
		return RoundedMousePos;
	}

	public bool isOpen()
	{
		return ChestMenu.activeSelf;
	}

	/**
	 * tries to add the item to the opened chest if there is space.
	 * returns an int, amount of the item that is left (that didnt fit in the chest)
	 */
	public int addToOpenedChest(InventorySlot item)
	{
		if (!isOpen())
		{
			Debug.LogError("tried to add to a chest that isnt open");
			return item.amount;
		}
		if(openedChest == null)
		{
			Debug.LogError("tried to add to a chest but openedChest is null");
			return item.amount;
		}
		

		return openedChest.addToChest(item, chestSlots);
	}

	public void updateOpenedChestSlot(int slotNumber, InventorySlot item)
	{
		if (!isOpen())
		{
			Debug.LogError("tried to update a chest that isnt open");
			return;
		}
		if (openedChest == null)
		{
			Debug.LogError("tried to update a chest but openedChest is null");
			return;
		}
		openedChest.inventorySlots[slotNumber] = item;
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
	/**
	 * tries to add the item to the chest if there is space.
	 * returns an int, amount of the item that is left (that didnt fit in the chest)
	 */
	public int addToChest(InventorySlot item, ChestSlotScript[] csScripts)
	{
		bool isStackable = !item.isTool() && !item.isArmor() && !BlockHashtable.isNotStackable(item.itemName);
		// the first for loop checks if the same item is in the chest, then add the items into that stack (if the item is stackable)
		for(int i = 0; i < inventorySlots.Length; i++)
		{
			if (inventorySlots[i].isEmpty() && !isStackable)
			{
				inventorySlots[i] = item; // add item to chest
				csScripts[i].updateSlot(inventorySlots[i]);
				return 0;
			}
			if (inventorySlots[i].itemName.Equals(item.itemName) && isStackable)
			{
				while(item.amount > 0 && inventorySlots[i].amount < 64)
				{
					inventorySlots[i].incrementAmount();
					item.decrementAmount();
				}
				csScripts[i].updateSlot(inventorySlots[i]);
				if (item.amount <= 0) return 0;
			}
		}
		// the second for loop adds the item anywhere in an empty slot
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (inventorySlots[i].isEmpty())
			{
				inventorySlots[i] = item;
				csScripts[i].updateSlot(inventorySlots[i]);
				return 0;
			}
		}

		return item.amount;
	}
}