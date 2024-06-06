using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * This class is responsible for:
 * showing the items that are on the hotbar
 * changing which slot is selected on the hotbar
 */
public class HotbarScript : MonoBehaviour
{

	private PlayerInventory playerInventory;
	private int selectedSlot = 0;
	private RectTransform selectedSlotTransform;

	private Image[] hotbarSlotImages = new Image[9];
	private TextMeshProUGUI[] hotbarSlotAmount = new TextMeshProUGUI[9];
	private Hashtable durabilityBars = new Hashtable(); // (slotNumber, DurabilityBar)

	// Start is called before the first frame update
	void Start()
    {
        playerInventory = GameObject.Find("SteveContainer").transform.Find("Steve").GetComponent<PlayerInventory>();
		selectedSlotTransform = transform.Find("SelectedSlot").gameObject.GetComponent<RectTransform>();

		for (int i = 0; i < hotbarSlotImages.Length; i++)
		{
			hotbarSlotImages[i] = transform.Find("HotbarSlot" + i).GetComponent<Image>();
		}

		for (int i = 0; i < hotbarSlotAmount.Length; i++)
		{
			hotbarSlotAmount[i] = transform.Find("HotbarSlot" + i + "Amount").GetComponent<TextMeshProUGUI>();
		}
    }

    // Update is called once per frame
    void Update()
    {
		checkIfSelectedSlotChange();
	}

	public void updateHotbarSlot(DurabilityItem tool, int slot) // slot from 0 to 8 (inclusive)
	{
		if (playerInventory == null) Start();

		InventorySlot itemAndAmount = InventoryScript.getItemsInSlot(slot);
		string itemName = itemAndAmount.itemName;
		int amount = itemAndAmount.amount;

		if (!itemName.Equals("")) // if this slot has an item
		{
			Sprite itemImage = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemName);

			hotbarSlotImages[slot].sprite = itemImage;
			hotbarSlotImages[slot].color = new Color(1f, 1f, 1f, 1f); // remove transparency
		}
		else // if there isn't any item in this slot
		{
			hotbarSlotImages[slot].sprite = null; // remove image
			hotbarSlotImages[slot].color = new Color(1f, 1f, 1f, 0f); // add transparency


		}

		if (amount <= 1)
		{
			hotbarSlotAmount[slot].SetText("");
		}
		else
		{
			hotbarSlotAmount[slot].SetText(amount.ToString());
		}

		if (selectedSlot == slot) // if player is holding the item that is being updated
		{
			playerInventory.holdItem(itemName); // hold the item
		}

		// if there is a tool in this slot then add the durability bar to the slot, but only display it if durability < STARTING_DURABILITY
		if (tool != null)
		{
			GameObject durabilityBar;

			if (durabilityBars[slot] == null) // if this slot doesnt have a durability bar
			{
				// create a durability bar
				durabilityBar = Resources.Load<GameObject>("Prefabs\\DurabilityBarBackground");

				GameObject instantiatedBar = Instantiate(durabilityBar);
				instantiatedBar.transform.SetParent(hotbarSlotImages[slot].transform);
				instantiatedBar.GetComponent<RectTransform>().sizeDelta = new Vector2(41, 3.1f);
				instantiatedBar.transform.localPosition = new Vector2(0, -43);
			}
			// edit the durability bar to show how much durability is left
			durabilityBar = hotbarSlotImages[slot].transform.Find("DurabilityBarBackground(Clone)").gameObject;
			DurabilityBar durabilityBarScript = durabilityBar.GetComponent<DurabilityBar>();
			durabilityBarScript.setMaximumDurability(tool);
			durabilityBarScript.updateDurability(tool);

			durabilityBars[slot] = durabilityBarScript; // add the script to the hashtable

			if (tool.getDurability() >= tool.getStartingDurability()) // only display durability bar if durability < STARTING_DURABILITY
			{
				durabilityBar.SetActive(false);
			}
			else
			{
				durabilityBar.SetActive(true);
			}
		}
		else if (durabilityBars[slot] != null) // if this slot has a durability bar, but there isnt a tool in this slot
		{
			// delete durability bar since there isnt any tool in this slot
			durabilityBars.Remove(slot);

			Destroy(hotbarSlotImages[slot].transform.Find("DurabilityBarBackground(Clone)").gameObject);
		}
	}

	public void updateDurabilityBar(DurabilityItem tool, int slot)
	{
		if (durabilityBars[slot] == null) return;

		DurabilityBar durabilityBarScript = durabilityBars[slot] as DurabilityBar;

		durabilityBarScript.gameObject.SetActive(true);

		durabilityBarScript.updateDurability(tool);

	}

	public int getSelectedSlot()
	{
		return selectedSlot;
	}

	private void changeSelectedSlot(int slot)
	{
		// hold the selected item
		playerInventory.holdItem(InventoryScript.getItemBySlot(slot));

		int selectedSlotXPosition = -80 + (slot * 20);
		// put the "selected" texture over the slot
		selectedSlotTransform.localPosition = new Vector2(selectedSlotXPosition, selectedSlotTransform.localPosition.y);

		selectedSlot = slot;
	}

	private void checkIfSelectedSlotChange()
    {
		if (InventoryScript.getIsInUI()) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
			changeSelectedSlot(0);

		}
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
			changeSelectedSlot(1);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			changeSelectedSlot(2);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			changeSelectedSlot(3);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			changeSelectedSlot(4);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			changeSelectedSlot(5);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			changeSelectedSlot(6);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			changeSelectedSlot(7);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			changeSelectedSlot(8);
		}
		else if (!Input.GetKey(KeyCode.X)) // if X is not held down
		{
			int change = -(int)Input.mouseScrollDelta.y;
			if (change != 0)
			{
				int newSlot = selectedSlot + change;
				changeSelectedSlot(((newSlot % 9) + 9) % 9);
			}

		}
	}
}
