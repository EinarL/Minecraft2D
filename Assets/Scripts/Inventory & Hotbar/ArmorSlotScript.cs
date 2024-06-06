using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Runtime.Serialization;


/**
 * this class is responsible for:
 * * hover texture on the armor slot
 * * picking up an item from the slot
 * * putting an item in the slot
 */
public class ArmorSlotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, Slot
{
    public ArmorType armorSlotType; // which armor slot type is this? a Chestplate slot type will only allow chesplates to be in the slot, etc.
    private GameObject itemImage;
    private Image hoverTexture;
    private OpenInventoryScript openInventoryScript;
    private InventorySlot itemInSlot = new InventorySlot();
    private DurabilityBar durabilityBarScript;
    private GameObject outline; // the outline of the armor
    private ArmorScript armorScript;
    private ArmorOutfitScript armorOutfitScript;

    // Start is called before the first frame update
    void Start()
    {
        outline = transform.Find("Outline").gameObject;
        itemImage = transform.Find("ItemImage").gameObject;
		hoverTexture = transform.Find("HoverTexture").GetComponent<Image>();
        armorScript = GameObject.Find("Canvas").transform.Find("Armorbar").GetComponent<ArmorScript>();
		armorOutfitScript = ArmorOutfitScript.Instance;

		openInventoryScript = transform.parent.parent.parent.parent.GetComponent<OpenInventoryScript>();
    }
    /**
     * gets called when the game starts if there is a saved armor.json file.
     * this initializes this slot with the armor that is in the file.
     */
    public void initializeSlot(InventorySlot armor)
    {
        if (armor.isEmpty()) return;
        if (armorScript == null) Start();
        itemInSlot = armor;
        armorScript.addArmor(armor.armorInstance.armorPoints);
        updateSlot();
		outline.SetActive(false);
        updateArmorVisually();
	}

	/**
     * updates what is in the slot 
     */
	public void updateSlot()
    {
        if(itemImage == null) Start();
		Sprite image = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemInSlot.itemName);

        if(image != null) // found item to display
        {
            itemImage.GetComponent<Image>().sprite = image;
            
            itemImage.SetActive(true);
		}
        else
        {
            itemImage.SetActive(false);
        }

        // if there is an armor in this slot then add the durability bar, but only display it if durability < STARTING_DURABILITY
        if(itemInSlot.armorInstance != null)
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
            durabilityBarScript.setMaximumDurability(itemInSlot.armorInstance);
            durabilityBarScript.updateDurability(itemInSlot.armorInstance);

            if (itemInSlot.armorInstance.getDurability() >= itemInSlot.armorInstance.getStartingDurability()) // only display durability bar if durability < STARTING_DURABILITY
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
            // delete durability bar since there isnt any armor in this slot
            durabilityBarScript = null;

