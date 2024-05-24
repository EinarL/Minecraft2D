using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBed : PlaceBlockBehaviour
{
	public List<GameObject> placeBlock(GameObject blockToPlace, PlaceBlockScript pbScript, BreakBlockScript bbScript)
	{
		bool placeLeftSide = blockToPlace.transform.position.x < pbScript.gameObject.transform.parent.position.x;

		GameObject bedLower;
		GameObject bedUpper;

		// if were placing the bed on the left side
		if (placeLeftSide)
		{
			bedLower = GameObject.Instantiate(BlockHashtable.getBlockByID(38), blockToPlace.transform.position, Quaternion.identity); // place BedLower block
			bedUpper = GameObject.Instantiate(BlockHashtable.getBlockByID(37), new Vector2(blockToPlace.transform.position.x - 1, blockToPlace.transform.position.y), Quaternion.identity); // place BedUpper block on left side
		}
		else
		{
			bedLower = GameObject.Instantiate(BlockHashtable.getBlockByID(40), blockToPlace.transform.position, Quaternion.identity); // place BedLower block
			bedUpper = GameObject.Instantiate(BlockHashtable.getBlockByID(39), new Vector2(blockToPlace.transform.position.x + 1, blockToPlace.transform.position.y), Quaternion.identity); // place BedUpper block on right side
		}

		return new List<GameObject>() { bedLower, bedUpper };
	}
}
