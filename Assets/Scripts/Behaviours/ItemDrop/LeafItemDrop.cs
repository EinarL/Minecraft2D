using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafItemDrop : ItemDropBehaviour
{

	private float appleChance = 10;
	private float saplingChance = 15;
	private string leafType; // this is one of "Oak", "Spruce", etc. 
	public LeafItemDrop(string leafType)
	{
		this.leafType = leafType;
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool)
	{
		// TODO: if usingTool is a shear, then drop the leaf block

		if (leafType.Equals("Oak")) return oakDropItem();
		if (leafType.Equals("Spruce")) return spruceDropItem();

		Debug.LogError("Error! leafType is not a legal value. leafType: " + leafType);
		return null;
	}

	private List<GameObject> oakDropItem()
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		if (rand < saplingChance) // spawn sapling
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\SaplingOak"); // change item texture
			return new List<GameObject> { itemToDrop };
		}
		else if (rand < saplingChance + appleChance) // spawn apple
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\Apple"); // change item texture
			return new List<GameObject> { itemToDrop };
		}
		return null;
	}

	private List<GameObject> spruceDropItem()
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		if (rand < saplingChance) // spawn sapling
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\SaplingSpruce"); // change item texture
			return new List<GameObject> { itemToDrop };
		}
		return null;
	}
}
