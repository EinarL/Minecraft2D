using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Collections.AllocatorManager;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;


/**
 * 2D placing/breaking block mechanics:
 * Breaking blocks:
 * hold down left click to break blocks, doesn´t matter which layer theyre on
 * you will only be able to break background blocks that are not a part of the world generation (except maybe villages if we create those)
 * Placing blocks:
 * right click in order to place blocks on the foreground layer
 * hold right click in order to place blocks on the background layer
 */
public class PlaceBlockScript : MonoBehaviour
{
    private bool holdingItemIsPlaceable = false;
    private GameObject holdingItem; // the item that the player is holding
    private BreakBlockScript breakBlockScript;
    public Tilemap backgroundVisualTiles;
    private AudioSource placeBlockAudioSource;
    private Animator anim;

    private RightClickItemBehaviour rightClickItemBehaviour = null;
    private bool isHoldingRightClick = false;

    private GameObject hoverGrid = null; // this is the grid that gets placed where mouse is
    private Vector2 hoveringOverPosition; // the position that the mouse is hovering over, rounded to a potential block position
    private GameObject hoverTexture; // this is a texture for the hoverGrid
    private bool didPress = false; // true if the player pressed the right click button on the previous frame
    private bool canPlaceAgain = true; // this needed so that the player cant continue holding right click to continue placing more blocks
    private float holdRightClickTimer = 0f;
    private float holdDownThreshold = 0.2f; // hold right click for this many seconds to place on background layer
    private float placingRange = 6f;
    private Transform head; // steve's head
    private Transform torso;

	void Awake()
	{
		BlockBehaviourData.initializeHashtables();
	}

	// Start is called before the first frame update
	void Start()
    {
		breakBlockScript = GetComponent<BreakBlockScript>();
        hoverTexture = breakBlockScript.hoverTexture;
        head = transform.Find("Head").transform;
        torso = transform.Find("Torso").transform;
        anim = GetComponent<Animator>();
        backgroundVisualTiles = GameObject.Find("Grid").transform.Find("BackgroundVisualTiles").GetComponent<Tilemap>();
        placeBlockAudioSource = GameObject.Find("Audio").transform.Find("BreakBlockSound").GetComponent<AudioSource>();
	}

    // Update is called once per frame
    void Update()
    {
        if (rightClickItemBehaviour != null)
        {
            if (Input.GetMouseButton(1) && !isHoldingRightClick) // begin holding/clicking right click
            {
                rightClickItemBehaviour.rightClickItem();
                isHoldingRightClick = true;
            }
            else if (Input.GetMouseButtonUp(1) && isHoldingRightClick) // stop holding right click
            {
                rightClickItemBehaviour.stopHoldingRightClick();
                isHoldingRightClick = false;
            }
        }
        else isHoldingRightClick = false;

        if (holdingItemIsPlaceable)
        {
            checkIfWeCanDisplayHoverTexture();
		}

        if(hoverGrid != null) // if we can place a block
        {
            if (Input.GetMouseButton(1) && canPlaceAgain) // hold down right click
            {
                didPress = true;
                holdRightClickTimer += Time.deltaTime;
                if(holdRightClickTimer > holdDownThreshold)
                {
                    didPress = false;
                    canPlaceAgain = false;
					holdRightClickTimer = 0;
                    // if the block to be placed is a NoFloatType && we can only place the float type block in the background layer
                    if (canPlaceInBackBackground())
                    {
						if (holdingItem.tag.Equals("NoFloatType"))
						{
							if (canPlaceNoFloatTypeInBackground()) placeBlockInBackground(); // place block in background layer
						}
						else
						{
							placeBlockInBackground(); // place block in background layer
						}
					}

				}
            }
            else if (didPress && Input.GetMouseButtonUp(1)) // single click
            {
                didPress = false;
                holdRightClickTimer = 0;

                // check if the hovered block is right clickable, then dont place block
                if (breakBlockScript.isHoveredBlockRightClickable() && !Input.GetKey(KeyCode.LeftControl)) return;
                
				// if the block to be placed is a NoFloatType && we can only place the float type block in the foreground
				if (holdingItem.tag.Equals("NoFloatType"))
                {
                    // returns true if the block below is in the default or frontBackground layer
					if (breakBlockScript.isBlockBelowBlock(holdingItem.transform.position, false, true)) placeBlockInForeground(); // place block in foreground layer
                }
                else
                {
					placeBlockInForeground(); // place block in foreground layer
				}
                
            }
            else if(!Input.GetMouseButton(1))
            {
                canPlaceAgain = true;
				holdRightClickTimer = 0;
			}
        }

    }

