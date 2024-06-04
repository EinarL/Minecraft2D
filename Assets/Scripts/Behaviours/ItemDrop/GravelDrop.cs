using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravelDrop : ItemDropBehaviour
{
	private System.Random random = new System.Random();
	private float dropFlintChance = 0.2f;

	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		if(random.NextDouble() < dropFlintChance)
		{
			GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
			itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\Flint");
			return new List<GameObject> { itemToDrop };
		}

		return base.dropItem(gameObjectName, usingTool);
	}
}
