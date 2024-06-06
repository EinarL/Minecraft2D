using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingSlotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Slot
{

    public Vector2 slotPosition;
	private GameObject itemImage;
	private Image hoverTexture;
	private TextMeshProUGUI amountText;
	private OpenInventoryScript openInventoryScript;
	public InventorySlot itemInSlot; 
	public bool isInventoryCraftingSlot = false; // if false, then it is a slot in the crafting table, otherwise inventory crafting slot
	private bool hasRightClicked = false; // has the cursor right clicked to place an item in this slot

	// Start is called before the first frame update
	void Start()
	{
		itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
		amountText = itemImage.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

		openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();

		itemInSlot = new InventorySlot();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void updateSlot(bool checkIfCanCraft = true)
	{
		string itemName = itemInSlot.itemName;
		int amount = itemInSlot.amount;

		Sprite image = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemName);

		if (image != null) // found item to display
		{
			itemImage.GetComponent<Image>().sprite = image;

			itemImage.SetActive(true);

			amountText.SetText(amount.ToString());
			if (amount <= 1) amountText.gameObject.SetActive(false);
			else amountText.gameObject.SetActive(true);
		}
		else
		{
			itemImage.SetActive(false);
		}
		if(checkIfCanCraft) Craft.checkIfCanCraft(isInventoryCraftingSlot); // check if we can craft

	}

	/**
	 * runs when the player exits the crafing menu/inventory
	 * drops the items that is in the slot, if any
	 */
	public void dropFromSlot()
	{
		if (!itemInSlot.isEmpty())
		{
			openInventoryScript.dropItems(itemInSlot); // drop items in this slot
			itemInSlot = new InventorySlot();
			updateSlot(false);
		}
	}

	/**
	 * removes one instance of the item in the slot, if there is any item in the slot
	 */
	public void removeOne()
	{
		if (!itemImage.activeSelf) return; // if there is not an item in this slot

		itemInSlot.decrementAmount();
		updateSlot();

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
			int amount = int.Parse(amountText.text);
			string itemToPickup = itemImage.GetComponent<Image>().sprite.name;

			InventorySlot slotItems = new InventorySlot(itemToPickup, amount);
			InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
			openInventoryScript.setIsItemBeingHeld(true);

			itemInSlot.removeEverythingFromSlot();
			updateSlot();

		}
		else if (hasPickedUp) // if the player has picked up something
		{
			// put the picked up items in this slot
			bool isStillHoldingItems = addPickedUpItemsToSlot(InventoryScript.getItemsPickedUp(), true);
			openInventoryScript.setIsItemBeingHeld(isStillHoldingItems);

		}

	}

	/**
	 * adds the items in "itemsPickedUp" to the slot
	 * if addAll is false, then it only adds one of the items it is holding
	 * 
	 * returns: true if it is still holding items, but false if it got rid of all the items in itemsPickedUp
	 */

	public bool addPickedUpItemsToSlot(InventorySlot itemsPickedUp, bool addAll)
	{
		if (itemsPickedUp.itemName.Equals(""))
		{
			Debug.LogError("addPickedUpItemsToSlot() in CraftingSlotScript was called, but there arent any items that are picked up.");
			return true;
		}
		bool isTool = itemsPickedUp.toolInstance != null;
		bool isArmor = itemsPickedUp.armorInstance != null;

		if (isTool || isArmor) return true; // we dont craft with tools nor armors

		if (addAll)
		{
			if (itemsPickedUp.itemName.Equals(itemInSlot.itemName)) // if we're putting the same item type that already is in this slot
			{
				int leftOver = itemInSlot.addItemsToSlot(itemsPickedUp.itemName, itemsPickedUp.amount);
				itemsPickedUp.amount = leftOver;
				updateSlot();
				if (leftOver > 0) return true;
				return false;
			}
			if (!itemInSlot.isEmpty()) // if there is already a tool or an item in this slot
			{
				// here we switch the items that are being held and the items that are in the slot
				switchHeldItemsAndItemInSlot(itemsPickedUp);
				return true;
			}
			else // if the slot is empty
			{
				itemInSlot.putItemInSlot(itemsPickedUp.itemName);
				itemInSlot.setAmount(itemsPickedUp.amount);
				updateSlot();
				return false;
			}
		}
		else // only add one
		{
			if (itemInSlot.putItemInSlot(itemsPickedUp.itemName)) // if it did add the item to the slot
			{
				itemsPickedUp.amount--;
				updateSlot();
				if (itemsPickedUp.amount > 0) return true; // return true if still holding items
			}

		}

		return false;
	}

	private void switchHeldItemsAndItemInSlot(InventorySlot itemsPickedUp)
	{
		string itemNameInSlot = itemInSlot.itemName;
		ToolInstance toolInSlot = itemInSlot.toolInstance;
		ArmorInstance armorInSlot = itemInSlot.armorInstance;
		int amountInSlot = itemInSlot.amount;
		itemInSlot.putItemOrToolInSlot(itemsPickedUp.itemName, itemsPickedUp.toolInstance, itemsPickedUp.armorInstance, itemsPickedUp.amount);
		itemsPickedUp = new InventorySlot(itemNameInSlot, toolInSlot, armorInSlot, amountInSlot);
		InventoryScript.setItemsPickedUp(itemsPickedUp);
		updateSlot();
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
			int amount = int.Parse(amountText.text);
			string itemInSlot = "";
			if (itemImage.activeSelf) itemInSlot = itemImage.GetComponent<Image>().sprite.name; // "itemImage.activeSelf" basically means: is there an item in this slot
			if (itemInSlot == "" || (itemInSlot.Equals(InventoryScript.getItemsPickedUp().itemName) && amount < 64)) // if the slot is empty or has the same item with amount < 64
			{
				// then we can place one of the held items in this slot
				bool isStillHoldingItems = addPickedUpItemsToSlot(InventoryScript.getItemsPickedUp(), false);
				openInventoryScript.setIsItemBeingHeld(isStillHoldingItems);
			}
		}

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
}
