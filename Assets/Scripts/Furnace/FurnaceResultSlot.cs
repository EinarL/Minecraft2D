using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FurnaceResultSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, Slot
{

	private GameObject itemImage;
	private Image hoverTexture;
	private TextMeshProUGUI amountText;
	private OpenInventoryScript openInventoryScript;
	public InventorySlot itemInSlot;

	private OpenFurnaceScript openFurnaceScript;

	// Start is called before the first frame update
	void Start()
	{
		itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
		amountText = itemImage.transform.Find("Amount").GetComponent<TextMeshProUGUI>();

		openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();
		openFurnaceScript = transform.parent.parent.parent.parent.GetComponent<OpenFurnaceScript>();

		itemInSlot = new InventorySlot();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void updateSlot(bool updateFurnaceLogic = true)
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

		if (updateFurnaceLogic) openFurnaceScript.updateResultSlot(itemInSlot);
	}

	public void setItemInSlot(InventorySlot itemInSlot)
	{
		if (itemInSlot.isTool()) this.itemInSlot = new InventorySlot(itemInSlot.itemName, new ToolInstance(Resources.Load<Tool>("ToolScriptables\\" + itemInSlot.itemName)), itemInSlot.amount); // need to do this to fix a bug
		else this.itemInSlot = new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.amount);
		updateSlot(false);
	}

	/**
    * runs when player left clicks the item slot.
    * 
    * if the player hasnt picked up anything:
    *   picks up the items in this slot
    * if the player has picked up something:
    *	if the player holds the same item as in the result slot, then add the items from the result slot into the held item, if there is space
    * 
    * if shift is being held, then try to put the item in this slot in the inventory, YET TO IMPLEMENT
    */
	public void leftClickSlot()
	{
		if (!itemImage.activeSelf) return; // if there is not an item in this slot

		bool hasPickedUp = InventoryScript.getHasItemsPickedUp();

		if (!hasPickedUp) // if the player hasnt picked up anything
		{
			// pickup the items in this slot
			int amount = itemInSlot.amount;
			string itemToPickup = itemInSlot.itemName;
			ToolInstance tool = itemInSlot.toolInstance;

			InventorySlot slotItems = new InventorySlot(itemToPickup, tool, amount);
			InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
			openInventoryScript.setIsItemBeingHeld(true);

			itemInSlot.removeEverythingFromSlot();
			updateSlot();

		}
		else // if the player has picked up something
		{
			int amount = itemInSlot.amount;
			string itemToPickup = itemInSlot.itemName;

			InventorySlot heldItems = InventoryScript.getItemsPickedUp();

			// if the items being held are the same as in the result slot and there is space for them to be held
			if (heldItems.itemName.Equals(itemToPickup) && heldItems.amount + amount <= 64)
			{
				InventorySlot itemsTogether = new InventorySlot(itemToPickup, heldItems.amount + amount);
				InventoryScript.setItemsPickedUp(itemsTogether);
				openInventoryScript.setIsItemBeingHeld(true);

				itemInSlot.removeEverythingFromSlot();
				updateSlot();
			}


			// if holding shift, put the item in this slot in the inventory, if there is space

		}

	}


	public void OnPointerEnter(PointerEventData eventData)
	{
		if (itemInSlot.isEmpty()) return;
		hoverTexture.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // remove transparency
		openInventoryScript.setHoveringOverSlotScript(this);
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
}

