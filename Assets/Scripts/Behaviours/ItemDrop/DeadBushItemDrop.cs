using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBushItemDrop : ItemDropBehaviour
{

	private float stickDropChance = 50;
	public DeadBushItemDrop()
	{
	}


	public override List<GameObject> dropItem(string gameObjectaName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		if (rand < stickDropChance) // spawn stick
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\Stick"); // change item texture
			return new List<GameObject> { itemToDrop };
		}

		return null;
	}
}
