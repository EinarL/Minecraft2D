using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BreakBlockScript : MonoBehaviour
{

    private Camera cam;
    public GameObject hoverTexture; // hovering over block texture
    private PlayerControllerScript playerControllerScript;
    private Transform stevePosition; // get distance from this transform to the blockPos to check if the distance in within range of mining/placing blocks
    private Transform headPosition;
    private Animator anim;
    private GameObject hoverGrid = null;
    private GameObject hoveringOverBlock = null; // the block the mouse is hovering over
    private GameObject miningBlock = null; // the block that we are mining
    private float miningRange = 6;
    private bool isBreaking = false; // is in the process of breaking a block

    private AnimationClip punchRight;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        stevePosition = transform.Find("MiningPlacingRange");
        headPosition = transform.Find("Head");
        anim = GetComponent<Animator>();
        playerControllerScript = GameObject.Find("SteveContainer").transform.GetComponent<PlayerControllerScript>();

        foreach(AnimationClip clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals("punch"))
            {
                punchRight = clip;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        mineBlock();
        highlightBlock();

        if(hoveringOverBlock != null) // if the mouse is hovering over a block
        {
            if (Input.GetMouseButtonDown(1)) // if right click
            {
                hoveringOverBlock.GetComponent<BlockScript>().rightClick(); // call right click method on the block
            }
        }
	}

    /**
     * Highlights the block that the mouse is hovering over, but only if the block is:
     *      a) within the players range to place/break blocks
     *      b) reachable from the player, i.e. not behind another block
     *      c) player is not in the UI, e.g. in inventory
     */
    private void highlightBlock()
    {
		GameObject highlightedBlock = getHoveredBlock(); // block that is being hovered over
	    // if not hovering over any block or block not within range or (block is fallType and is falling)
		if (highlightedBlock == null || !isBlockWithinRange(highlightedBlock.transform) || !isBlockReachable(highlightedBlock) || InventoryScript.getIsInUI() || (highlightedBlock.gameObject.tag.Equals("FallType") && highlightedBlock.gameObject.GetComponent<FallScript>().isFallingDown())) 
		{
            removeHighlighting();
			return;
		}
        if (ReferenceEquals(highlightedBlock, hoveringOverBlock)) return; // if they are the same object
        Destroy(hoverGrid);

        Vector3 highlightPos = new Vector3(highlightedBlock.transform.position.x + 0.205f, highlightedBlock.transform.position.y + 0.33f, highlightedBlock.transform.position.z - 1);
        hoverGrid = Instantiate(hoverTexture, highlightPos, highlightedBlock.transform.rotation); // create highlight
        hoveringOverBlock = highlightedBlock;
	}
    // removes highlighing of block, if any
    private void removeHighlighting()
    {
		Destroy(hoverGrid);
        hoverGrid = null;
        hoveringOverBlock = null;
	}

    public bool isHoveringOverBlock()
    {
        return hoverGrid != null;
    }

    /**
     * check if we are pressing mousebutton to mine block, then it calls the mine function for the block
     */
    private void mineBlock()
    {

        if (Input.GetMouseButton(0) && !isBreaking)
        {
            GameObject gameObjectToBreak = getHoveredBlock(); // this is the block object to break
            if (gameObjectToBreak != null && hoveringOverBlock == gameObjectToBreak)
            {
                isBreaking = true;
                miningBlock = gameObjectToBreak;
                ToolInstance heldTool = InventoryScript.getHeldTool(); // get the tool that the player is holding
                gameObjectToBreak.GetComponent<BlockScript>().mineBlock(heldTool); 
                doPunchAnimation();
            }



        }
        else if ((isBreaking && Input.GetMouseButtonUp(0)) || (Input.GetMouseButton(0) && (hoveringOverBlock != miningBlock || hoveringOverBlock == null)))
        {
            if (miningBlock != null) 
            {
                miningBlock.GetComponent<BlockScript>().stopMining();
                miningBlock = null;
			}
            isBreaking = false;
            stopPunchAnimation();
        }
	}

    private void doPunchAnimation()
    {
		anim.SetBool("isPunching", true);

		// make hand rotate towards mouse
		// now rotate arm to point towards mouse position
        /*
		Transform arm = transform.Find("Arm Front"); // find player's arm

		Vector3 worldMousePos = getMousePosition();

		Vector3 difference = worldMousePos - arm.position;
		difference.Normalize();

		float rotationZ = (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg) + 90;

        Keyframe[] keys = new Keyframe[3];
        keys[0] = new Keyframe(0, 90);
        keys[1] = new Keyframe(0.1f, 75);
        keys[2] = new Keyframe(0.25f, 90);

        AnimationCurve curve = new AnimationCurve(keys);
        punchRight.SetCurve("", typeof(Transform), "localRotation.z", curve);
        */
	}

    private void stopPunchAnimation()
    {
		anim.SetBool("isPunching", false);
	}

	/** 
     * returns the gameObject that is being hovered over by the mouse.
     * the gameObject must be on the "Default" layer
     */
	private GameObject getHoveredBlock()
    {
		Vector3 worldMousePos = getMousePosition();

		// Create a list to store the results
		List<Collider2D> blockToBreak = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "FrontBackground" or "BackBackground"

		// Check for overlaps
		Physics2D.OverlapCircle(new Vector2(worldMousePos.x, worldMousePos.y), 0.0001f, filter, blockToBreak);
		if (blockToBreak.Count == 0) return null; // mousePosition wasn't on any block

		return blockToBreak[0].gameObject; // this is the game object the mouse is hovering over
	}

    public Vector3 getMousePosition()
    {
		Vector3 mousePos = Input.mousePosition;
		return cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
	}

	/**
     * checks if the block position is within the mining/placing block range (miningRange)
     */
	public bool isBlockWithinRange(Transform block)
    {
        float distance = Vector2.Distance(stevePosition.position, block.position);
        return distance <= miningRange;
    }
	/**
     * checks if a block is reachable from the player, i.e. not behind another block
     * 
     * note: maybe which to raycast
     */
	public bool isBlockReachable(GameObject block)
    {

        SpriteRenderer blockRenderer = block.GetComponent<SpriteRenderer>();
        // if player head is on the same level as the block
        if (!isPlayerAboveBlock(blockRenderer) && !isPlayerBelowBlock(blockRenderer))
        {
            if (isPlayerOnRightSideOfBlock(blockRenderer))
            {
                if (isBlockOnRightSideOfBlock(block)) return false;
            }
            else if (isPlayerOnLeftSideOfBlock(blockRenderer))
            {
                if (isBlockOnLeftSideOfBlock(block)) return false;
            }
        }

        // if player is on left side of the block
        if (isPlayerOnLeftSideOfBlock(blockRenderer))
        {
            if (isPlayerBelowBlock(blockRenderer)) // below block
            {
                if (isBlockOnLeftSideOfBlock(block) && isBlockBelowBlock(block)) return false;
                
            }
            if (isPlayerAboveBlock(blockRenderer)) // above block
			{
                if (isBlockOnLeftSideOfBlock(block) && isBlockAboveBlock(block)) return false;
            }
        }

        // if player is on the right side of the block
        if (isPlayerOnRightSideOfBlock(blockRenderer))
        {
            if (isPlayerBelowBlock(blockRenderer)) // below block
            {
                if (isBlockOnRightSideOfBlock(block) && isBlockBelowBlock(block)) return false;
            }
            if (isPlayerAboveBlock(blockRenderer)) // above block
            {
                if (isBlockOnRightSideOfBlock(block) && isBlockAboveBlock(block)) return false;
            }
        }

        // if player is on the same x, i.e. above or below the block
        if(!isPlayerOnRightSideOfBlock(blockRenderer) && !isPlayerOnLeftSideOfBlock(blockRenderer))
        {
            if (isPlayerBelowBlock(blockRenderer)) // below block
            {
                if (isBlockBelowBlock(block)) return false;
            }
            if (isPlayerAboveBlock(blockRenderer)) // above block
            {
                if(isBlockAboveBlock(block)) return false;
            }
        }

        return true;
    }

    public bool isPlayerOnRightSideOfBlock(SpriteRenderer block)
    {
        return headPosition.transform.position.x > block.transform.position.x + block.bounds.size.x/2;

	}

	public bool isPlayerOnLeftSideOfBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.x < block.transform.position.x - block.bounds.size.x/2;

	}

	public bool isPlayerAboveBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.y > block.transform.position.y + block.bounds.size.y/2;

	}

	public bool isPlayerBelowBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.y < block.transform.position.y - block.bounds.size.y/2;

	}
	// checks if there is a block on the right side of GameObject block (right next to it)
    // bool includeBackBackground: true if you want to check if there is a block on the right side with layer: BackBackground or Default/foreground
    //                             if its false then you're only checking if there is a Default layer block next to it
	public bool isBlockOnRightSideOfBlock(GameObject block, bool includeBackBackground = false)
    {
        Vector2 rightBlockPosition = new Vector2(block.transform.position.x + block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
        int mask = LayerMask.GetMask("Default");
        if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		return Physics2D.OverlapCircle(rightBlockPosition, 0.1f, mask);
	}

	public bool isBlockOnLeftSideOfBlock(GameObject block, bool includeBackBackground = false)
	{
		Vector2 leftBlockPosition = new Vector2(block.transform.position.x - block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
		int mask = LayerMask.GetMask("Default");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		return Physics2D.OverlapCircle(leftBlockPosition, 0.1f, mask);
	}
	// checks if there is a block above GameObject block (right up against it)
	public bool isBlockAboveBlock(GameObject block, bool includeBackBackground = false)
	{
		Vector2 aboveBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y + block.GetComponent<SpriteRenderer>().bounds.size.y);
		int mask = LayerMask.GetMask("Default");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		return Physics2D.OverlapCircle(aboveBlockPosition, 0.1f, mask);
	}

	public bool isBlockBelowBlock(GameObject block, bool includeBackBackground = false)
	{
		Vector2 belowBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y - block.GetComponent<SpriteRenderer>().bounds.size.y);
		int mask = LayerMask.GetMask("Default");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		return Physics2D.OverlapCircle(belowBlockPosition, 0.1f, mask);
	}
}
