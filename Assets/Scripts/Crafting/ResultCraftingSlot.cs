using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class ResultCraftingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, Slot
{

	private GameObject itemImage;
	private Image hoverTexture;
	private TextMeshProUGUI amountText;
	private OpenInventoryScript openInventoryScript;
	public InventorySlot itemInSlot;
	public bool isInventoryResultSlot = false; // if false, then it is a result slot in the crafting table, otherwise inventory crafting result slot

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

	public void updateSlot()
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


	}

	public void setItemInSlot(InventorySlot itemInSlot)
	{
		if(itemInSlot.isTool()) this.itemInSlot = new InventorySlot(itemInSlot.itemName, new ToolInstance(Resources.Load<Tool>("ToolScriptables\\" + itemInSlot.itemName)), itemInSlot.amount); // need to do this to fix a bug
		else if(itemInSlot.isArmor()) this.itemInSlot = new InventorySlot(new ArmorInstance(Resources.Load<Armor>("ToolScriptables\\Armor\\" + itemInSlot.itemName)), itemInSlot.itemName);
		else this.itemInSlot = new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.amount);
		updateSlot();
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
		// if holding shift, put the item in this slot in the inventory, if there is space TODO


		if (!hasPickedUp) // if the player hasnt picked up anything
		{
			// pickup the items in this slot
			int amount = itemInSlot.amount;
			string itemToPickup = itemInSlot.itemName;
			ToolInstance tool = itemInSlot.toolInstance;
			ArmorInstance armor = itemInSlot.armorInstance;

			InventorySlot slotItems = new InventorySlot(itemToPickup, tool, armor, amount);
			InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
			openInventoryScript.setIsItemBeingHeld(true);

			itemInSlot.removeEverythingFromSlot();
			Craft.deleteItemsFromSlots(isInventoryResultSlot); // delete the items that were used for crafting
			updateSlot();

		}
		else // if the player has picked up something
		{
			if (itemInSlot.isTool() || itemInSlot.isArmor()) return;
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
				Craft.deleteItemsFromSlots(isInventoryResultSlot); // delete the items that were used for crafting
				updateSlot();
			}

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

