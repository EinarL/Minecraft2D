using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class RightClickBucket : RightClickItemBehaviour
{
	private Animator anim;
	private AudioClip waterGetSound;

	public RightClickBucket()
	{
		anim = GameObject.Find("SteveContainer").transform.Find("Steve").GetComponent<Animator>();
		waterGetSound = Resources.Load<AudioClip>("Sounds\\Liquid\\water_get");
	}

	public override void stopHoldingRightClick(bool executeDefaultBehaviour = true)
	{
		Vector2 mousePos = getMousePosition();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(mousePos, 0.0001f, filter, results);
		if (results.Count == 0) return;
		if (results.Count > 1) Debug.LogError("RightClickBucket: Found more than 1 water blocks at position: " + mousePos);

		WaterScript waterScript = results[0].GetComponent<WaterScript>();
		if (waterScript.waterState == 0 && !waterScript.isFlowing)
		{
			AudioSource.PlayClipAtPoint(waterGetSound, mousePos); // play water sound

			GameObject.Destroy(results[0].gameObject); // remove water
			Vector2 removedWaterPosition = getRoundedMousePosition();
			SpawningChunkData.updateChunkData(removedWaterPosition.x, removedWaterPosition.y, 0, "Water"); // remove water from chunk data

			InventoryScript.setSelectedSlotItem(new InventorySlot("WaterBucket")); // change Bucket to WaterBucket

			// make water around this water flow
			checkForWater();

			doPlaceAnimation();
		}

	}

	// after removing a water block this function runs to make the water around it flow
	private void checkForWater()
	{
		Vector2 pos = getRoundedMousePosition();
		WaterScript aboveWater = getWaterAtPosition(new Vector2(pos.x, pos.y + 1));
		if (aboveWater != null) aboveWater.startFlowing();
		WaterScript leftWater = getWaterAtPosition(new Vector2(pos.x - 1, pos.y));
		if (leftWater != null) leftWater.startFlowing();
		WaterScript rightWater = getWaterAtPosition(new Vector2(pos.x + 1, pos.y));
		if (rightWater != null) rightWater.startFlowing();
		WaterScript belowWater = getWaterAtPosition(new Vector2(pos.x, pos.y - 1));
		if (belowWater != null) belowWater.startFlowing();
	}

	private WaterScript getWaterAtPosition(Vector2 pos)
	{
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(pos, 0.45f, filter, results);
		if (results.Count == 0) return null;
		if (results.Count > 1) Debug.LogError("RightClickBucket: Found more than one water blocks at position: " + pos);
		return results[0].gameObject.GetComponent<WaterScript>();
	}



	private void doPlaceAnimation()
	{
		bool facingRight = anim.GetBool("isFacingRight");

		if (facingRight) anim.Play("fightFrontArm");
		else anim.Play("fightBackArm");
	}



	// this function should be empty:
	public override void rightClickItem()
	{

	}
}
