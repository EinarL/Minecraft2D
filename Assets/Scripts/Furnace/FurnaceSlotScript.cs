using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


/**
 * this class is responsible for:
 * * hover texture on the inventory slot
 * * picking up an item from the slot
 * * putting an item in the slot
 */
public class FurnaceSlotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Slot
{

	private GameObject itemImage;
	private Image hoverTexture;
	private TextMeshProUGUI amountText;
	private OpenInventoryScript openInventoryScript;
	private OpenFurnaceScript openFurnaceScript;
	private InventorySlot itemInSlot = new InventorySlot();
	private DurabilityBar durabilityBarScript;
	private bool hasRightClicked = false; // has the cursor right clicked to place an item in this slot
	private bool isBottomSlot = false;

	// Start is called before the first frame update
	void Start()
	{
		itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
		amountText = itemImage.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

		openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();
		openFurnaceScript = openInventoryScript.GetComponent<OpenFurnaceScript>();

		if (gameObject.name.Equals("FurnaceSlotBottom")) isBottomSlot = true;
	}

	// Update is called once per frame
	void Update()
	{

	}

	/**
     * updates what is in the slot 
     * 
     * ToolInstance tool: the tool that is in this slot. this is null if there is no tool in the slot.
     */
	public void updateSlot(bool updateFurnaceLogic = true)
	{
		if (itemImage == null) Start();

		Sprite image = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemInSlot.itemName);

		if (image != null) // found item to display
		{
			itemImage.GetComponent<Image>().sprite = image;

			itemImage.SetActive(true);

			amountText.SetText(itemInSlot.amount.ToString());
			if (itemInSlot.amount <= 1) amountText.gameObject.SetActive(false);
			else amountText.gameObject.SetActive(true);
		}
		else
		{
			itemImage.SetActive(false);
		}

		// if there is a tool in this slot then add the durability bar, but only display it if durability < STARTING_DURABILITY
		if (itemInSlot.toolInstance != null)
		{
			GameObject durabilityBar;

			if (!hasDurabilityBar())
			{
				// create a durability bar
				durabilityBar = Resources.Load<GameObject>("Prefabs\\DurabilityBarBackground");

				GameObject instantiatedBar = Instantiate(durabilityBar);
				instantiatedBar.transform.SetParent(gameObject.transform);
				instantiatedBar.GetComponent<RectTransform>().sizeDelta = new Vector2(41, 3.1f);
				instantiatedBar.transform.localPosition = new Vector2(0, -48);
			}
			// edit the durability bar to show how much durability is left
			durabilityBar = transform.Find("DurabilityBarBackground(Clone)").gameObject;
			durabilityBarScript = durabilityBar.GetComponent<DurabilityBar>();
			durabilityBarScript.setMaximumDurability(itemInSlot.toolInstance);
			durabilityBarScript.updateDurability(itemInSlot.toolInstance);

			if (itemInSlot.toolInstance.getDurability() >= itemInSlot.toolInstance.getStartingDurability()) // only display durability bar if durability < STARTING_DURABILITY
			{
				durabilityBar.SetActive(false);
			}
			else
			{
				durabilityBar.SetActive(true);
			}

		}
		else if (hasDurabilityBar())
		{
			// delete durability bar since there isnt any tool in this slot
			durabilityBarScript = null;

			Destroy(transform.Find("DurabilityBarBackground(Clone)").gameObject);
		}