    private void makePlaceBlockSound()
    {
        AudioClip placeBlockAudio = BlockHashtable.getBlockPlacingAudio(holdingItem.name);
        if(placeBlockAudio != null)
        {
			placeBlockAudioSource.clip = placeBlockAudio;
			placeBlockAudioSource.Play();
		}
    }

    /**
     * places the block that the player is holding on the foreground/Default layer
     */
    private void placeBlockInForeground()
    {
        List<GameObject> placedBlocks = placeBlock();

        foreach(GameObject block in placedBlocks)
        {
			// here we need to check if placedBlock is a special type of block which goes on the FrontBackground Layer
			if (FrontBackgroundBlocks.isFrontBackgroundBlock(block.name)) // if its a "front background" block
			{
				block.layer = LayerMask.NameToLayer("FrontBackground");
			}
            else // remove water if there is any at this position
            {
                removeWater(block);
				if(block.name.StartsWith("Water")) block.GetComponent<WaterScript>().startFlowing();
			}

			// update the chunkData
			if (block != null) SpawningChunkData.updateChunkData(block.transform.position.x, block.transform.position.y, BlockHashtable.getIDByBlockName(block.name), LayerMask.LayerToName(block.layer));


			if (block.tag.Equals("FallType"))
			{
				// we need FallScript to execute its Start() function before we call fall()
				IEnumerator executeAfterStart(FallScript fallScript)
				{
					yield return null;
					fallScript.fall();
				}

				StartCoroutine(executeAfterStart(block.GetComponent<FallScript>()));
			}
		}
	}

	/**
     * places the block that the player is holding on the BackBackground layer
     */
	private void placeBlockInBackground()
	{
        if (holdingItem.name.Equals("Water")) return; // cant place water in the background
		List<GameObject> placedBlocks = placeBlock();

		foreach (GameObject block in placedBlocks)
        {
			// here we need to check if placedBlock is a special type of block which goes on the FrontBackground Layer
			if (FrontBackgroundBlocks.isFrontBackgroundBlock(block.name)) // if its a "front background" block
			{
				block.layer = LayerMask.NameToLayer("FrontBackground");
			}
			else // else place on BackBackground layer
			{
				block.layer = LayerMask.NameToLayer("BackBackground");

				SpriteRenderer blockRenderer = block.GetComponent<SpriteRenderer>();
				blockRenderer.color = new Color(170f / 255f, 170f / 255f, 170f / 255f); // dark tint
				blockRenderer.sortingOrder = -10;

			}

			// update the chunkData
			SpawningChunkData.updateChunkData(block.transform.position.x, block.transform.position.y, BlockHashtable.getIDByBlockName(block.name), LayerMask.LayerToName(block.layer));


			if (block.tag.Equals("FallType"))
			{
				block.GetComponent<FallScript>().fall();
			}

		}

	}
	//helper function that both placeBlockInForeground() and placeBlockInBackground() use
	private List<GameObject> placeBlock()
    {
		makePlaceBlockSound();
		PlaceBlockBehaviour pbBehaviour = BlockHashtable.getPlaceBlockBehaviour(holdingItem.name);
		List<GameObject> placedBlocks;
        if (pbBehaviour != null)
        {
            placedBlocks = pbBehaviour.placeBlock(holdingItem, this, breakBlockScript);

            if (placedBlocks == null) placedBlocks = new List<GameObject>() { Instantiate(holdingItem, hoveringOverPosition, Quaternion.identity) }; // place block

		}
		else placedBlocks = new List<GameObject>() { Instantiate(holdingItem, hoveringOverPosition, Quaternion.identity) }; // place block
		InventoryScript.decrementSlot(InventoryScript.getSelectedSlot()); // remove the block from the inventory
        if (placedBlocks[0].name.StartsWith("Water"))
        {
            InventoryScript.setSelectedSlotItem(new InventorySlot("Bucket"));
        }

		playPlaceBlockAnimation();

		foreach (GameObject block in placedBlocks)
        {
			block.name = block.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
		}


        return placedBlocks;
	}

    private void playPlaceBlockAnimation()
    {
		bool facingRight = anim.GetBool("isFacingRight");

		if (facingRight) anim.Play("fightFrontArm");
		else anim.Play("fightBackArm");
	}

