using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakBlockScript : MonoBehaviour
{

	private Camera cam;
	public GameObject hoverTexture; // hovering over block texture
	public Tilemap tilemap;
	private spawnChunkScript scScript;
	private Transform stevePosition; // get distance from this transform to the blockPos to check if the distance in within range of mining/placing blocks
	private Transform headPosition;
	private Animator anim;
	private GameObject hoverGrid = null;
	private object hoveringOverBlock = null; // the block the mouse is hovering over, this can either be a Tile or a GameObject
	private Vector2 prevTilePosition; // the previously hovered over tile or gameobject world position
	private GameObject miningBlock = null; // the block that we are mining
	private float miningRange = 6;
	private bool isBreaking = false; // is in the process of breaking a block

	private Transform head; // steve's head
	private Transform torso;

	private HashSet<string> highestPriority = new HashSet<string>(){ "Furnace", "CraftingTable", "Torch", "TorchWall", "TorchLeft", "TorchRight", "Ladder", "LadderLeft", "LadderRight" }; // these blocks all have equal highest priority over other blocks
	private HashSet<string> lowestPriority = new HashSet<string>(){ "SnowBlockThin" };


	// Start is called before the first frame update
	void Start()
	{
		cam = Camera.main;
		stevePosition = transform.Find("MiningPlacingRange");
		headPosition = transform.Find("Head");
		anim = GetComponent<Animator>();
		scScript = GameObject.Find("Main Camera").GetComponent<spawnChunkScript>();
		head = transform.Find("Head").transform;
		torso = transform.Find("Torso").transform;
	}

	// Update is called once per frame
	void Update()
	{
		if (isBreaking || hoverGrid != null) mineBlock();
		highlightBlock();

		if (hoveringOverBlock != null) // if the mouse is hovering over a block
		{

			if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl)) // if right click
			{
				if (hoveringOverBlock is GameObject) ((GameObject)hoveringOverBlock).GetComponent<BlockScript>().rightClick(); // call right click method on the block
				else if (hoveringOverBlock != null) // if its a tile
				{
					spawnGameObjectInsteadOfTile(new Vector3Int((int)(prevTilePosition.x - .5f), (int)(prevTilePosition.y - .5f)));
					IEnumerator rightClickCoroutine()
					{
						yield return new WaitForEndOfFrame();
						if (getHoveredBlock() != null && getHoveredBlock() is GameObject) ((GameObject)getHoveredBlock()).GetComponent<BlockScript>().rightClick();
					}
					StartCoroutine(rightClickCoroutine());
				}

			}
		}
	}

	/**
     * Highlights the block that the mouse is hovering over, but only if the block is:
     *      a) within the players range to place/break blocks
     *      b) reachable from the player, i.e. not behind another block
     *      c) player is not in the UI, e.g. in inventory, nor sleeping
     */
	private void highlightBlock()
	{
		object highlightedBlock = getHoveredBlock(); // block that is being hovered over
		if (highlightedBlock is Tile)
		{
			Tile highlightedTile = highlightedBlock as Tile;
			Vector2 tilePos = getRoundedMousePosition();
			if(highlightedTile.name.StartsWith("Door")) tilePos = getMousePosition();
			// if not hovering over any block or block not within range nor reachable
			if (highlightedTile == null || !isBlockWithinRange(tilePos) || !isTileReachable(tilePos) || InventoryScript.getIsInUI() || anim.GetBool("isSleeping"))
			{
				removeHighlighting();
				return;
			}
			if (tilePos == prevTilePosition) return;
			Destroy(hoverGrid);
			prevTilePosition = tilePos;
			hoverGrid = Instantiate(hoverTexture, getRoundedMousePosition(), Quaternion.identity); // create highlight
			hoveringOverBlock = highlightedTile;
		}
		else
		{
			GameObject highlightedGameObject = highlightedBlock as GameObject;
			// if not hovering over any block or block not within range or (block is fallType and is falling)
			if (highlightedGameObject == null || !isBlockWithinRange(highlightedGameObject.transform.position) || !isBlockReachable(highlightedGameObject) || InventoryScript.getIsInUI() || anim.GetBool("isSleeping") || (highlightedGameObject.gameObject.tag.Equals("FallType") && highlightedGameObject.gameObject.GetComponent<FallScript>().isFallingDown()))
			{
				removeHighlighting();
				return;
			}
			if (ReferenceEquals(highlightedGameObject, hoveringOverBlock)) return; // if they are the same object
			Destroy(hoverGrid);
			prevTilePosition = getRoundedMousePosition();
			Vector3 highlightPos = new Vector3(highlightedGameObject.transform.position.x, highlightedGameObject.transform.position.y, highlightedGameObject.transform.position.z - 1);
			hoverGrid = Instantiate(hoverTexture, highlightPos, highlightedGameObject.transform.rotation); // create highlight
			hoveringOverBlock = highlightedGameObject;
		}

	}
	// removes highlighing of block, if any
	private void removeHighlighting()
	{
		Destroy(hoverGrid);
		hoverGrid = null;
		hoveringOverBlock = null;
		prevTilePosition = new Vector2(-999f, -999f);
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
			object blockToBreak = getHoveredBlock(); // this is the block gameObject/tile to break
			GameObject gameObjectToBreak = null;
			if (blockToBreak is GameObject) gameObjectToBreak = (GameObject)blockToBreak;
			else if (blockToBreak != null)// if blockToBreak is a tile
			{
				Vector3Int blockPos = new Vector3Int((int)(prevTilePosition.x - .5f), (int)(prevTilePosition.y - .5f));
				gameObjectToBreak = spawnGameObjectInsteadOfTile(blockPos);
				if (gameObjectToBreak == null) return;
			}

			if (gameObjectToBreak != null) //  && hoveringOverBlock as GameObject == gameObjectToBreak
			{
				isBreaking = true;
				miningBlock = gameObjectToBreak;
				ToolInstance heldTool = InventoryScript.getHeldTool(); // get the tool that the player is holding
				gameObjectToBreak.GetComponent<BlockScript>().mineBlock(heldTool);
				doPunchAnimation();
			}



		}
		else if ((isBreaking && Input.GetMouseButtonUp(0)) || (Input.GetMouseButton(0) && ((hoveringOverBlock is GameObject && ((GameObject)hoveringOverBlock != miningBlock || hoveringOverBlock == null)) || hoveringOverBlock is not GameObject)))
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

	private GameObject spawnGameObjectInsteadOfTile(Vector3Int tilePos)
	{
		TileBase tile = tilemap.GetTile(tilePos);
		if (tile == null) return null;
		scScript.spawnGameObjectInsteadOfTile(tile, tilePos); // place gameObject at tiles position
		tilemap.SetTile(tilePos, null); // remove tile
		return getHoveredBlock() as GameObject; // get the gameObject that just spawned at the tiles position
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
     * returns the gameObject or tile that is being hovered over by the mouse.
     */
	private object getHoveredBlock()
	{
		Vector3 worldMousePos = getRoundedMousePosition();
		// first lets check if there is a tile in the cursors position
		Collider2D[] results = new Collider2D[1];
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Tilemap")); // only blocks on layer "Tilemap"
														   // Check for overlaps
		int count = Physics2D.OverlapCircle(worldMousePos, 0.4f, filter, results);
		if (count > 0)
		{
			return tilemap.GetTile(tilemap.WorldToCell(worldMousePos)); // return a tile
		}


		// Create a list to store the results
		List<Collider2D> blockToBreak = new List<Collider2D>();

		filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "FrontBackground" or "BackBackground"

		// Check for overlaps
		Physics2D.OverlapCircle(worldMousePos, 0.40f, filter, blockToBreak);
		if (blockToBreak.Count == 0) return null; // mousePosition wasn't on any block
		if (blockToBreak.Count > 1) // if hovering over many blocks then here you can define which block has the highest priority to be broken/right-clicked first
		{
			GameObject objectToReturn = null;
			for (int i = 0; i < blockToBreak.Count; i++)
			{
				if (blockToBreak[i].gameObject.name.StartsWith("Door")) return blockToBreak[i].gameObject;
				if (objectToReturn == null && highestPriority.Contains(blockToBreak[i].gameObject.name)) objectToReturn = blockToBreak[i].gameObject;
				if (lowestPriority.Contains(blockToBreak[i].gameObject.name)) return i == 0 ? blockToBreak[1].gameObject : blockToBreak[0].gameObject;
			}
			if (objectToReturn != null) return objectToReturn;
		}

		return blockToBreak[0].gameObject; // this is the game object the mouse is hovering over
	}

	public Vector3 getMousePosition()
	{
		Vector3 mousePos = Input.mousePosition;
		return cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
	}

	public Vector2 getRoundedMousePosition()
	{
		Vector2 mousePos = getMousePosition();
		return new Vector2((float)Math.Round(mousePos.x + 0.5f) - 0.5f, (float)Math.Round(mousePos.y + 0.5f) - 0.5f); // round it to the closes possible "block position"
	}

	/**
     * checks if the block position is within the mining/placing block range (miningRange)
     */
	public bool isBlockWithinRange(Vector2 blockPos)
	{
		float distance = Vector2.Distance(stevePosition.position, blockPos);
		return distance <= miningRange;
	}

	private bool isBlockReachable(GameObject block)
	{
		// if (block.layer == LayerMask.GetMask("Default")) 
		return raycastGameObject(torso.transform.position, block.transform.position, block) || raycastGameObject(head.transform.position, block.transform.position, block) || raycastGameObject(new Vector2(head.transform.position.x, head.transform.position.y + 1f), block.transform.position, block);
		//else return raycastBackgroundGameObject(torso.transform.position, block.transform.position, block) || raycastBackgroundGameObject(head.transform.position, block.transform.position, block) || raycastBackgroundGameObject(new Vector2(head.transform.position.x, head.transform.position.y + 1f), block.transform.position, block);
	}

	/**
     * returns true if the raycast can get from start to end without hitting a block on the default layer
     */
	private bool isTileReachable(Vector2 tileWorldPos)
	{
		return raycast(torso.transform.position, tileWorldPos, tilemap.WorldToCell(tileWorldPos)) || raycast(head.transform.position, tileWorldPos, tilemap.WorldToCell(tileWorldPos)) || raycast(new Vector2(head.transform.position.x, head.transform.position.y + 1f), tileWorldPos, tilemap.WorldToCell(tileWorldPos));
	}

	/**
     * returns true if the raycast can get from start to end without hitting anything other than the tile at tilePos
     */
	private bool raycast(Vector2 start, Vector2 end, Vector3Int tilePos)
	{

		// Calculate direction and distance
		Vector2 direction = end - start;
		float distance = Vector2.Distance(start, end);
		if (distance > miningRange) return false; // if its out of the placing range
		distance = Mathf.Min(distance, miningRange);
		// Cast the ray
		RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
		if (hit.collider != null)
		{
			// Get the cell position of the hit point and check if it matches the target tile position
			Vector3Int hitTilePos = tilemap.WorldToCell(hit.point + direction * 0.01f); // Move slightly into the tile
																						
			return hitTilePos == tilePos;
		}

		return true;
	}

	/**
     * returns true if the raycast can get from start to end and not hit any block except "block"
     */
	private bool raycastGameObject(Vector2 start, Vector2 end, GameObject block)
	{

		// Calculate direction and distance
		Vector2 direction = end - start;
		float distance = Vector2.Distance(start, end);
		if (distance > miningRange) return false; // if its out of the placing range
		distance = Mathf.Min(distance, miningRange);

		// Cast the ray
		RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
		if (hit.collider != null)
		{
			return ReferenceEquals(hit.collider.gameObject, block);
		}

		return true; // hit nothing, return true
	}

	/**
     * returns true if the raycast can get from start to end and hit the block
     */
	private bool raycastBackgroundGameObject(Vector2 start, Vector2 end, GameObject block)
	{
		// Calculate direction and distance
		Vector2 direction = end - start;
		float distance = Vector2.Distance(start, end);
		if (distance > miningRange) return false; // if its out of the placing range
		distance = Mathf.Min(distance, miningRange);

		// Cast the ray
		RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground"));
		bool hitBlock = false;
		foreach (RaycastHit2D hit in hits)
		{
			if (hit.collider != null)
			{
				if (hit.collider.gameObject.layer == LayerMask.GetMask("Default") || hit.collider.gameObject.layer == LayerMask.GetMask("Tilemap")) return false;
				if (ReferenceEquals(hit.collider.gameObject, block)) hitBlock = true;
			}
		}

		return hitBlock;
	}

	public bool isPlayerOnRightSideOfBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.x > block.transform.position.x + block.bounds.size.x / 2;

	}

	public bool isPlayerOnLeftSideOfBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.x < block.transform.position.x - block.bounds.size.x / 2;

	}

	public bool isPlayerAboveBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.y > block.transform.position.y + block.bounds.size.y / 2;

	}

	public bool isPlayerBelowBlock(SpriteRenderer block)
	{
		return headPosition.transform.position.y < block.transform.position.y - block.bounds.size.y / 2;

	}
	// checks if there is a block on the right side of GameObject block (right next to it)
	// bool includeBackBackground: true if you want to check if there is a block on the right side with layer: BackBackground or Default/foreground
	//                             if its false then you're only checking if there is a Default layer block next to it
	public bool isBlockOnRightSideOfBlock(Vector2 blockPos, bool includeBackBackground = false, bool includeFrontBackground = false)
	{
		Vector2 rightBlockPosition = new Vector2(blockPos.x + 1, blockPos.y);
		int mask = LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		if (includeFrontBackground)
		{
			int frontBackgroundMask = LayerMask.GetMask("FrontBackground");
			Collider2D blockHit = Physics2D.OverlapCircle(rightBlockPosition, 0.1f, frontBackgroundMask);
			if (blockHit != null) // if hit block on frontBackground
			{
				blockHit.name = blockHit.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
				if (FrontBackgroundBlocks.isFrontBackgroundBlockPlaceableNextTo(blockHit.name)) return true; // if you can place a block next to this block
			}
		}
		return Physics2D.OverlapCircle(rightBlockPosition, 0.1f, mask);
	}

	public bool isBlockOnLeftSideOfBlock(Vector2 blockPos, bool includeBackBackground = false, bool includeFrontBackground = false)
	{
		Vector2 leftBlockPosition = new Vector2(blockPos.x - 1, blockPos.y);
		int mask = LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		if (includeFrontBackground)
		{
			int frontBackgroundMask = LayerMask.GetMask("FrontBackground");
			Collider2D blockHit = Physics2D.OverlapCircle(leftBlockPosition, 0.1f, frontBackgroundMask);
			if (blockHit != null) // if hit block on frontBackground
			{
				blockHit.name = blockHit.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
				if (FrontBackgroundBlocks.isFrontBackgroundBlockPlaceableNextTo(blockHit.name)) return true; // if you can place a block next to this block
			}
		}
		return Physics2D.OverlapCircle(leftBlockPosition, 0.1f, mask);
	}
	// checks if there is a block above GameObject block (right up against it)
	public bool isBlockAboveBlock(Vector2 blockPos, bool includeBackBackground = false, bool includeFrontBackground = false)
	{
		Vector2 aboveBlockPosition = new Vector2(blockPos.x, blockPos.y + 1);
		int mask = LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		if (includeFrontBackground) mask |= LayerMask.GetMask("FrontBackground"); // add FrontBackground

		return Physics2D.OverlapCircle(aboveBlockPosition, 0.1f, mask);
	}

	public bool isBlockBelowBlock(Vector2 blockPos, bool includeBackBackground = false, bool includeFrontBackground = false)
	{
		Vector2 belowBlockPosition = new Vector2(blockPos.x, blockPos.y - 1);
		int mask = LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap");
		if (includeBackBackground) mask |= LayerMask.GetMask("BackBackground"); // add BackBackground
		if (includeFrontBackground)
		{
			int frontBackgroundMask = LayerMask.GetMask("FrontBackground");
			Collider2D blockHit = Physics2D.OverlapCircle(belowBlockPosition, 0.1f, frontBackgroundMask);
			if (blockHit != null) // if hit block on frontBackground
			{
				blockHit.name = blockHit.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
				if (FrontBackgroundBlocks.isFrontBackgroundBlockPlaceableNextTo(blockHit.name)) return true; // if you can place a block next to this block
			}
		}
		return Physics2D.OverlapCircle(belowBlockPosition, 0.1f, mask);
	}

	public bool isHoveredBlockRightClickable()
	{
		if (hoveringOverBlock == null) return false;
		if (hoveringOverBlock is GameObject) return ((GameObject)hoveringOverBlock).GetComponent<BlockScript>().isRightClickable();
		else if (hoveringOverBlock != null) // if its a tile
		{
			spawnGameObjectInsteadOfTile(new Vector3Int((int)(prevTilePosition.x - .5f), (int)(prevTilePosition.y - .5f)));
			hoveringOverBlock = getHoveredBlock();
			if (hoveringOverBlock is GameObject) return ((GameObject)hoveringOverBlock).GetComponent<BlockScript>().isRightClickable();
		}
		return false;
	}
}