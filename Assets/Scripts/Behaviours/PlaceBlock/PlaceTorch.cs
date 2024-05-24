using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceTorch : PlaceBlockBehaviour
{
	public List<GameObject> placeBlock(GameObject blockToPlace, PlaceBlockScript pbScript, BreakBlockScript bbScript)
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("BackBackground") | LayerMask.GetMask("BackgroundVisual"));
		// there is a block in the backgroundVisualLayer or background layer, then return null to place torch normally
		if (bbScript.isBlockBelowBlock(blockToPlace.transform.position)) // place torch normally
		{
			return null;
		}
		// place torch on wall in the background
		else if (pbScript.checkIfBlockInPosition(filter) || pbScript.backgroundVisualTiles.HasTile(pbScript.backgroundVisualTiles.WorldToCell(blockToPlace.transform.position)))
		{
			return new List<GameObject>() { GameObject.Instantiate(BlockHashtable.getBlockByID(25), blockToPlace.transform.position, Quaternion.identity) };
		}
		else if (bbScript.isBlockOnRightSideOfBlock(blockToPlace.transform.position, true)) // place torch on right side of wall
		{
			return new List<GameObject>() { GameObject.Instantiate(BlockHashtable.getBlockByID(27), blockToPlace.transform.position, Quaternion.identity) };
		}
		else if (bbScript.isBlockOnLeftSideOfBlock(blockToPlace.transform.position, true)) // place torch on left side of the wall
		{
			return new List<GameObject>() { GameObject.Instantiate(BlockHashtable.getBlockByID(26), blockToPlace.transform.position, Quaternion.identity) };
		} 
		
		Debug.LogError("Error with placing the torch, there isnt a block on the left, right, bottom or behind the torch");
		return null;
	}
}