	private void createHighlight(Vector2 position)
    {
        if (hoverGrid != null) return;
        Vector2 highlightPos = new Vector3(position.x, position.y);
        hoverGrid = Instantiate(hoverTexture, highlightPos, Quaternion.identity);
        hoveringOverPosition = position;

    }

    private void removeHighlight()
    {
        Destroy(hoverGrid);
        hoverGrid = null;
    }
	/**
     * the player can place in the BackBackground if the player can place the block in Default/foreground layer, and
     * there is not a BackBackground block already in this spot.
     * the function checkIfWeCanDisplayHoverTexture() checks if we can place a block on the Default/foreground layer,
     * so we should use that function and this function to check if the block can be placed on the FrontBackground layer
     * 
     * this function returns true if there is not a block in the BackBackground in the spot where the hoverGrid is, otherwise false
     */
	private bool canPlaceInBackBackground()
    {
        return !checkIfBlockInPosition(true, false);

	}

    /**
     * this gets called when the player is holding a different item in his hand
     * 
     * checks if the player is holding a placeable item by checking if the item exists in the folder "Blocks"
     * if its placeable, then we create a hoverTexture where the mouse is if we can place the block in that place.
     */
    public void checkIfHoldingPlaceableItem(string itemName)
    {
        // special case if the player is holding a bed. because there isnt a bed block, but rather a bedUpper and bedLower blocks (similar for doors)
        if(itemName.Equals("Bed")) holdingItem = Resources.Load<GameObject>("Prefabs\\Blocks\\BedUpperLeft");
		else if (itemName.StartsWith("Door")) holdingItem = Resources.Load<GameObject>("Prefabs\\Blocks\\Door" + itemName.Replace("Door", "") + "TopRight");
        else if (itemName.Equals("WaterBucket")) holdingItem = Resources.Load<GameObject>("Prefabs\\Blocks\\Water");
		else holdingItem = Resources.Load<GameObject>("Prefabs\\Blocks\\" +  itemName);

        if (rightClickItemBehaviour != null) rightClickItemBehaviour.stopHoldingRightClick(false); // previously held item might have e.g. been a bow and if we are holding right click while switching items, then we call this function
        if (holdingItem == null) // didn't find the block, so the block isn't placeable
		{
            // the block/item might have a rightClickItemBehaviour though so we have to check that
            rightClickItemBehaviour = BlockBehaviourData.getRightClickItemBehaviour(itemName);

            holdingItemIsPlaceable = false;
            removeHighlight();
			return;
        }
        holdingItemIsPlaceable = true;
        rightClickItemBehaviour = null;
	}

    // checks if we can place the block on the Default/foreground layer, if so it calls createHighlight()
    private void checkIfWeCanDisplayHoverTexture()
    {
        if (InventoryScript.getIsInUI()) // if user is in UI, then we cant place a block
		{
            removeHighlight();
            return; 
        }
        Vector2 mousePos = breakBlockScript.getMousePosition();
        Vector2 RoundedMousePos = new Vector2((float)Math.Round(mousePos.x + 0.5f) - 0.5f, (float)Math.Round(mousePos.y + 0.5f) - 0.5f); // round it to the closes possible "block position"

        //if (RoundedMousePos == hoveringOverPosition && transform.position == prevPosition) return; // if have not moved mouse nor player, we know we dont need to create a new hoverTexture
		removeHighlight();
		holdingItem.transform.position = RoundedMousePos;
        if (holdingItem.name.Equals("BedUpperLeft")) // special case where the block takes up more that one-block-space
        {
            if (checkIfLongBlockPlaceable(holdingItem))
            {
                createHighlight(RoundedMousePos);
            }
            return;
        }

		if (holdingItem.name.StartsWith("Door")) // special case where the block takes up also the space above it
		{
			if (checkIfTallBlockPlaceable(holdingItem))
			{
				createHighlight(RoundedMousePos);
			}
			return;
		}

		if (checkIfPlaceable(holdingItem)) // if its placeable, then display the hoverTexture
        {
            // if its a no float type then there must be a block below in order to place it
            if (holdingItem.tag.Equals("NoFloatType"))
            {
                if (breakBlockScript.isBlockBelowBlock(holdingItem.transform.position, true, true)) createHighlight(RoundedMousePos);
			}
			else createHighlight(RoundedMousePos);
        }
    }

