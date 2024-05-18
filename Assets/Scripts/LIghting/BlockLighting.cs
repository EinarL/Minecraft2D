using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using UnityEngine.UI;

public class BlockLighting : MonoBehaviour
{

	private int stage = 4;
	private SpriteRenderer blockRenderer;

	private void Start()
	{
		blockRenderer = GetComponent<SpriteRenderer>();
	}

	public void setStage(int stage = 4)
	{
		if (blockRenderer == null) Start();

		this.stage = stage;
		Debug.Log("Stage: " + stage);
		switch (stage)
		{
			case 0:
				blockRenderer.color = Color.white;
				break;
			case 1:
				blockRenderer.color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
				break;
			case 2:
				blockRenderer.color = new Color(130f / 255f, 130f / 255f, 130f / 255f);
				break;
			case 3:
				blockRenderer.color = new Color(60f / 255f, 60f / 255f, 60f / 255f);
				break;
			case 4:
				blockRenderer.color = Color.black;
				break;
			default:
				Debug.LogError("stage is not a valid stage: " + stage + " should be between 0 and 4");
				break;
		}
	}

	public int getStage()
	{
		return stage;
	}

	// returns the adjacent blocks of this block
	public List<BlockLighting> getNeighbors()
	{
		List<BlockLighting> neighbors = new List<BlockLighting>();

		GameObject block = gameObject;
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default"));

		Collider2D[] results = new Collider2D[1];

		// check above block
		Vector2 aboveBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y + block.GetComponent<SpriteRenderer>().bounds.size.y);
		Physics2D.OverlapCircle(aboveBlockPosition, 0.1f, filter, results);
		if (results[0] != null)
		{
			neighbors.Add(results[0].gameObject.GetComponent<BlockLighting>());
			results[0] = null;
		}

		// check below block
		Vector2 belowBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y - block.GetComponent<SpriteRenderer>().bounds.size.y);
		Physics2D.OverlapCircle(belowBlockPosition, 0.1f, filter, results);
		if (results[0] != null)
		{
			neighbors.Add(results[0].gameObject.GetComponent<BlockLighting>());
			results[0] = null;
		}

		// check right side
		Vector2 rightBlockPosition = new Vector2(block.transform.position.x + block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
		Physics2D.OverlapCircle(rightBlockPosition, 0.1f, filter, results);
		if (results[0] != null)
		{
			neighbors.Add(results[0].gameObject.GetComponent<BlockLighting>());
			results[0] = null;
		}

		// check left side
		Vector2 leftBlockPosition = new Vector2(block.transform.position.x - block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
		Physics2D.OverlapCircle(leftBlockPosition, 0.1f, filter, results);
		if (results[0] != null)
		{
			neighbors.Add(results[0].gameObject.GetComponent<BlockLighting>());
			results[0] = null;
		}

		return neighbors;
	}
}
