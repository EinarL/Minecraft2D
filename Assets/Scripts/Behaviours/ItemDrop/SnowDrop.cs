using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowDrop : ItemDropBehaviour
{
	private System.Random random = new System.Random();
	private int[] maxAndMinSnowBalls;
	public SnowDrop(int min, int max)
	{
		maxAndMinSnowBalls = new int[] { min, max };
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition = default)
	{
		GameObject itemToDrop = Resources.Load("Prefabs\\ItemContainer") as GameObject;
		itemToDrop.transform.Find("Item").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures\\ItemTextures\\Snowball"); // change item texture to the block we were breaking
		int amountOfSnowBalls = random.Next(maxAndMinSnowBalls[0], maxAndMinSnowBalls[1] + 1);
		List<GameObject> snowBalls = new List<GameObject>();
		for (int _ = 0; _ < amountOfSnowBalls; _++)
		{
			snowBalls.Add(itemToDrop);
		}
		return snowBalls;
	}
}
