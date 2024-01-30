using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OpenFurnaceScript : MonoBehaviour
{
	private GameObject furnaceMenu;
	private Transform furnaceMenuPanel;
	private Transform inventoryPanel;
	private GameObject darkBackground;
	private GameObject inventorySlots;
	private Camera cam;

	private FurnaceSlotScript furnaceBottomSlot;
	private FurnaceSlotScript furnaceTopSlot;
	private FurnaceResultSlot furnaceResultSlot;
	private FurnaceLogic openedFurnace; // this is the furnace that is currently open

	private Image fireImage;
	private Image arrowImage;

	private List<FurnaceLogic> furnaces = new List<FurnaceLogic>();

	private static IDataService dataService = new JsonDataService();

	// Start is called before the first frame update
	void Start()
	{
		furnaceMenu = transform.Find("FurnaceMenu").gameObject;
		darkBackground = transform.Find("DarkBackground").gameObject;
		furnaceMenu.SetActive(false);

		inventoryPanel = transform.Find("Inventory").Find("InventoryPanel");
		furnaceMenuPanel = furnaceMenu.transform.Find("FurnaceMenuPanel");
		inventorySlots = inventoryPanel.Find("InventorySlots").gameObject;
		
		fireImage = furnaceMenuPanel.Find("FurnaceImages").Find("FireImage").GetComponent<Image>();
		arrowImage = furnaceMenuPanel.Find("FurnaceImages").Find("ArrowImage").GetComponent<Image>();

		cam = Camera.main;

		furnaceBottomSlot = furnaceMenuPanel.Find("FurnaceSlots").Find("FurnaceSlotBottom").GetComponent<FurnaceSlotScript>();
		furnaceTopSlot = furnaceMenuPanel.Find("FurnaceSlots").Find("FurnaceSlotTop").GetComponent<FurnaceSlotScript>();
		furnaceResultSlot = furnaceMenuPanel.Find("FurnaceSlots").Find("ResultFurnaceSlot").GetComponent<FurnaceResultSlot>();

		// load furnace data if it exists
		if (dataService.exists("furnaces.json"))
		{
			object[] furnaceData = dataService.loadData<object[]>("furnaces.json");
			
			foreach (JArray furnace in furnaceData)
			{
				FurnaceLogic newFurance = new FurnaceLogic(furnace);
				furnaces.Add(newFurance);
				// start coroutines
				if(newFurance.getFireProgress() > 0)
				{
					Debug.Log("STARTING FIRE COROUTINE!");
					StartCoroutine(newFurance.cookCoroutine());
					if (newFurance.getIsBurnableInTopSlot())
					{
						Debug.Log("STARTING ARROW COROUTINE!");
						StartCoroutine(newFurance.arrowCoroutine());
					}
				}
				
			}
			
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (furnaceMenu.activeSelf) // if furnace is open
		{
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			{
				// close furnace
				furnaceMenu.SetActive(false);
				darkBackground.SetActive(false);
				inventorySlots.transform.SetParent(inventoryPanel); // put inventory slots back to inventory
				StartCoroutine(setIsInUIToFalse());
				openedFurnace = null;
			}

		}
	}

	public void openFurnaceMenu()
	{
		if (InventoryScript.getIsInUI()) return; // if already in some UI, then dont open the furnace
		furnaceMenu.SetActive(true);
		darkBackground.SetActive(true);
		InventoryScript.setIsInUI(true);

		inventorySlots.transform.SetParent(furnaceMenuPanel); // put inventory slots in the furnace

		Vector2 mousePos = getRoundedMousePos();
		foreach(FurnaceLogic furnace in furnaces)
		{
			if(furnace.getFurnacePosition() == mousePos)
			{
				openedFurnace = furnace;
				break;
			}
		}
		if (openedFurnace == null) // if we didnt find a furnace script, then create a script for the furnace
		{
			openedFurnace = new FurnaceLogic(mousePos);
			furnaces.Add(openedFurnace);
		}
		initializeFurnaceMenu(openedFurnace);

	}

	/**
	 * this runs when we open the furnace
	 * it gets the items and progress from the FurnaceLogic and puts it on the furnace UI
	 */
	private void initializeFurnaceMenu(FurnaceLogic furnace)
	{
		furnaceBottomSlot.setItemInSlot(furnace.getFurnaceBottomSlot());
		furnaceTopSlot.setItemInSlot(furnace.getFurnaceTopSlot());
		furnaceResultSlot.setItemInSlot(furnace.getFurnaceResultSlot());
		fireImage.fillAmount = furnace.getFireProgress(); // get fire animation progress
		arrowImage.fillAmount = furnace.getArrowProgress(); // get arrow animation progress
	}
	/**
	 * Gets called when a furnace is broken
	 * Vector2 furnacePos: the position of the furnace, needs to be a valid block position
	 */
	public void removeFurnace(Vector2 furnacePos)
	{
		foreach (FurnaceLogic furnace in furnaces)
		{
			if (furnace.getFurnacePosition() == furnacePos)
			{
				furnaces.Remove(furnace);
				// drop the items that were in the furnace
				InventorySlot topSlot = furnace.getFurnaceTopSlot();
				InventorySlot bottomSlot = furnace.getFurnaceBottomSlot();
				InventorySlot resultSlot = furnace.getFurnaceResultSlot();

				GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
				while (!topSlot.isEmpty())
				{
					itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + topSlot.itemName); // change item texture to the corresponding item
					Instantiate(itemToDrop, furnacePos, Quaternion.identity); // spawn item
					topSlot.decrementAmount();
				}
				while (!bottomSlot.isEmpty())
				{
					itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + bottomSlot.itemName); // change item texture to the corresponding item
					Instantiate(itemToDrop, furnacePos, Quaternion.identity); // spawn item
					bottomSlot.decrementAmount();
				}
				while (!resultSlot.isEmpty())
				{
					itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + resultSlot.itemName); // change item texture to the corresponding item
					Instantiate(itemToDrop, furnacePos, Quaternion.identity); // spawn item
					resultSlot.decrementAmount();
				}

				return;
			}
		}
	}

	public void updateFurnaceSlotsVisually(FurnaceLogic furnace, InventorySlot bottomSlot, InventorySlot topSlot, InventorySlot resultSlot)
	{
		if (!ReferenceEquals(openedFurnace, furnace)) return; // if the opened furnace is not the furnace that called this function, then we dont need to update the slots visually

		furnaceBottomSlot.setItemInSlot(bottomSlot, false);
		furnaceTopSlot.setItemInSlot(topSlot, false);
		furnaceResultSlot.setItemInSlot(resultSlot);
	}


	/**
	 * This function gets called from FuranceSlotScript
	 * updates the bottom or top furnce slot in FurnaceLogic for the current furnace that is opened
	 */
	public void updateFurnaceSlot(InventorySlot newItem, bool isBottomSlot)
	{
		bool isCooking = false;
		if (isBottomSlot)
		{
			isCooking = openedFurnace.putItemInBottomSlot(newItem);
		}
		else
		{
			isCooking = openedFurnace.putItemInTopSlot(newItem);
		}
		if(isCooking) // if isCooking then we need to start a corouting which slowly makes the fire shrink and arrow expand
		{
			startCooking();
		}
	}

	private void startCooking()
	{
		StartCoroutine(openedFurnace.cookCoroutine());
		StartCoroutine(openedFurnace.arrowCoroutine());
	}

	public void updateResultSlot(InventorySlot resultSlot)
	{
		if (openedFurnace.putItemInResultSlot(resultSlot))
		{
			startCooking();
		}
	}

	public void updateFireAnimation(FurnaceLogic furnace, float progress)
	{
		if (!ReferenceEquals(openedFurnace, furnace)) return; // if the opened furnace is not the furnace that called this function, then we dont need to update the fire animation
		fireImage.fillAmount = progress;
	}

	public void updateArrowAnimation(FurnaceLogic furnace, float progress)
	{
		if (!ReferenceEquals(openedFurnace, furnace)) return; // if the opened furnace is not the furnace that called this function, then we dont want to update the arrow animation
		arrowImage.fillAmount = progress;
	}
	/**
	 * saves the information for the furnaces, so you can quit the game with the furnaces saved.
	 * the things we need to save are:
	 * * topSlot, bottomSlot and resultSlot,
	 * * arrowProgress and fireProgress,
	 * * world position
	 */
	public void saveFurnaces()
	{
		object[] furnaceData = new object[furnaces.Count]; // list of furnaces
		for (int i = 0; i < furnaceData.Length; i++)
		{
			furnaceData[i] = furnaces[i].getFurnaceData();
		}

		if (!dataService.saveData("furnaces.json", furnaceData)) // save furnaces
		{
			Debug.LogError("Could not save furnaces data file :(");
		}
	}

	private Vector2 getRoundedMousePos()
	{
		Vector3 mousePos = Input.mousePosition;
		Vector3 worldMousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
		Vector2 RoundedMousePos = new Vector2((float)Math.Round(worldMousePos.x + 0.5f) - 0.5f, (float)Math.Round(worldMousePos.y + 0.5f) - 0.5f); // round it to the closes possible "block position"
		return RoundedMousePos;
	}


	// to fix a bug where setting ui to false is faster than checking if the player is opening the inventory
	private IEnumerator setIsInUIToFalse()
	{
		yield return new WaitForSeconds(.05f);
		InventoryScript.setIsInUI(false);
	}
}
