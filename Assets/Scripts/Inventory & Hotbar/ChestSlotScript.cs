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
public class ChestSlotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Slot
{

    private GameObject itemImage;
    private Image hoverTexture;
    private TextMeshProUGUI amountText;
	private int slotNumber;
    private OpenInventoryScript openInventoryScript;
	private OpenChestScript openChestScript;
    public InventorySlot itemInSlot { get; private set; } = new InventorySlot();
    private DurabilityBar durabilityBarScript;
    private bool hasRightClicked = false; // has the cursor right clicked to place an item in this slot

    // Start is called before the first frame update
    void Start()
    {
        itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
		amountText = itemImage.transform.Find("Amount").GetComponent<TextMeshProUGUI>();
		string gameObjectName = gameObject.name;
		gameObjectName = gameObjectName.Replace("ChestSlot", "").Trim(); // remove ChestSlot from name
        slotNumber = int.Parse(gameObjectName);

        openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();
		openChestScript = openInventoryScript.GetComponent<OpenChestScript>();
    }

	/**
     * updates what is in the slot 
     */
	public void updateSlot(InventorySlot newItem = null, bool updateChestScript = true)
    {
        if(itemImage == null) Start();

        if(newItem != null) itemInSlot = new InventorySlot(newItem.itemName, newItem.toolInstance, newItem.armorInstance, newItem.amount);
		Sprite image = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemInSlot.itemName);

        if(image != null) // found item to display
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
        DurabilityItem toolOrArmor = itemInSlot.toolInstance == null ? itemInSlot.armorInstance : itemInSlot.toolInstance;
        // if there is a tool in this slot then add the durability bar, but only display it if durability < STARTING_DURABILITY
        if(toolOrArmor != null)
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
            durabilityBarScript.setMaximumDurability(toolOrArmor);
            durabilityBarScript.updateDurability(toolOrArmor);

            if (toolOrArmor.getDurability() >= toolOrArmor.getStartingDurability()) // only display durability bar if durability < STARTING_DURABILITY
            {
                durabilityBar.SetActive(false);
            }
            else
            {
				durabilityBar.SetActive(true);
            }

		}
        else if(hasDurabilityBar())
        {
            // delete durability bar since there isnt any tool in this slot
            durabilityBarScript = null;

            Destroy(transform.Find("DurabilityBarBackground(Clone)").gameObject);
        }
		if(updateChestScript) openChestScript.updateOpenedChestSlot(slotNumber, itemInSlot.copy());
	}

    public void updateDurabilityBar(ToolInstance tool)
    {
        if (!hasDurabilityBar()) return;
		durabilityBarScript.gameObject.SetActive(true);

		durabilityBarScript.updateDurability(tool);
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

        if(!hasPickedUp && itemImage.activeSelf) // if the player hasnt picked up anything && there is an item in this slot
		{
            // pickup the items in this slot
			InventorySlot slotItems = new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.armorInstance, itemInSlot.amount);
            
            InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
            openInventoryScript.setIsItemBeingHeld(true);

            itemInSlot.removeEverythingFromSlot(); // remove everything from this slot
			updateSlot(); 

		}
        else if(hasPickedUp) // if the player has picked up something
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
		if (itemsPickedUp.isEmpty())
		{
			Debug.LogError("addPickedUpItemsToSlot() in ChestSlotScript was called, but there arent any items that are picked up.");
			return true;
		}

		if (itemsPickedUp.isTool() || itemsPickedUp.isArmor() || BlockHashtable.isNotStackable(itemsPickedUp.itemName))
		{
			if (!itemInSlot.isEmpty()) // if the slot is not empty
			{
				switchHeldItemsAndItemInSlot(itemsPickedUp); // switch the items being held and the items in the slot
				return true;
			}
			else // otherwise just put the held tool/armor in the slot
			{
				itemInSlot.putItemOrToolInSlot(itemsPickedUp.itemName, itemsPickedUp.toolInstance, itemsPickedUp.armorInstance, 1);
				updateSlot();
			}

			return false;
		}

		if (addAll)
		{
			if (itemsPickedUp.itemName.Equals(itemInSlot.itemName) && !BlockHashtable.isNotStackable(itemInSlot.itemName)) // if we're putting the same item type that already is in this slot
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
			if(itemImage.activeSelf) itemInSlot = itemImage.GetComponent<Image>().sprite.name; // "itemImage.activeSelf" basically means: is there an item in this slot
			if (itemInSlot.Equals("") || (itemInSlot.Equals(InventoryScript.getItemsPickedUp().itemName) && amount < 64 && !BlockHashtable.isNotStackable(InventoryScript.getItemsPickedUp().itemName))) // if the slot is empty or has the same item with amount < 64 (and the item is stackable)
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
        
        if(eventData.button == PointerEventData.InputButton.Left)
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
