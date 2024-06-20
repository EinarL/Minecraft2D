using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/**
 * this script displays the armor-bar,
 * knows how much protection the player should get from taking damage
 */
public class ArmorScript : MonoBehaviour
{
    private Sprite armorEmptyImage;
	private Sprite armorHalfImage;
	private Sprite armorFullImage;

	private Image[] ArmorImages = new Image[10];

	private ArmorSlotScript helmetSlot;
	private ArmorSlotScript chestplateSlot;
	private ArmorSlotScript leggingsSlot;
	private ArmorSlotScript bootsSlot;

	private IDataService dataService = JsonDataService.Instance;

	private int totalArmor = 0; // from 0 to 20 (inclusive), how much armor points the player has

	// Start is called before the first frame update
	void Start()
    {
		for (int i = 0; i < ArmorImages.Length; i++)
		{
			ArmorImages[i] = transform.Find("Armor" + i).GetComponent<Image>();
		}

		Sprite[] armorbarImages = Resources.LoadAll<Sprite>("Textures/UI/icons");

		armorEmptyImage = getSpriteWithName(armorbarImages, "icons_4");
		armorHalfImage = getSpriteWithName(armorbarImages, "icons_5");
		armorFullImage = getSpriteWithName(armorbarImages, "icons_6");

		Transform armorSlotsParent = GameObject.Find("Canvas").transform.Find("InventoryParent").Find("Inventory").Find("InventoryPanel").Find("ArmorSlots").transform;

		helmetSlot = armorSlotsParent.Find("HelmetSlot").GetComponent<ArmorSlotScript>();
		chestplateSlot = armorSlotsParent.Find("ChestplateSlot").GetComponent<ArmorSlotScript>();
		leggingsSlot = armorSlotsParent.Find("LeggingsSlot").GetComponent<ArmorSlotScript>();
		bootsSlot = armorSlotsParent.Find("BootsSlot").GetComponent<ArmorSlotScript>();


		// load the saved armor
		if (dataService.exists("armor.json"))
		{
			InventorySlot[] armorSlots = dataService.loadData<InventorySlot[]>("armor.json");

			helmetSlot.initializeSlot(armorSlots[0]);
			chestplateSlot.initializeSlot(armorSlots[1]);
			leggingsSlot.initializeSlot(armorSlots[2]);
			bootsSlot.initializeSlot(armorSlots[3]);
		}
	}

	public void saveArmor()
	{
		InventorySlot[] armorSlots = new InventorySlot[]{
			helmetSlot.getArmorInSlot(),
			chestplateSlot.getArmorInSlot(),
			leggingsSlot.getArmorInSlot(),
			bootsSlot.getArmorInSlot(),
		};

		if (!dataService.saveData("armor.json", armorSlots)) // save armor
		{
			Debug.LogError("Could not save armor file :(");
		}
	}

    private void updateArmorImages()
    {
		if(totalArmor == 0)
		{
			foreach(Image image in ArmorImages)
			{
				image.color = new Color(1, 1, 1, 0); // make all armor images invisible
			}
			return;
		}

		int i;
		for (i = 0; i < Mathf.Floor(totalArmor / 2f); i++)
		{
			ArmorImages[i].sprite = armorFullImage;
			ArmorImages[i].color = new Color(1, 1, 1, 1); // make visible
		}
		if (totalArmor % 2 != 0) // if totalArmor is an odd number, then we need to display half a heart
		{
			ArmorImages[i].sprite = armorHalfImage;
			ArmorImages[i].color = new Color(1, 1, 1, 1);
			i++;
		}
		for (; i < 10; i++) // no fill image for the rest of the hearts
		{
			ArmorImages[i].sprite = armorEmptyImage;
			ArmorImages[i].color = new Color(1, 1, 1, 1);
		}
	}

    public void addArmor(int armorPoints)
    {
        totalArmor += armorPoints;
        Assert.IsTrue(totalArmor >= 0 && totalArmor <= 20, "totalArmor is out of the valid range (0-20)");
        updateArmorImages();
    }

	public void removeArmor(int armorPoints)
	{
		totalArmor -= armorPoints;
		Assert.IsTrue(totalArmor >= 0 && totalArmor <= 20, "totalArmor is out of the valid range (0-20)");
        updateArmorImages();
	}

    /**
     * gets called when the player is about to takes damage and returns the damage that the player should take
     */
    public int getReducedDamage(int baseDamage)
    {
		reduceArmorDurability();

        float damageReductionPercentage = 1 - (totalArmor / 20f) * 0.8f;

		return Mathf.RoundToInt(baseDamage * damageReductionPercentage);
    }

	private void reduceArmorDurability()
	{
		helmetSlot.reduceArmorDurability();
		chestplateSlot.reduceArmorDurability();
		leggingsSlot.reduceArmorDurability();
		bootsSlot.reduceArmorDurability();
	}

	public InventorySlot[] getArmorSlots()
	{
		return new InventorySlot[] { helmetSlot.getArmorInSlot(), chestplateSlot.getArmorInSlot(), leggingsSlot.getArmorInSlot(), bootsSlot.getArmorInSlot() };
	}

	/**
	 * adds the armor to the player if there is place for the armor
	 * 
	 * armor: InventorySlot[]{ helmetSlot, chestplateSlot, leggingsSlot, bootsSlot }
	 * 
	 * returns the armor that there wasnt space for
	 * 
	 */
	public List<InventorySlot> addArmor(InventorySlot[] armor)
	{
		List<InventorySlot> armorsThatWerentPutOn = new List<InventorySlot>();

		if (armor[0].armorInstance != null && !helmetSlot.addArmorIfThereIsSpace(armor[0])) armorsThatWerentPutOn.Add(armor[0]);
		if (armor[1].armorInstance != null && !chestplateSlot.addArmorIfThereIsSpace(armor[1])) armorsThatWerentPutOn.Add(armor[1]);
		if (armor[2].armorInstance != null && !leggingsSlot.addArmorIfThereIsSpace(armor[2])) armorsThatWerentPutOn.Add(armor[2]);
		if (armor[3].armorInstance != null && !bootsSlot.addArmorIfThereIsSpace(armor[3])) armorsThatWerentPutOn.Add(armor[3]);

		return armorsThatWerentPutOn;
	}
	/**
	 * runs when the player dies
	 */
	public void removeAllArmor()
	{
		helmetSlot.removeArmorFromSlot();
		chestplateSlot.removeArmorFromSlot();
		leggingsSlot.removeArmorFromSlot();
		bootsSlot.removeArmorFromSlot();
	}


	private Sprite getSpriteWithName(Sprite[] list, string name)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i].name == name)
			{
				return list[i];
			}
		}
		Debug.LogError("ERROR: sprite with name " + name + " was not located in the icons sprite");
		return null;
	}

}
