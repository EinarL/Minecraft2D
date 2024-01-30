using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class FallScript : MonoBehaviour
{

    private bool isFalling = false;
    private Vector3 velocity = new Vector3(0, -1, 0);
    private Transform groundCheck;

    // Start is called before the first frame update
    void Start()
    {
        groundCheck = transform.Find("GroundCheck");
    }

	void Update()
	{
        if (isFalling)
        {
			velocity += Physics.gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;

            checkIfHitGround();
        }
	}


	/**
     * gets called when the block below is mined or when a FallType below falls
     * makes this block start falling
     */
	public void fall()
	{
		Debug.Log("fall");
		SpawningChunkData.updateChunkData(transform.position.x, transform.position.y, 0, LayerMask.LayerToName(gameObject.layer)); // remove block from data
        isFalling = true;
		checkIfAboveBlockIsFallType(); // check if above block needs to fall also
		checkIfAboveIsNoFloatType(); // check if above block needs to get destroyed because some blocks are not allowed to float
	}

	private void checkIfAboveBlockIsFallType()
	{
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) | LayerMask.GetMask("BackBackground");
		contactFilter.useLayerMask = true;

		GameObject aboveBlock = getAboveBlock(contactFilter);
		if (aboveBlock != null && aboveBlock.tag.Equals("FallType"))
		{
			Debug.Log("above is fall type, so fall");
			aboveBlock.GetComponent<FallScript>().fall();
		}
	}

	private void checkIfAboveIsNoFloatType()
	{
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("FrontBackground");
		contactFilter.useLayerMask = true;

		GameObject aboveBlock = getAboveBlock(contactFilter);
		if (aboveBlock != null && aboveBlock.tag.Equals("NoFloatType")) aboveBlock.GetComponent<BlockScript>().breakBlock();
	}

	private GameObject getAboveBlock(ContactFilter2D contactFilter)
	{
		Collider2D[] results = new Collider2D[1];

		int count = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y + 1), 0.05f, contactFilter, results);

		if (count > 0) return results[0].gameObject;
		return null;
	}
	// returns true if falling else false
	public void checkIfHitGround()
    {
		if (groundCheck == null) Start();

		Collider2D[] results = new Collider2D[2];

		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)) | LayerMask.GetMask("Default");
		contactFilter.useLayerMask = true;

		int count = Physics2D.OverlapCircle(groundCheck.position, 0.01f, contactFilter, results);
        foreach (Collider2D result in results)
        {
			if (result != null && result.gameObject.GetInstanceID() != gameObject.GetInstanceID() && count > 0)
			{
				// if (the block below is a fallType && it is not falling) || its not a fallType
				if(result.gameObject.tag.Equals("FallType") && !result.gameObject.GetComponent<FallScript>().isFallingDown() || !result.gameObject.tag.Equals("FallType"))
				{
					isFalling = false;
					velocity = new Vector3(0, -1, 0);
					goToClosestBlockPos();
					return;
				}
			}
		}
		return;
	}

    /**
     * this runs after the block stops falling
     * it places the block in the closest "block position"
     */
    private void goToClosestBlockPos()
    {
        int roundedYPos = (int)transform.position.y;

		if (transform.position.y < 0) transform.position = new Vector2(transform.position.x, roundedYPos - .5f);
        else transform.position = new Vector2(transform.position.x, roundedYPos + .5f);

		SpawningChunkData.updateChunkData(transform.position.x, transform.position.y, BlockHashtable.getIDByBlockName(gameObject.name), LayerMask.LayerToName(gameObject.layer)); // save the block to data
	}

	public bool isFallingDown()
	{
		return isFalling;
	}
}
