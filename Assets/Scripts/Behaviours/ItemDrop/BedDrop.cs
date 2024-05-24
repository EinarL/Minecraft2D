using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BedDrop : ItemDropBehaviour
{
	// bed is 2 blocks wide so when we destoy a part of a bed then we want to remove the other bed block also
	// if this is true then we remove the bed block on the right side of this block, otherwise left
	private bool removeRight;
	private Tilemap tilemap;

	public BedDrop(bool removeRight)
	{
		this.removeRight = removeRight;
		tilemap = GameObject.Find("Grid").transform.Find("Tilemap").GetComponent<Tilemap>();
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition)
	{
		
		if(removeRight) blockPosition = new Vector2(blockPosition.x + 1, blockPosition.y); // remove the bed block that is on the right side 
		else blockPosition = new Vector2(blockPosition.x - 1, blockPosition.y); // left side

		Collider2D[] results = new Collider2D[1];
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Tilemap")); // only blocks on layer "Tilemap"
		// Check for overlaps
		int count = Physics2D.OverlapCircle(blockPosition, 0.0001f, filter, results);
		if (count > 0) // if the bed is a Tile
		{
			Vector3Int tilePos = new Vector3Int((int)(blockPosition.x - .5f), (int)(blockPosition.y - .5f));
			tilemap.SetTile(tilePos, null); // remove tile
			SpawningChunkData.updateChunkData(blockPosition.x, blockPosition.y, 0, "Default");
		}
		else // if its not a tile, then it must be a GameObject
		{
			// Create a list to store the results
			List<Collider2D> blocks = new List<Collider2D>();

			filter = new ContactFilter2D();
			filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "FrontBackground" or "BackBackground"

			// Check for overlaps
			Physics2D.OverlapCircle(blockPosition, 0.0001f, filter, blocks);

			foreach(Collider2D collider in blocks)
			{
				if (collider.name.StartsWith("Bed"))
				{
					Object.Destroy(collider.gameObject);
					SpawningChunkData.updateChunkData(blockPosition.x, blockPosition.y, 0, LayerMask.LayerToName(collider.gameObject.layer));
					break;
				}
			}

		}

		return base.dropItem("Bed", null);
	}
}