		// update the FurnaceLogic script for the currently opened furnace
		if (updateFurnaceLogic) openFurnaceScript.updateFurnaceSlot(itemInSlot, isBottomSlot);
	}

	/**
    * runs when player left clicks the item slot.
    * 
    * if the player hasnt picked up anything:
    *   picks up the items in this slot
    * if the player has picked up something:
    *   puts the items he has picked up in this slot (and)
    *   picks up the items in this slot, if any
    */
	public void leftClickSlot()
	{
		bool hasPickedUp = InventoryScript.getHasItemsPickedUp();

		if (!hasPickedUp && itemImage.activeSelf) // if the player hasnt picked up anything && there is an item in this slot
		{
			// pickup the items in this slot

			InventorySlot slotItems = new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.amount);

			itemInSlot.removeFromSlot(itemInSlot.amount); // remove items from the slot

			InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
			openInventoryScript.setIsItemBeingHeld(true);
			updateSlot();

		}
		else if (hasPickedUp) // if the player has picked up something
		{
			// put the picked up items in this slot

			InventorySlot heldItems = InventoryScript.getItemsPickedUp();
			bool isStillHoldingItems = addPickedUpItemsToSlot(heldItems, true);
			openInventoryScript.setIsItemBeingHeld(isStillHoldingItems);
		}

	}

	/**
     * runs when player right clicks the item slot
     * 
     * if the player hasnt picked up anything:
     *   picks up half of all items
     * if the player has picked up something:
     *   puts one of the item its holding into the slot if the slot is empty or has the same item with amount < 64
     * 
     */
	public void rightClickSlot()
	{
		bool hasPickedUp = InventoryScript.getHasItemsPickedUp();

		if (hasPickedUp)
		{
			int amount = itemInSlot.amount;
			InventorySlot heldItems = InventoryScript.getItemsPickedUp();
			if (itemInSlot.isEmpty() || (itemInSlot.itemName.Equals(heldItems.itemName) && amount < 64 && !itemInSlot.isTool())) // if the slot is empty or has the same item with amount < 64
			{
				// then we can place one of the held items in this slot
				bool isStillHoldingItems = addPickedUpItemsToSlot(heldItems, false);
				openInventoryScript.setIsItemBeingHeld(isStillHoldingItems);
			}
		}

	}

	/**
	 * adds the items in "itemsPickedUp" to the slot
	 * if addAll is false, then it only adds one of the items it is holding
	 * 
	 * returns: true if it is still holding items, but false if it got rid of all the items in itemsPickedUp
	 */
	
	private bool addPickedUpItemsToSlot(InventorySlot heldItems, bool addAll)
	{
		// TODO: if held items is armor, then return

		if (heldItems.isTool())
		{
			if (!itemInSlot.isEmpty()) // if the slot is not empty
			{
				switchHeldItemsAndItemInSlot(heldItems); // switch the items being held and the items in the slot
				return true;
			}
			else // otherwise just put the held tool in the slot
			{
				itemInSlot.putItemOrToolInSlot(heldItems.itemName, heldItems.toolInstance, heldItems.amount);
				updateSlot();
			}

			return false;
		}

		if (addAll)
		{
			if (heldItems.itemName.Equals(itemInSlot.itemName)) // if we're putting the same item type that already is in this slot
			{

				int leftOver = itemInSlot.addItemsToSlot(heldItems.itemName, heldItems.amount);
				heldItems.amount = leftOver;
				updateSlot();
				if (leftOver > 0) return true;
				return false;
			}
			if (!itemInSlot.isEmpty()) // if there is already a tool or an item in this slot
			{
				// here we switch the items that are being held and the items that are in the slot
				switchHeldItemsAndItemInSlot(heldItems);
				return true;
			}
			else // if the slot is empty
			{
				itemInSlot.putItemInSlot(heldItems.itemName);
				itemInSlot.setAmount(heldItems.amount);
				updateSlot();
				return false;
			}
		}
		else // only add one
		{
			if (itemInSlot.putItemInSlot(heldItems.itemName)) // if it did add the item to the slot
			{
				heldItems.amount--;
				updateSlot();
				if (heldItems.amount > 0) return true; // return true if still holding items
			}

		}

		return false;
	}
	
	/**
	 * runs when the player has items held by the cursor and he left-clicks on a non-empty slot, then we switch the items
	 */
	private void switchHeldItemsAndItemInSlot(InventorySlot heldItems)
	{
		string itemNameInSlot = itemInSlot.itemName;
		ToolInstance toolInSlot = itemInSlot.toolInstance;
		int amountInSlot = itemInSlot.amount;
		itemInSlot.putItemOrToolInSlot(heldItems.itemName, heldItems.toolInstance, heldItems.amount);
		heldItems = new InventorySlot(itemNameInSlot, toolInSlot, amountInSlot);
		updateSlot();
	}

	public void setItemInSlot(InventorySlot newItem, bool updateFurnaceLogic = true)
	{
		itemInSlot = newItem;
		updateSlot(updateFurnaceLogic);
	}

	/**
     * this function gets called when the cursor enters this slot
     * it checks if the mouse right click button is being held, if so,
     * then add an item from held items to this slot, if this slot is empty or has the same item as the held item
     * and there is an item being held
     */
	private void checkIfHoldingRightClick()
	{
		if (Input.GetMouseButton(1)) // if right click is held down
		{
			rightClickSlot();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hoverTexture.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // remove transparency
		openInventoryScript.setHoveringOverSlotScript(this);
		checkIfHoldingRightClick();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hoverTexture.color = new Color(0.7f, 0.7f, 0.7f, 0f); // make hoverTexture invisible
	}

	public void OnPointerClick(PointerEventData eventData)
	{

		if (eventData.button == PointerEventData.InputButton.Left)
		{
			leftClickSlot();
		}

	}

	public void OnPointerDown(PointerEventData eventData)
	{

		if (eventData.button == PointerEventData.InputButton.Right && !hasRightClicked)
		{
			rightClickSlot();
			hasRightClicked = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			hasRightClicked = false;
		}
	}

	// does this inventory slot have a durability bar for a tool
	private bool hasDurabilityBar()
	{
		return durabilityBarScript != null;
	}


}