	/*
     * bool includeBackBackground: true if the block should not be placeable if there is a BackBackground block in this position
     * bool includeEntity: true if the block should not be placeable if there is an entity in this position
     * returns true if there is a block in this position with the corresponding layer, otherwise false
     */
	private bool checkIfBlockInPosition(bool includeBackBackground = false, bool includeEntity = true, bool includePlayer = true)
    {
		// Create a collision filter to only include colliders in the default layer
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default"));
        if (includePlayer) filter.SetLayerMask(filter.layerMask | LayerMask.GetMask("Player"));
		if (includeBackBackground) filter.SetLayerMask(filter.layerMask | LayerMask.GetMask("BackBackground"));
		if (includeEntity) filter.SetLayerMask(filter.layerMask | LayerMask.GetMask("Entity"));

		// if the item is a "Front Background" type, then we cant place it if there already is a block in the FrontBackground in this spot
		if (FrontBackgroundBlocks.isFrontBackgroundBlock(holdingItem.name)) filter.SetLayerMask(filter.layerMask | LayerMask.GetMask("FrontBackground"));
		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(holdingItem.transform.position, 0.45f, filter, results);

		foreach (Collider2D coll in results)
		{
			// if the collider isn't the inactiveBlock, then there is already a block here and we cant display the hover texture
			if (!ReferenceEquals(holdingItem, coll.gameObject)) return true;
		}
        return false;
	}
    /**
     * returns true if a block in the contactFilter's layer is in holdingItem's (the position of the block where the cursor is) position.
     */
	public bool checkIfBlockInPosition(ContactFilter2D filter)
	{
		// if the item is a "Front Background" type, then we cant place it if there already is a block in the FrontBackground in this spot
		if (FrontBackgroundBlocks.isFrontBackgroundBlock(holdingItem.name)) filter.SetLayerMask(filter.layerMask | LayerMask.GetMask("FrontBackground"));
		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(holdingItem.transform.position, 0.45f, filter, results);

		foreach (Collider2D coll in results)
		{
			// if the collider isn't the inactiveBlock, then there is already a block here and we cant display the hover texture
			if (!ReferenceEquals(holdingItem, coll.gameObject)) return true;
		}
		return false;
	}
	/**
     * returns true if the block is placeable in futureBlockPos.
     * returns true if:
     *  futureBlockPos is within 7 blocks of the player &&
     *  you cast a raycast towards the cursor and if the raycast can get there without hitting a block on the default layer &&
     *  there is not already a block in this position on the default layer &&
     *  (there is a block in the backgroundVisualLayer or background layer at futureBlockPos || there is a block next to futureBlockPos)
     * 
     */
	private bool checkIfPlaceable(GameObject futureBlockPos)
    {
        // cast a ray from the players head and torso to check if the ray can get to the block's position
        bool raycastSuccess = raycast(head.transform.position, futureBlockPos.transform.position) || raycast(torso.transform.position, futureBlockPos.transform.position) || raycast(new Vector2(head.transform.position.x, head.transform.position.y + 1.5f), futureBlockPos.transform.position);
        if(!raycastSuccess) return false;
        if (futureBlockPos.layer == 7) // if it's a frontBackground block
        {
			if (checkIfBlockInPosition(false, false, false)) return false;
		}
        else if (checkIfBlockInPosition()) return false;

		// Create a collision filter to only include colliders in the following layers
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("BackBackground") | LayerMask.GetMask("BackgroundVisual"));

