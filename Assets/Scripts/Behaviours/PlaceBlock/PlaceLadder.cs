using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceLadder : PlaceBlockBehaviour
{
	public List<GameObject> placeBlock(GameObject blockToPlace, PlaceBlockScript pbScript, BreakBlockScript bbScript)
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("BackBackground") | LayerMask.GetMask("BackgroundVisual"));
		// there is a block in the backgroundVisualLayer or background layer, then return null to place ladder normally
		if (pbScript.checkIfBlockInPosition(filter) || pbScript.backgroundVisualTiles.HasTile(pbScript.backgroundVisualTiles.WorldToCell(blockToPlace.transform.position)))
		{
			return null;
		}
		else if (bbScript.isBlockOnRightSideOfBlock(blockToPlace.transform.position, true)) // place ladder on right side of wall
		{
			return new List<GameObject>() { GameObject.Instantiate(BlockHashtable.getBlockByID(60), blockToPlace.transform.position, Quaternion.identity) };
		}
		else if (bbScript.isBlockOnLeftSideOfBlock(blockToPlace.transform.position, true)) // place ladder on left side of the wall
		{
			return new List<GameObject>() { GameObject.Instantiate(BlockHashtable.getBlockByID(59), blockToPlace.transform.position, Quaternion.identity) };
		} 
		
		Debug.LogError("Error with placing the ladder, there isnt a block on the left, right or behind the ladder");
		return null;
	}
}
