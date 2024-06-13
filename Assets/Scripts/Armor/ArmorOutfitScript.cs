using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class has functions to put armor off/on visually on steve
 * 
 * this class is a non-thread safe singleton pattern.
 */
public class ArmorOutfitScript
{
	private SpriteRenderer helmet;
	private SpriteRenderer torsoChestplate;
	private SpriteRenderer armFrontChestplate;
	private SpriteRenderer armBackChestplate;
	private SpriteRenderer legFrontLeggings;
	private SpriteRenderer legBackLeggings;
	private SpriteRenderer legFrontBoots;
	private SpriteRenderer legBackBoots;

	private Sprite[] ironArmorImages;
	private Sprite[] ironLeggingsImages;

	private Sprite[] diamondArmorImages;
	private Sprite[] diamondLeggingsImages;

	private static ArmorOutfitScript instance = null;

	public static ArmorOutfitScript Instance
	{
		get
		{
			if(instance == null) instance = new ArmorOutfitScript();
			return instance;
		}
	}

	public static void removeInstance()
	{
		instance = null;
	}


	private ArmorOutfitScript()
	{
		Transform steve = GameObject.Find("SteveContainer").transform.Find("Steve").transform;

		helmet = steve.Find("Head").Find("Helmet").GetComponent<SpriteRenderer>();
		torsoChestplate = steve.Find("Torso").Find("TorsoChestplate").GetComponent<SpriteRenderer>();
		armFrontChestplate = steve.Find("Arm Front Parent").Find("Arm Front").Find("ArmFrontChestplate").GetComponent<SpriteRenderer>();
		armBackChestplate = steve.Find("Arm Back Parent").Find("Arm Back").Find("ArmBackChestplate").GetComponent<SpriteRenderer>();
		legFrontLeggings = steve.Find("Leg Front").Find("LegFrontLeggings").GetComponent<SpriteRenderer>();
		legBackLeggings = steve.Find("Leg Back").Find("LegBackLeggings").GetComponent<SpriteRenderer>();
		legFrontBoots = steve.Find("Leg Front").Find("LegFrontBoots").GetComponent<SpriteRenderer>();
		legBackBoots = steve.Find("Leg Back").Find("LegBackBoots").GetComponent<SpriteRenderer>();


		ironArmorImages = Resources.LoadAll<Sprite>("Textures/Armor/iron_layer_1");
		ironLeggingsImages = Resources.LoadAll<Sprite>("Textures/Armor/iron_layer_2");

		diamondArmorImages = Resources.LoadAll<Sprite>("Textures/Armor/diamond_layer_1");
		diamondLeggingsImages = Resources.LoadAll<Sprite>("Textures/Armor/diamond_layer_2");
	}

	public void removeHelmet()
	{
		helmet.sprite = null;
	}

	public void removeChestplate()
	{
		torsoChestplate.sprite = null;
		armFrontChestplate.sprite = null;
		armBackChestplate.sprite = null;
	}

	public void removeLeggings()
	{
		legFrontLeggings.sprite = null;
		legBackLeggings.sprite = null;
	}

	public void removeBoots()
	{
		legFrontBoots.sprite = null;
		legBackBoots.sprite = null;
	}

	public void addIronHelmet()
	{
		helmet.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_0");
	}

	public void addIronChestplate()
	{
		torsoChestplate.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_1");
		armFrontChestplate.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_2");
		armBackChestplate.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_5");
	}

	public void addIronLeggings()
	{
		legFrontLeggings.sprite = getSpriteWithName(ironLeggingsImages, "iron_layer_2_0");
		legBackLeggings.sprite = getSpriteWithName(ironLeggingsImages, "iron_layer_2_1");
	}

	public void addIronBoots()
	{
		legFrontBoots.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_3");
		legBackBoots.sprite = getSpriteWithName(ironArmorImages, "iron_layer_1_4");
	}

	public void addDiamondHelmet()
	{
		helmet.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_0");
	}

	public void addDiamondChestplate()
	{
		torsoChestplate.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_1");
		armFrontChestplate.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_2");
		armBackChestplate.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_2");
	}

	public void addDiamondLeggings()
	{
		legFrontLeggings.sprite = getSpriteWithName(diamondLeggingsImages, "diamond_layer_2_0");
		legBackLeggings.sprite = getSpriteWithName(diamondLeggingsImages, "diamond_layer_2_1");
	}

	public void addDiamondBoots()
	{
		legFrontBoots.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_3");
		legBackBoots.sprite = getSpriteWithName(diamondArmorImages, "diamond_layer_1_4");
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
		Debug.LogError("ERROR: sprite with name " + name + " was not located in within the sprites");
		return null;
	}
}
