using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorDrop : ItemDropBehaviour
{
	// a door is 2 blocks tall so when we destoy a part of a door then we want to remove the other door block also
	// if this is true then we remove the door block above this block, otherwise below
	private bool removeTop;
	private string doorType;
	private Tilemap tilemap;

	public DoorDrop(bool removeTop, string doorType = "Oak")
	{
		this.removeTop = removeTop;
		this.doorType = doorType;
		tilemap = GameObject.Find("Grid").transform.Find("Tilemap").GetComponent<Tilemap>();
	}


	public override List<GameObject> dropItem(string gameObjectName, ToolInstance usingTool, Vector2 blockPosition)
	{
		
		if(removeTop) blockPosition = new Vector2(blockPosition.x, blockPosition.y + 1); // remove the door block that is above this block
		else blockPosition = new Vector2(blockPosition.x, blockPosition.y - 1); // below

		Collider2D[] results = new Collider2D[1];
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Tilemap")); // only blocks on layer "Tilemap"
		// Check for overlaps
		int count = Physics2D.OverlapCircle(blockPosition, 0.45f, filter, results);
		if (count > 0) // if the door is a Tile
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
			Physics2D.OverlapCircle(blockPosition, 0.45f, filter, blocks);

			foreach(Collider2D collider in blocks)
			{
				if (collider.name.StartsWith("Door"))
				{
					Object.Destroy(collider.gameObject);
					SpawningChunkData.updateChunkData(blockPosition.x, blockPosition.y, 0, LayerMask.LayerToName(collider.gameObject.layer));
					break;
				}
			}

		}

		return base.dropItem("Door" + doorType, null);
	}
}
