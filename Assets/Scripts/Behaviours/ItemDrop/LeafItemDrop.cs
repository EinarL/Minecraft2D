using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafItemDrop : ItemDropBehaviour
{

	private float appleChance = 10;
	private float saplingChance = 20;
	public LeafItemDrop()
	{
	}


	public override List<GameObject> dropItem(string gameObjectaName, ToolInstance usingTool)
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
}
