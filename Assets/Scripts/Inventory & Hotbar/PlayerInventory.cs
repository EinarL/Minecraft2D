using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/**
 * This script is responsible for:
 * detecting items to pickup
 * dropping item
 * opening inventory
 * displaying the inventory
 * mouse clicks within the inventory
 * which item is selected in the hotbar
 */
public class PlayerInventory : MonoBehaviour
{

    private SpriteRenderer holdingItemSpriteRenderer;
    private GameObject torchLight;
    private PlaceBlockScript placeBlockScript;
	private Animator anim;
    private AudioClip pickupItemSound;

	private float pickupRange = 1; // can pick up items from within this range


    // Start is called before the first frame update
    void Start()
    {
		holdingItemSpriteRenderer = transform.Find("Arm Front Parent").Find("Arm Front").transform.Find("HoldingItemPosition").transform.Find("HoldingItem").GetComponent<SpriteRenderer>();
        torchLight = holdingItemSpriteRenderer.gameObject.transform.Find("TorchLight").gameObject;
		placeBlockScript = GetComponent<PlaceBlockScript>();
		anim = GetComponent<Animator>();
		InventoryScript.initializeInventory();
        Craft.initializeCrafting();
        pickupItemSound = Resources.Load<AudioClip>("Sounds\\Random\\pop");
	}

    // Update is called once per frame
    void Update()
    {
        detectItems();

        if(Input.GetKeyDown(KeyCode.Q))
        {
            dropItem();
        }
	}

    public void holdItem(string itemName)
    {
        if (itemName.Equals("")) holdingItemSpriteRenderer.sprite = null;
        else
        {
            holdingItemSpriteRenderer.sprite = Resources.Load<Sprite>("Textures\\BlockTextures\\" + itemName);
		}

        if (itemName.Equals("Torch"))
        {
            anim.SetBool("isHoldingTorch", true);
            torchLight.SetActive(true);
        }
        else
        {
            anim.SetBool("isHoldingTorch", false);
            torchLight.SetActive(false);
        }

		placeBlockScript.checkIfHoldingPlaceableItem(itemName);
	}

    /**
     * checks if there is an item within the player's pickup range,
     * if so, then pick it up.
     */
    private void detectItems()
    {
        Vector3 torsoPosition = transform.Find("Torso").transform.position;

		// Create a list to store the results
		List<Collider2D> itemsToPickup = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Item")); // only blocks on layer "Item"

		// Check for overlaps
		if(Physics2D.OverlapCircle(torsoPosition, pickupRange, filter, itemsToPickup) > 0) // if there are items to pickup
        {
            foreach(Collider2D collider in itemsToPickup)
            {
                DroppedItemScript itemScript = collider.gameObject.GetComponent<DroppedItemScript>();
                if (itemScript.isPickupable())
                {
					pickupItem(collider.gameObject);
				}
            }
        }
	}

    /**
     * picks up the item if there is space and then destroys the item
     */
    public void pickupItem(GameObject itemContainer, bool isInstantiated = true)
    {

        GameObject item = itemContainer.transform.Find("Item").gameObject;
        ToolInstance itemTool = itemContainer.GetComponent<DroppedItemScript>().tool;
		ArmorInstance itemArmor = itemContainer.GetComponent<DroppedItemScript>().armor;

		string itemName = item.GetComponent<SpriteRenderer>().sprite.name;

		
        bool didPickup;
		if (itemTool != null || itemArmor != null || BlockHashtable.isNotStackable(itemName)) // if the item is a tool or armor or unstackable item
        {
			didPickup = InventoryScript.addToInventory(itemTool, itemArmor, itemName); // add tool or armor to inventory
        }
        else
        {
			didPickup = InventoryScript.addToInventory(itemName); // adds the item to the inventory if there is space
		}

        if (didPickup)
        {
            // TODO: make the item shoot towards the player and then destroy it when it hits the player's collider
            AudioSource.PlayClipAtPoint(pickupItemSound, transform.position);
            if(isInstantiated) Destroy(itemContainer);
        }
    }

    /**
     * drops an item from the selected hotbar slot 
     */
    private void dropItem()
    {
        int dropFromSlot = InventoryScript.getSelectedSlot();

        InventorySlot itemToDrop = InventoryScript.getItemsInSlot(dropFromSlot);
        if (itemToDrop.isEmpty()) return; // if there is not an item in this slot

        GameObject itemContainer = Resources.Load<GameObject>("Prefabs\\ItemContainer"); // get itemContainer
        GameObject item = itemContainer.transform.Find("Item").gameObject; // get item within itemContainer


		Sprite itemImage = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemToDrop.itemName); // get the image for the item
        item.GetComponent<SpriteRenderer>().sprite = itemImage; // put the image on the SpriteRenderer

        GameObject spawnedItem = Instantiate(itemContainer, new Vector2(transform.position.x, transform.position.y + .7f), itemContainer.transform.rotation); // spawn the item

		DroppedItemScript itemScript = spawnedItem.GetComponent<DroppedItemScript>();
		itemScript.tool = itemToDrop.toolInstance;
        itemScript.armor = itemToDrop.armorInstance;

		itemScript.setPickupable(false); // cant pickup the item immediately
        itemScript.addDropVelocity(transform.position); // shoot the item from the player

        InventoryScript.decrementSlot(dropFromSlot);
	}
}