            Destroy(transform.Find("DurabilityBarBackground(Clone)").gameObject);
        }   
	}

    public void updateDurabilityBar()
    {
        if (!hasDurabilityBar()) return;
		durabilityBarScript.gameObject.SetActive(true);

		durabilityBarScript.updateDurability(itemInSlot.armorInstance);
	}

    public void reduceArmorDurability()
    {
        if(itemInSlot.isEmpty()) return;
        itemInSlot.armorInstance.reduceDurability();

        if(itemInSlot.armorInstance.getDurability() <= 0) // if durability is 0, then break the armor
        {
            removeArmorFromSlot();
        }
        else updateDurabilityBar();
    }

    private void removeArmorFromSlot()
    {
		armorScript.removeArmor(itemInSlot.armorInstance.armorPoints);
		itemInSlot.removeEverythingFromSlot();
		updateSlot();
		outline.SetActive(true);
		updateArmorVisually();
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

			InventoryScript.setItemsPickedUp(new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.armorInstance, itemInSlot.amount));
            removeArmorFromSlot();
			openInventoryScript.setIsItemBeingHeld(true);
		}
        else if(hasPickedUp) // if the player has picked up something
		{
            // put the picked up items in this slot
            InventorySlot pickedUpItem = InventoryScript.getItemsPickedUp();
            if (!pickedUpItem.isArmor()) return; // only allow armors in this slot
            if (pickedUpItem.armorInstance.getArmorType() != armorSlotType) return; // only allow chestplates if this is a chestplate slot, etc.

            if (!itemInSlot.isEmpty()) // if there is an armor in this slot, then we want to switch the items being held and the item in the slot
            {
				armorScript.removeArmor(itemInSlot.armorInstance.armorPoints); // remove armor points 
                InventoryScript.setItemsPickedUp(new InventorySlot(itemInSlot.itemName, itemInSlot.toolInstance, itemInSlot.armorInstance, 1));
				openInventoryScript.setIsItemBeingHeld(true);
			}
            else openInventoryScript.setIsItemBeingHeld(false);
			armorScript.addArmor(pickedUpItem.armorInstance.armorPoints); // add the armor points

            itemInSlot.putItemOrToolInSlot(pickedUpItem.itemName, pickedUpItem.toolInstance, pickedUpItem.armorInstance, 1);
            
            updateSlot();
			outline.SetActive(false);

			updateArmorVisually();
		}
        
    }
    /**
     * this function calls another function in ArmorOutfitScript to show the armor on the player
     */
    private void updateArmorVisually()
    {
        if (itemInSlot.isEmpty()) // if we just removed an armor
        {
            switch (armorSlotType)
            {
                case ArmorType.Helmet:
                    armorOutfitScript.removeHelmet();
                    break;
				case ArmorType.Chestplate:
					armorOutfitScript.removeChestplate();
					break;
				case ArmorType.Leggings:
					armorOutfitScript.removeLeggings();
					break;
				case ArmorType.Boots:
					armorOutfitScript.removeBoots();
					break;
			}
            return;
        }
		// if we get here then we just added an armor to this slot
		switch (armorSlotType)
		{
			case ArmorType.Helmet:
                switch (itemInSlot.armorInstance.getArmorMaterial())
                {
                    case ArmorMaterial.Leather:
						// armorOutfitScript.addLeatherHelmet();
						break;
                    case ArmorMaterial.Iron:
                        armorOutfitScript.addIronHelmet();
                        break;
                    case ArmorMaterial.Gold:
						// armorOutfitScript.addGoldHelmet();
						break;
                    case ArmorMaterial.Diamond:
						armorOutfitScript.addDiamondHelmet();
						break;
                }
				break;
			case ArmorType.Chestplate:
				switch (itemInSlot.armorInstance.getArmorMaterial())
				{
					case ArmorMaterial.Leather:
						// armorOutfitScript.addLeatherChestplate();
						break;
					case ArmorMaterial.Iron:
						armorOutfitScript.addIronChestplate();
						break;
					case ArmorMaterial.Gold:
						// armorOutfitScript.addGoldChestplate();
						break;
					case ArmorMaterial.Diamond:
						armorOutfitScript.addDiamondChestplate();
						break;
				}
				break;
			case ArmorType.Leggings:
				switch (itemInSlot.armorInstance.getArmorMaterial())
				{
					case ArmorMaterial.Leather:
						// armorOutfitScript.addLeatherLeggings();
						break;
					case ArmorMaterial.Iron:
						armorOutfitScript.addIronLeggings();
						break;
					case ArmorMaterial.Gold:
						// armorOutfitScript.addGoldLeggings();
						break;
					case ArmorMaterial.Diamond:
						armorOutfitScript.addDiamondLeggings();
						break;
				}
				break;
			case ArmorType.Boots:
				switch (itemInSlot.armorInstance.getArmorMaterial())
				{
					case ArmorMaterial.Leather:
						// armorOutfitScript.addLeatherBoots();
						break;
					case ArmorMaterial.Iron:
						armorOutfitScript.addIronBoots();
						break;
					case ArmorMaterial.Gold:
						// armorOutfitScript.addGoldBoots();
						break;
					case ArmorMaterial.Diamond:
						armorOutfitScript.addDiamondBoots();
						break;
				}
				break;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hoverTexture.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // remove transparency
        openInventoryScript.setHoveringOverSlotScript(this);
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

	// does this inventory slot have a durability bar for an armor
	private bool hasDurabilityBar()
    {
        return durabilityBarScript != null;
    }

    public InventorySlot getArmorInSlot()
    {
        return itemInSlot;
    }
}