		if (checkIfBlockInPosition(filter) || backgroundVisualTiles.HasTile(backgroundVisualTiles.WorldToCell(futureBlockPos.transform.position))) return true;
        if (isBlockNextToBlock(futureBlockPos)) return true;
        return false;
	}

    private bool checkIfLongBlockPlaceable(GameObject futureBlockPos)
    {
        if (!checkIfPlaceable(futureBlockPos)) return false;
        bool placeLeft = head.position.x > futureBlockPos.transform.position.x; // if this is true then we need to rotate the bed right when placing it

        // if were placing the bed on the left side of the player
        if (placeLeft)
        {
            // if there is a block on the left side of futureBlockPos (i.e. no space for the whole bed in this position)
			if(breakBlockScript.isBlockOnLeftSideOfBlock(futureBlockPos.transform.position, false, true)) return false;
            return true;
		}
		else // if were placing the bed on the right side of the player
		{
			// if there is a block on the right side of futureBlockPos (i.e. no space for the whole bed in this position)
			if (breakBlockScript.isBlockOnRightSideOfBlock(futureBlockPos.transform.position, false, true)) return false;
			return true;
		}
    }

	private bool checkIfTallBlockPlaceable(GameObject futureBlockPos)
	{
		if (!checkIfPlaceable(futureBlockPos)) return false;
		// if there is a block above futureBlockPos (i.e. no space for the whole door in this position)
		if (breakBlockScript.isBlockAboveBlock(futureBlockPos.transform.position, false, true)) return false;
		return true;
	}


	private bool isBlockNextToBlock(GameObject futureBlockPos)
    {
        // if its a front background block then we dont check if there is a block above, because torches, grass, flowers, etc. cant float below a block
        if(!FrontBackgroundBlocks.isFrontBackgroundBlock(holdingItem.name) && breakBlockScript.isBlockAboveBlock(futureBlockPos.transform.position, true, true)) return true;
        if(breakBlockScript.isBlockOnRightSideOfBlock(futureBlockPos.transform.position, true, true)) return true;
        if(!FrontBackgroundBlocks.isWallBlock(holdingItem.name) && breakBlockScript.isBlockBelowBlock(futureBlockPos.transform.position, true, true)) return true; // if its a wall block then we dont check if there is a block below it (we also dont check above it becuase wallblocks are also frontbackgroundblocks)
        if(breakBlockScript.isBlockOnLeftSideOfBlock(futureBlockPos.transform.position, true, true)) return true;
        return false;
    }
    /**
     * returns true if the raycast can get from start to end without hitting a block on the default layer
     */
    private bool raycast(Vector2 start, Vector2 end) 
    {

		// Calculate direction and distance
		Vector2 direction = end - start;
		float distance = Vector2.Distance(start, end);
		if (distance > placingRange) return false; // if its out of the placing range
        distance = Mathf.Min(distance, placingRange);

		// Cast the ray
		RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));

        return hit.collider == null;
	}

	/**
     * returns true if one of these conditions is true:
     *   a) there is a block below which is in the BackBackground layer
     *   b) there is a block below which is in the Default layer && the block below is not a NoFloatType
     */
	private bool canPlaceNoFloatTypeInBackground()
    {
		Vector2 belowBlockPosition = new Vector2(holdingItem.transform.position.x, holdingItem.transform.position.y - holdingItem.GetComponent<SpriteRenderer>().bounds.size.y);
		
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("Tilemap");
		contactFilter.useLayerMask = true;
		
        Collider2D[] result = new Collider2D[1];

		Physics2D.OverlapCircle(belowBlockPosition, 0.1f, contactFilter, result);

        if (result[0] == null) return false;
        if (LayerMask.LayerToName(result[0].gameObject.layer).Equals("BackBackground")) return true;
        if (LayerMask.LayerToName(result[0].gameObject.layer).Equals("Default") && !result[0].gameObject.tag.Equals("NoFloatType")) return true;
        return false;
	}

    // removes water if there is any at the cursors position
	private void removeWater(GameObject ignoredBlock)
    {
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(hoveringOverPosition, 0.45f, filter, results);
        if (results.Count > 0)
        {
            foreach (Collider2D col in results)
            {
                if(!ReferenceEquals(col.gameObject, ignoredBlock))
                {
					Destroy(results[0].gameObject);
                    deflowWater();
				}
            }

		}
		
	}

	// after placing a block on water, this function runs to make the water around it deflow
	private void deflowWater()
	{
		WaterScript belowWater = getWaterAtPosition(new Vector2(hoveringOverPosition.x, hoveringOverPosition.y - 1));
		if (belowWater != null) belowWater.startDeflowing();
		WaterScript leftWater = getWaterAtPosition(new Vector2(hoveringOverPosition.x - 1, hoveringOverPosition.y));
		if (leftWater != null) leftWater.startDeflowing();
		WaterScript rightWater = getWaterAtPosition(new Vector2(hoveringOverPosition.x + 1, hoveringOverPosition.y));
		if (rightWater != null) rightWater.startDeflowing();

        // if we placed a block below water then the water above the block might have to flow
        WaterScript aboveWater = getWaterAtPosition(new Vector2(hoveringOverPosition.x, hoveringOverPosition.y + 1));
		if (aboveWater != null) aboveWater.startFlowing();
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
		if (results.Count > 1) Debug.LogError("PlaceBlockScript: Found more than one water blocks at position: " + pos);
		return results[0].gameObject.GetComponent<WaterScript>();
	}
}
