using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BlockScript : MonoBehaviour
{
    private bool isMining = false;
	private ToolInstance toolBreakingWith = null; // this is the tool that the player is using to break this block
    private GameObject breakAnimationObject;
    private GameObject animationObjectInstance = null;
	private AudioSource breakBlockAudioSource; // the audio souce that plays the sound when the block breaks
	private AudioSource mineBlockAudioSource; // the audio souce that plays the sound while the block is being mined


	private bool animationIsFinishing = false;
    private BreakBehaviour breakBehaviour; // dig sound, speed 
	private ItemDropBehaviour itemDropBehaviour; // what item to drop when broken
	private RightClickBlockBehaviour rightClickBehaviour; // what happens when the player right clicks this block
    private float audioSpeed = 1.15f;
	private Tilemap tilemap;
	private spawnChunkScript scScript;

	private OpenFurnaceScript openFurnaceScript;


	void Awake()
	{
		gameObject.name = gameObject.name.Replace("(Clone)", "").Trim(); // remove (Clone) from object name
		scScript = GameObject.Find("Main Camera").GetComponent<spawnChunkScript>();
		tilemap = GameObject.Find("Grid").transform.Find("Tilemap").GetComponent<Tilemap>();

		breakAnimationObject = Resources.Load("Prefabs\\BreakBlockAnimation") as GameObject;
		// initialize break behaviour
		breakBehaviour = BlockBehaviourData.getBreakBehaviour(gameObject.name);
	}
	// Start is called before the first frame update
	void Start()
    {
		itemDropBehaviour = BlockBehaviourData.getItemDropBehaviour(gameObject.name, transform.position);
		rightClickBehaviour = BlockBehaviourData.getRightClickBehaviour(gameObject, transform.position);

		Transform audioParent = GameObject.Find("Audio").transform;

		mineBlockAudioSource = audioParent.Find("MineBlockSound").GetComponent<AudioSource>();
		mineBlockAudioSource.pitch = audioSpeed;

		breakBlockAudioSource = audioParent.Find("BreakBlockSound").GetComponent<AudioSource>();

		openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();
	}

    // Update is called once per frame
    void Update()
    {
		if (isMining)
		{
			Texture2D spriteTexture = animationObjectInstance.GetComponent<SpriteRenderer>().sprite.texture;

			// play dig sound
			if (!mineBlockAudioSource.isPlaying)
			{
				mineBlockAudioSource.clip = breakBehaviour.getDigSound();
				mineBlockAudioSource.Play();
			}

			// break block when animation finishes
			if (animationIsFinishing && spriteTexture.name == "destroy_stage_0")
			{
				// destroy block and drop item
				breakBlock();
			}
			else if (spriteTexture.name == "destroy_stage_9") animationIsFinishing = true;
		}
	}

	/**
	 * runs when the player right clicks this block
	 */
	public void rightClick()
	{
		if (rightClickBehaviour == null) return;
		rightClickBehaviour.rightClickBlock();
	}

	public bool isRightClickable()
	{
		return rightClickBehaviour != null;
	}

	/**
     * function for mining block 
     * 
     * runs when the user starts mining this block
     * 
     * Tool heldTool: the tool that the player is using to mine this block
     */
	public void mineBlock(ToolInstance heldTool)
    {

        isMining = true;
		toolBreakingWith = heldTool;

		// put break block GameObject (that has the animation) as a child of this block
		animationObjectInstance = Instantiate(breakAnimationObject, gameObject.transform);
		animationObjectInstance.transform.parent = gameObject.transform;
		animationObjectInstance.GetComponent<Animator>().SetFloat("breakingSpeed", breakBehaviour.getBreakingSpeed(heldTool));

	}

    /**
     * runs when the user stops mining this block
     */
    public void stopMining()
    {
        if (!isMining) return;
		if(animationIsFinishing)
		{
			breakBlock();
			return;
		}
        Destroy(animationObjectInstance);
        isMining = false;
    }

	/**
	 * runs when the player breaks this block.
	 * drops an item, plays break sound, and destroys this block.
	 */
	public void breakBlock()
	{
		dropItems();

		SpawningChunkData.updateChunkData(transform.position.x, transform.position.y, 0, LayerMask.LayerToName(gameObject.layer));

		// play break sound
		breakBlockAudioSource.clip = breakBehaviour.getBreakSound();
		breakBlockAudioSource.Play();
		Destroy(gameObject);

		

		// remove durability from the tool that the player was using to break this block
		if(toolBreakingWith != null)
		{
			toolBreakingWith.reduceDurability();
		}

		// make tiles next to this block turn into gameObjects
		//turnTilesToGameObjects();

		// check if above block is fallType, then make it fall
		checkIfAboveBlockIsFallType();
		checkIfAboveIsNoFloatType(); // check if the block above is cactus, then make it break instantly

		// if this block was a furnace, then we need to drop the items that were in the furnace
		if (gameObject.name.Equals("Furnace")) openFurnaceScript.removeFurnace(transform.position);

		createBackgroundVisualBlock();
	}

	// runs when the block is broken, drops items from the block
	private void dropItems()
	{
		List<GameObject> itemsToDrop;
		itemsToDrop = itemDropBehaviour.dropItem(gameObject.name, toolBreakingWith, transform.position); // get items to drop
		if (itemsToDrop != null)
		{
			foreach (GameObject item in itemsToDrop)
			{
				Instantiate(item, transform.position, transform.rotation); // spawn item
			}

		}
	}

	/**
	 * creates a background visual block at this blocks position if there doesnt already exist one
	 */
	public void createBackgroundVisualBlock()
	{
		float vLineHeight = SpawningChunkData.getVerticalLineHeight(transform.position.x);
		// check if this block is below the surface level, then we need to display a background block
		if (vLineHeight >= transform.position.y)
		{
			int backgroundBlockID = 3;
			// if this block is a dirt block && it is at the top block position, then display a grass block as a background block
			if (gameObject.name.Equals("Dirt") && vLineHeight == transform.position.y)
			{
				if(gameObject.GetComponent<SpriteRenderer>().sprite.name.Equals("GrassBlock")) backgroundBlockID = 2;
				else if (gameObject.GetComponent<SpriteRenderer>().sprite.name.Equals("SnowyGrassBlock")) backgroundBlockID = 28;
			}
			else backgroundBlockID = BlockHashtable.getBackgroundVisualBlock(gameObject.name);
			bool added = SpawningChunkData.addBackgroundVisualBlock(transform.position.x, transform.position.y, backgroundBlockID); // add the background block to the data
			if (added) scScript.instantiateTile(backgroundBlockID, transform.position.x, transform.position.y, true); // instantiate the tile
		}
	}

	private void turnAboveTileToGameObject()
	{
		Vector3Int aboveBlockPos = new Vector3Int((int)(transform.position.x - .5f), (int)(transform.position.y - .5f) + 1);
		if (tilemap.HasTile(aboveBlockPos)) // if there is a tile above this block
		{
			TileBase tile = tilemap.GetTile(aboveBlockPos);
			scScript.spawnGameObjectInsteadOfTile(tile, aboveBlockPos); // place gameObject at tiles position
			tilemap.SetTile(aboveBlockPos, null); // remove tile
		}
	}


	public void checkIfAboveBlockIsFallType()
	{
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask("Tilemap");
		contactFilter.useLayerMask = true;

		GameObject aboveBlock = getAboveBlock(contactFilter);
		if(aboveBlock != null)
		{
			// if above block is a fall type
			if (FrontBackgroundBlocks.isFallType((tilemap.GetTile(tilemap.WorldToCell(new Vector2(transform.position.x, transform.position.y + 1))) as Tile)?.sprite.name))
			{
				turnAboveTileToGameObject();

			}
		}

		contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask("Default");
		contactFilter.useLayerMask = true;

		aboveBlock = getAboveBlock(contactFilter);
		if (aboveBlock != null)
		{
			// if above block is a fall type
			if (aboveBlock.tag == "FallType")
			{
				aboveBlock.GetComponent<FallScript>().fall();
			}
		}
	}

	public void checkIfAboveIsNoFloatType()
	{
		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("FrontBackground");
		contactFilter.useLayerMask = true;

		List<Collider2D> aboveBlock = getAllAboveColliders(contactFilter);

		foreach (Collider2D c in aboveBlock)
		{
			if (c.gameObject.tag.Equals("NoFloatType")) c.gameObject.GetComponent<BlockScript>().breakBlock();
		}
	}

	private GameObject getAboveBlock(ContactFilter2D contactFilter)
	{
		Collider2D[] results = new Collider2D[1];

		int count = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y + 1), 0.05f, contactFilter, results);

		if (count > 0) return results[0].gameObject;
		return null;
	}

	private List<Collider2D> getAllAboveColliders(ContactFilter2D contactFilter)
	{
		List<Collider2D> results = new List<Collider2D>();

		int count = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y + 1), 0.05f, contactFilter, results);

		return results;
	}
}
