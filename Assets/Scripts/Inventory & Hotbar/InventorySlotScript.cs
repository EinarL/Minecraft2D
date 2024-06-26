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
public class InventorySlotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Slot
{

    private GameObject itemImage;
    private Image hoverTexture;
    private TextMeshProUGUI amountText;
	private int slotNumber;
    private OpenInventoryScript openInventoryScript;
    private InventorySlot itemInSlot = new InventorySlot();
    private DurabilityBar durabilityBarScript;
    private bool hasRightClicked = false; // has the cursor right clicked to place an item in this slot

    // Start is called before the first frame update
    void Start()
    {
        itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
		amountText = itemImage.transform.Find("Amount").GetComponent<TextMeshProUGUI>();
		string gameObjectName = gameObject.name;
		gameObjectName = gameObjectName.Replace("InventorySlot", "").Replace("ChestSlot", "").Trim(); // remove InventorySlot from name
        slotNumber = int.Parse(gameObjectName);

        openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();
    }

	/**
     * updates what is in the slot 
     * 
     * DurabilityItem tool: the tool/armor that is in this slot. this is null if there is no tool nor armor in the slot.
     */
	public void updateSlot(DurabilityItem toolOrArmor)
    {
        if(itemImage == null) Start();

        InventorySlot itemsToPutInSlot = InventoryScript.getItemsInSlot(slotNumber);
        itemInSlot = new InventorySlot(itemsToPutInSlot.itemName, itemsToPutInSlot.toolInstance, itemsToPutInSlot.armorInstance, itemsToPutInSlot.amount);
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

			InventoryScript.removeFromInventory(slotNumber, itemInSlot.amount); // remove items from inventory
            
            InventoryScript.setItemsPickedUp(slotItems); // set the picked up items to be the items that were in this slot
            openInventoryScript.setIsItemBeingHeld(true);

		}
        else if(hasPickedUp) // if the player has picked up something
		{
            // put the picked up items in this slot
            bool isStillHoldingItems = InventoryScript.addPickedUpItemsToSlot(slotNumber, true);
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
			int amount = int.Parse(amountText.text);
            string itemInSlot = "";
			if(itemImage.activeSelf) itemInSlot = itemImage.GetComponent<Image>().sprite.name; // "itemImage.activeSelf" basically means: is there an item in this slot
			if (itemInSlot.Equals("") || (itemInSlot.Equals(InventoryScript.getItemsPickedUp().itemName) && amount < 64 && !BlockHashtable.isNotStackable(InventoryScript.getItemsPickedUp().itemName))) // if the slot is empty or has the same item with amount < 64 (and the item is stackable)
			{
                // then we can place one of the held items in this slot
                bool isStillHoldingItems = InventoryScript.addPickedUpItemsToSlot(slotNumber, false);
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
