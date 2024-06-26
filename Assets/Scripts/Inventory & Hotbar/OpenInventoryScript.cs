using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * this class is responsible for:
 * opening/closing the inventory when user presses e
 * displaying the item that is being held byt the mouse in the inventory
 */
public class OpenInventoryScript : MonoBehaviour, IPointerClickHandler
{

    private GameObject inventory;
    private GameObject darkBackground;
    private GameObject heldItemObject;
    private bool isItemBeingHeld = false;
    private InventorySlot heldItem = new InventorySlot();
    private Slot hoveringOverSlotScript = null; // the script for the slot that the mouse is hovering over

    // Start is called before the first frame update
    void Start()
    {
        inventory = transform.Find("Inventory").gameObject;
        darkBackground = transform.Find("DarkBackground").gameObject;
        inventory.SetActive(false);
        darkBackground.SetActive(false);
        

		heldItemObject = transform.Find("HeldItem").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || (Input.GetKeyDown(KeyCode.Escape) && InventoryScript.getIsInUI())) // open/close inventory
        {
            if (!(!inventory.activeSelf && InventoryScript.getIsInUI())) // if player is not (trying to open inventory and there is already a UI open)
            {
                bool isInInventory = !inventory.activeSelf;
                inventory.SetActive(isInInventory);
                darkBackground.SetActive(isInInventory);
                InventoryScript.setIsInUI(isInInventory);

                Craft.deleteItemFromResultSlot(true); // remove items from result slot, if any
				Craft.dropItemsFromSlots(true); // drop items from crafting slots, if any
            }

            if (isItemBeingHeld) // if we are holding an item and we closed the inventory, then we want to drop the item.
            {
                dropItems(heldItem);
            }

            if(hoveringOverSlotScript != null) hoveringOverSlotScript.OnPointerExit(null); // remove the hovering over texture on the last hovered over slot
        }

        if (isItemBeingHeld)
        {
            displayHeldItem();
        }
    }

    public void setHoveringOverSlotScript(Slot script)
    {
        hoveringOverSlotScript = script;
    }

    public void setIsItemBeingHeld(bool value)
    {
        isItemBeingHeld = value;
		heldItemObject.SetActive(value);
        InventoryScript.setHasItemsPickedUp(value);

        if (value) // if item is being held, we need to display that item
        {
            heldItem = InventoryScript.getItemsPickedUp();
            heldItemObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\" + heldItem.itemName); // set the image to the corresponding texture
            if (heldItem.amount > 1)heldItemObject.transform.Find("Amount").GetComponent<TextMeshProUGUI>().SetText(heldItem.amount.ToString()); // set the amount text
            else heldItemObject.transform.Find("Amount").GetComponent<TextMeshProUGUI>().SetText("");
		}
    }


    private void displayHeldItem()
    {
		Vector3 mousePos = Input.mousePosition;

        heldItemObject.transform.position = mousePos;
	}

    /**
     * drops the items that are being held by the cursor
     */
    public void dropItems(InventorySlot items)
    {
        string itemToDrop = items.itemName;
        int amountToDrop = items.amount;

		GameObject itemContainer = Resources.Load<GameObject>("Prefabs\\ItemContainer"); // get itemContainer

		Sprite itemImage = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemToDrop); // get the image for the item
		itemContainer.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = itemImage; // put the image on the SpriteRenderer
        GameObject spawnedItem;

        // we need to get steve's position in order to drop the items from his position
        Transform steve = GameObject.Find("SteveContainer").transform.Find("Steve");

        for(int i = 0; i < amountToDrop; i++)
        {
			spawnedItem = Instantiate(itemContainer, new Vector2(steve.position.x, steve.position.y + .7f), itemContainer.transform.rotation); // spawn the item
			DroppedItemScript itemScript = spawnedItem.GetComponent<DroppedItemScript>();
			itemScript.setPickupable(false); // cant pickup the item immediately
			itemScript.addDropVelocity(steve.position); // shoot the item from the player
			itemScript.tool = items.toolInstance; // add the tool info
            itemScript.armor = items.armorInstance;
		}

        if(items.Equals(heldItem)) setIsItemBeingHeld(false);
	}

    /**
     * runs when the user clicks on something in the inventory
     * 
     * this is used to check if the player clicked outside of the inventory, then if the cursor is holding items, then drop the items
     */
	public void OnPointerClick(PointerEventData eventData)
	{
        if (!eventData.pointerCurrentRaycast.gameObject.name.Equals("DarkBackground")) return; // return if the gameobject that was clicked is not "DarkBackground"

		if (eventData.button == PointerEventData.InputButton.Left) // drop all held items
		{
            if(isItemBeingHeld) dropItems(heldItem);
		}
		else if (eventData.button == PointerEventData.InputButton.Right) // drop one of the held items
		{
            print("drop one"); // TODO
		}
	}
}
