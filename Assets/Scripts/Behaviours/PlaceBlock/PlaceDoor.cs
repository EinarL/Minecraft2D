using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceDoor : PlaceBlockBehaviour
{
	public List<GameObject> placeBlock(GameObject blockToPlace, PlaceBlockScript pbScript, BreakBlockScript bbScript)
	{
		bool placeLeftSide = blockToPlace.transform.position.x < pbScript.gameObject.transform.parent.position.x;
		
		int doorTopBlockID = 43;
		int doorBottomBlockID = 44;

		if (blockToPlace.gameObject.name.Contains("Oak")) // placing oak door
		{
			if (placeLeftSide)
			{
				doorTopBlockID = 47;
				doorBottomBlockID = 48;
			}
			else
			{
				doorTopBlockID = 43;
				doorBottomBlockID = 44;
			}
		}

		GameObject doorTop = GameObject.Instantiate(BlockHashtable.getBlockByID(doorTopBlockID), new Vector2(blockToPlace.transform.position.x, blockToPlace.transform.position.y + 1), Quaternion.identity); // place top door block
		GameObject doorBottom = GameObject.Instantiate(BlockHashtable.getBlockByID(doorBottomBlockID), blockToPlace.transform.position, Quaternion.identity); // place bottom door block


		return new List<GameObject>() { doorTop, doorBottom };
	}
}
