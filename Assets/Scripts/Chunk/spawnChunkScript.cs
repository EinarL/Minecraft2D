using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using static Unity.Collections.AllocatorManager;

public class spawnChunkScript : MonoBehaviour
{
	private static System.Random random = new System.Random();

	private int chunkSize;
    private int amountOfChunksToRender = 4; // amount of chunks to be rendered at the same time
    private float defaultStartSpawnY = -2.5f; // the default height of the vertical line that is to be spawned

    public Sprite grassTexture;
	public Sprite snowyGrassTexture;
	private Camera cam;
    public Tilemap tilemap;
    public Tilemap backgroundVisualTiles;
    public ParticleSystem snowParticleSystem;

	// the snow particle systems that are being rendered, the key is the chunk position
	private Dictionary<int, ParticleSystem> snowParticleSystems = new Dictionary<int, ParticleSystem>(); 


    private int lowestBlockPos = -60;
    private int rendered; // leftmost chunk that is rendered

    private Biome spawnChunkStrategy;
    private List<Biome> biomes = new List<Biome> { new Tundra() }; // new List<Biome> { new Plains(), new Desert(), new Tundra() };

	// how long the biome is (in chunks), this counts down every time a new chunk is rendered and when
	// it hits 0, then a new random chunk gets generated
	private int biomeLength; 

    private OpenFurnaceScript openFurnaceScript;
    private SunLightMovementScript sunLightMovementScript;
    private DayProcessScript dayProcessScript;

	// Start is called before the first frame update
	void Start()
    {
		openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();
		sunLightMovementScript = GameObject.Find("Sun").GetComponent<SunLightMovementScript>();
        dayProcessScript = GameObject.Find("CM vcam").transform.Find("SunAndMoonTexture").GetComponent<DayProcessScript>();

	    BlockHashtable.initializeBlockHashtable();
        spawnChunkStrategy = decideBiome(); // dont do this if the biome is already decided, we need to save which biome was rendering when we quit the game

		cam = Camera.main;
		chunkSize = (int)(SpawningChunkData.blockSize * SpawningChunkData.blocksInChunk);
        SpawningChunkData.setRightMostY(defaultStartSpawnY);
		SpawningChunkData.setLeftMostY(defaultStartSpawnY);
        rendered = -2 * chunkSize;

		// spawn in the chunks at "camera x position": -20, -10, 0, 10
		renderChunk(0);
        renderChunk(-10);
        renderChunk(-20);
        renderChunk(10);
    }

    // Update is called once per frame
    void Update()
    {
        
        int newAmountOfChunksToRender = getAmountOfChunksToRender();
		// if we need to render more chunks (for the camera size change)
		if (amountOfChunksToRender < newAmountOfChunksToRender)
        {
            int diff = Math.Abs(amountOfChunksToRender - newAmountOfChunksToRender);
            for(int i = 1; i <= diff/2; i++)
            {
                renderChunk(rendered - (chunkSize * i)); // spawn left chunk
                renderChunk(rendered + (chunkSize * (amountOfChunksToRender - 1)) + (chunkSize*i)); // spawn right chunk
			}
			amountOfChunksToRender = newAmountOfChunksToRender;
            rendered = getChunkNumber();
        }
        else if(amountOfChunksToRender > newAmountOfChunksToRender) // if we need to render fewer chunks (because of the camera size change)
		{
			int diff = Math.Abs(amountOfChunksToRender - newAmountOfChunksToRender);
            for(int i = 1; i <= diff/2; i++)
            {
                unrenderChunk(rendered); //despawn left chunk
                unrenderChunk(rendered + chunkSize * (amountOfChunksToRender-1)); // despawn right chunk
                rendered += chunkSize;
                amountOfChunksToRender--;
            }
            amountOfChunksToRender = newAmountOfChunksToRender;
            rendered = getChunkNumber();
		}

        // check if we need to render a new chunk (because of movement left or right)
        int leftMostChunkToRender = getChunkNumber();
        if (leftMostChunkToRender != rendered) // if we need to load a chunk
        {
            if (leftMostChunkToRender == rendered + chunkSize) // need to load chunk rendered + 4*chunkSize (rightmost chunk)
			{
				renderChunk(rendered + amountOfChunksToRender * chunkSize);
                unrenderChunk(rendered); // unrender leftmost chunk
			}
			else if (leftMostChunkToRender == rendered - chunkSize) // need to load chunk rendered - chunkSize (leftmost chunk)
            {
				renderChunk(rendered - chunkSize);
                unrenderChunk(rendered + (amountOfChunksToRender-1) * chunkSize); // unrender rightmost chunk
            }
            else
            {
                Debug.LogWarning("Warning! leftMostChunkToRender isn't a chunk away from rendered; rendered = " + rendered + ", leftMostChunkToRender = " + leftMostChunkToRender);
            }
            rendered = leftMostChunkToRender;
        }
    }


    private Biome decideBiome()
    {

        Biome currentBiome = spawnChunkStrategy;

        //if(currentBiome != null) biomes.Remove(currentBiome);

		System.Random rand = new System.Random();
		int randIndex = rand.Next(biomes.Count); // get random index

		Biome newBiome = biomes[randIndex];
        biomeLength = rand.Next(newBiome.biomeLength[0], newBiome.biomeLength[1] + 1);
        
        //if (currentBiome != null) biomes.Add(currentBiome);

        return newBiome; // newBiome
    }

	public int getAmountOfChunksToRender()
	{
		int size = (int)cam.orthographicSize;
		Debug.Assert(size <= 20);

		if (size <= 5) return 4;
		if (size <= 7) return 6;
		if (size <= 11) return 8;
		return 10;
	}

	// gets the x position of the leftmost chunk to be rendered
	int getChunkNumber()
	{
		float closeToChunkNumber = transform.position.x - (chunkSize * amountOfChunksToRender / 2); // round this number to the closest 10
        bool isNegative = closeToChunkNumber < 0;

        float remainder = Math.Abs(closeToChunkNumber) % chunkSize;

        if (remainder < 5)
        {
            if (isNegative) closeToChunkNumber += remainder;
            else closeToChunkNumber -= remainder;
        }
        else
        {
            if (isNegative) closeToChunkNumber = closeToChunkNumber - SpawningChunkData.blocksInChunk + remainder;
            else closeToChunkNumber += SpawningChunkData.blocksInChunk - remainder;
        }

        return (int)closeToChunkNumber;
	}

	/**
     * renders a chunk, from chunkStart to chunkStart + chunkSize
     * chunkStart: the start x position of the chunk
     * fromRight is true when it starts rendering the vertical lines from right to left,
     *            otherwise left to right (this is to calculate the height of the vertical line based
     *            on the height of the vertical line that is next to it)
     *            needs to be true when rendering the leftmost chunk, and false when
     *            rendering the rightmost chunk
     */
	public void renderChunk(int chunkStart)
    {
        ChunkData chunkData;

		bool fromRight = false;
		if (chunkStart < transform.position.x) fromRight = true;

		// if chunk has been rendered, then render the saved chunk
		if (SaveChunk.exists(chunkStart))
        {
			chunkData = SaveChunk.load(chunkStart);
            if (fromRight) SpawningChunkData.prevVerticalLineLeft = getPrevVerticalLineFromChunk(false, chunkData.getChunkData());
            else SpawningChunkData.prevVerticalLineRight = getPrevVerticalLineFromChunk(true, chunkData.getChunkData());
		}
        else
        {
            if (fromRight) // spawning from right to left
			{
				chunkData = spawnChunkStrategy.renderLeftChunk(chunkStart);
			}
            else
            {
				chunkData = spawnChunkStrategy.renderRightChunk(chunkStart);
			}
            biomeLength--;
            if(biomeLength <= 0)
            {
                spawnChunkStrategy = decideBiome();
            }
		}
        sunLightMovementScript.addChunkHeight(chunkData.getVerticalLineHeights());
        SpawningChunkData.addRenderedChunk(chunkData);
		renderSavedChunk(chunkData, !fromRight);
	}

    /**
     * gets the most recent vertical line that was rendered in a right/left chunk
     * 
     */
    private int[] getPrevVerticalLineFromChunk(bool rightChunk, int[,] chunkData)
    {
        int[] vLine = new int[Math.Abs(lowestBlockPos) + 80]; // maxBuildHeight is 80
        for (int i = 0; i < chunkData.GetLength(1); i++)
        {
            vLine[i] = chunkData[rightChunk ? 0 : 9, i];
        }
        return vLine;
    }

    /**
     * Deletes/unrenderes all objects in the given chunk
     * chunkPos: position of the chunk (left side of the chunk)
     */
    public void unrenderChunk(int chunkPos)
    {
		// Create a collision filter to only include colliders in these layers
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("BackgroundVisual") | LayerMask.GetMask("Entity") | LayerMask.GetMask("Item"));


		List<Collider2D> results = getCollidersWithinChunk(chunkPos, filter);

        List<object[]> entities = new List<object[]>();

        foreach (Collider2D collider in results)
        {
            // add entites in this chunk to the list
            if (collider.gameObject.layer == 10)
            {
                entities.Add(new object[] { collider.gameObject.transform.position.x, collider.gameObject.transform.position.y, collider.gameObject.name });
            }
            Destroy(collider.gameObject);
        }
        if (snowParticleSystems.ContainsKey(chunkPos)) // destroy the snow particle system if there is one in this chunk
        {
            Destroy(snowParticleSystems[chunkPos]);
            snowParticleSystems.Remove(chunkPos);
        }

        SpawningChunkData.overwriteEntities(chunkPos, entities); // save entities
		openFurnaceScript.saveFurnaces(); // save furnaces
        ChunkData chunkToRemove = SpawningChunkData.getChunkByChunkPos(chunkPos);
		if (chunkToRemove != null) sunLightMovementScript.removeChunkHeight(chunkToRemove.getVerticalLineHeights()); // for sun position adjustment

		// TODO: implement so it saves dropped item also (maybe not tho?)
		SpawningChunkData.removeAndSaveChunkByChunkPosition(chunkPos); // save

        // remove tiles
        removeTilesInChunk(chunkPos);

	}

    private List<Collider2D> getCollidersWithinChunk(int chunkPos, ContactFilter2D filter)
    {
		Vector2 center = new Vector2(chunkPos + chunkSize / 2, (SpawningChunkData.maxBuildHeight + lowestBlockPos) / 2);
		Vector2 size = new Vector2(chunkSize - SpawningChunkData.blockSize, SpawningChunkData.maxBuildHeight + Math.Abs(lowestBlockPos));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapBox(center, size, 0f, filter, results);

        return results;
	}
    /**
     * loads a chunk that has been saved. loads the chunk from left to right.
     */
    void renderSavedChunk(ChunkData chunkData, bool goingRight)
    {
        int[,] chunk = chunkData.getChunkData(); //  2d array of the contents of the chunk, each integer represents a block ID (0 = no block)
        List<float[]> frontBackgroundBlocks = chunkData.getFrontBackgroundBlocks(); // list of type {[x,y, blockID]}, blocks that go on the FrontBackground layer
		List<float[]> backBackgroundBlocks = chunkData.getBackBackgroundBlocks(); // list of type {[x,y, blockID]}, blocks that go on the BackBackground layer
        List<float[]> backgroundVisualBlocks = chunkData.getBackgroundVisualBlocks();
		int chunkPos = chunkData.getChunkPosition(); // position of the chunk (left side of the chunk)
		float height = chunkData.getStartHeight();  // height of where blocks started spawning in this chunk, (basically means: y position of grass block)
        Hashtable prevOreSpawns = chunkData.getPrevOreSpawns();
        List<object[]> entities = editEntities(chunkData.getEntities(), chunkData);

        List<Vector3Int> tilePositionsInChunk = new List<Vector3Int>(); // list of the tiles in the chunk

		float xPos = chunkPos + SpawningChunkData.blockSize/ 2;
        float yPos = SpawningChunkData.maxBuildHeight - SpawningChunkData.blockSize/ 2;
        for(int x = 0; x < chunk.GetLength(0); x++) // spawn blocks on the "Default" layer
        {
            for(int y = 0; y < chunk.GetLength(1); y++)
            {
                Vector3Int spawnedTilePos = instantiateTile(chunk[x, y], xPos + SpawningChunkData.blockSize * x, yPos - SpawningChunkData.blockSize * y);
                if(spawnedTilePos != new Vector3Int(-100, -100)) tilePositionsInChunk.Add(spawnedTilePos);
                //instantiateBlock(chunk[x,y], xPos + SpawningChunkData.blockSize* x, yPos - SpawningChunkData.blockSize* y); // (blockID, xPos, yPos, layer)
            }
        }
		// if this chunk is in a tundra biome, then randomly add snow on top of the topmost blocks
		if (spawnChunkStrategy is Tundra)
		{
			List<float[]> snow = generateSnow(chunkData);
			foreach (float[] block in snow) // spawn the thin snow blocks and add them to the chunk data
			{
				chunkData.changeBlock(block[0], block[1], (int)block[2], "FrontBackground");
				instantiateBlock((int)block[2], block[0], block[1], "FrontBackground");
			}
            // spawn the falling snow at y=30
            snowParticleSystems[chunkPos] = Instantiate(snowParticleSystem, new Vector2(chunkPos + 5, 30), Quaternion.Euler(90f, 0f, 0f));

		}
		foreach (float[] block in frontBackgroundBlocks) // spawn blocks on the "FrontBackground" layer
		{
            instantiateBlock((int)block[2], block[0], block[1], "FrontBackground");
        }

		foreach (float[] block in backBackgroundBlocks) // spawn blocks on the "BackBackground" layer
		{
			instantiateBlock((int)block[2], block[0], block[1], "BackBackground");
		}
        foreach (float[] block in backgroundVisualBlocks)
        {
			instantiateTile((int)block[2], block[0], block[1], true);
		}

        if (goingRight)
        {
            SpawningChunkData.setRightMostY(height);
			SpawningChunkData.setPrevSpawnedOresRight(prevOreSpawns);
        }
        else
        {
			SpawningChunkData.setLeftMostY(height);
			SpawningChunkData.setPrevSpawnedOresLeft(prevOreSpawns);
		}

        // spawn entities in the chunk
        spawnEntities(entities);

        // check what tiles are exposed to air and turn them into GameObjects
        changeTilesToGameObjects(tilePositionsInChunk);


        // add lighting to the blocks
        //addLightingToBlocks(chunkPos);

    }
    // returns a list of SnowBlockThin that will be on the tundra chunk
    private List<float[]> generateSnow(ChunkData chunkData)
    {
		int[,] chunk = chunkData.getChunkData();
		List<float[]> frontBackgroundBlocks = chunkData.getFrontBackgroundBlocks();

        List<float[]> thinSnowBlocksToAdd = new List<float[]>();
		for (int i = 0; i < SpawningChunkData.blocksInChunk; i++)
        {
			float rand = (float)(random.NextDouble() * 100); // get random number between 0 and 100
            if(rand < 50)
            {
                int index = 0;
                while (index < chunk.GetLength(1) && chunk[i,index] == 0) // find the topmost block in the vertical line in the chunk
                {
                    index++;
                }
                Vector2 snowPosition = new Vector2(chunkData.getChunkPosition() + i + 0.5f, blockIndexToYPosition(index - 1));
                bool doAdd = true;
                foreach (float[] block in frontBackgroundBlocks)
                {
                    // if there is already a thin snow block at this position, then dont add a new one
                    if ((int)block[2] == 35 && block[0] == snowPosition.x && block[1] == snowPosition.y)
                    {
                        doAdd = false;
                        break;
                    }
                }
                // add the thin snow block above the topmost block
                if(doAdd) thinSnowBlocksToAdd.Add(new float[] {snowPosition.x, snowPosition.y, 35 });
            }
		}
        return thinSnowBlocksToAdd;
    }

    private void spawnEntities(List<object[]> entities)
    {
        foreach (object[] entity in entities)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs\\Entities\\" + entity[2]), new Vector2((float)entity[0], (float)entity[1] + 1), Quaternion.identity);
        }
    }

    // creates the block with the corresponding blockID, in the position (xPos, yPos)
    private void instantiateBlock(int blockID, float xPos, float yPos, string layer = "Default")
    {
        if (blockID == 0) return;
        if(blockID < 0) // special case where we need to spawn in a log and a leaf in front of it
        {
            int posBlockID = Math.Abs(blockID);
            instantiateBlock(posBlockID, xPos, yPos, "FrontBackground");
			instantiateBlock(posBlockID+1, xPos, yPos, "FrontBackground");
            return;
		}

        Sprite dirtTexture = null;
		if (blockID == 2) // grass block
        {
			dirtTexture = BlockHashtable.getBlockByID(2).GetComponent<SpriteRenderer>().sprite;
			BlockHashtable.getBlockByID(2).GetComponent<SpriteRenderer>().sprite = grassTexture;
		}
        else if (blockID == 28) // snowy grass block
        {
			dirtTexture = BlockHashtable.getBlockByID(28).GetComponent<SpriteRenderer>().sprite;
			BlockHashtable.getBlockByID(28).GetComponent<SpriteRenderer>().sprite = snowyGrassTexture;
		}
		GameObject spawnedBlock = Instantiate(BlockHashtable.getBlockByID(blockID), new Vector3(xPos, yPos, 0), transform.rotation);
		if (blockID == 2) BlockHashtable.getBlockByID(2).GetComponent<SpriteRenderer>().sprite = dirtTexture;
		else if (blockID == 28) BlockHashtable.getBlockByID(28).GetComponent<SpriteRenderer>().sprite = dirtTexture;
		spawnedBlock.layer = LayerMask.NameToLayer(layer);

        if (layer.Equals("BackBackground"))
        {
			SpriteRenderer blockRenderer = spawnedBlock.GetComponent<SpriteRenderer>();
			blockRenderer.color = new Color(170f / 255f, 170f / 255f, 170f / 255f); // dark tint
			blockRenderer.sortingOrder = -10;
		}

	}
	// returns the position of the tile, if the tile was set, otherwise Vector3Int(-100, -100)
	public Vector3Int instantiateTile(int blockID, float xPos, float yPos, bool isBackgroundVisualTile = false)
    {
        Vector3Int tilePos = new Vector3Int(-100, -100);

		if (blockID == 0) return tilePos;
        if(isBackgroundVisualTile)
        {
			tilePos = backgroundVisualTiles.WorldToCell(new Vector2(xPos, yPos));

			Tile tile = BlockHashtable.getTileByID(blockID);
			backgroundVisualTiles.SetTile(tilePos, tile);
		}
        else
        {
			tilePos = tilemap.WorldToCell(new Vector2(xPos, yPos));

			Tile tile = BlockHashtable.getTileByID(blockID);
			tilemap.SetTile(tilePos, tile);
		}


        return tilePos;
    }

    private void changeTilesToGameObjects(List<Vector3Int> tilePositions)
    {
        List<Vector3Int> tilesToReplace = new List<Vector3Int>();

        foreach (Vector3Int tilePos in tilePositions)
        {
            if (isTileExposedToAir(tilePos)) // if exposed to air then we need to change the tile into a gameobject
            {
                tilesToReplace.Add(tilePos);
			}
            
        }

        foreach(Vector3Int tilePos in tilesToReplace)
        {
			TileBase tile = tilemap.GetTile(tilePos);
			spawnGameObjectInsteadOfTile(tile, tilePos); // place gameObject at tiles position

			tilemap.SetTile(tilePos, null); // remove tile
		}
    }

    public void spawnGameObjectInsteadOfTile(TileBase tile, Vector3Int tilePos)
    {
        if(tile.name == "GrassBlock")
        {
            instantiateBlock(2, tilePos.x + .5f, tilePos.y + .5f); // spawn grass block
            return;
        }
		if (tile.name == "SnowyGrassBlock")
		{
			instantiateBlock(28, tilePos.x + .5f, tilePos.y + .5f); // spawn grass block
			return;
		}
		instantiateBlock(BlockHashtable.getIDByBlockName(tile.name), tilePos.x + .5f, tilePos.y + .5f);
    }

    private bool isTileExposedToAir(Vector3Int tilePos)
    {
		Vector3Int aboveTilePos = new Vector3Int(tilePos.x, tilePos.y + 1, tilePos.z);
        if (!tilemap.HasTile(aboveTilePos)) return true;

        if (tilePos.y != lowestBlockPos) // if its not the lowest block in the chunk
        {
			Vector3Int belowTilePos = new Vector3Int(tilePos.x, tilePos.y - 1, tilePos.z);
			if (!tilemap.HasTile(belowTilePos)) return true;
		}

		Vector3Int rightTilePos = new Vector3Int(tilePos.x + 1, tilePos.y, tilePos.z);
		if (!tilemap.HasTile(rightTilePos)) return true;

		Vector3Int leftTilePos = new Vector3Int(tilePos.x - 1, tilePos.y, tilePos.z);
		if (!tilemap.HasTile(leftTilePos)) return true;

		return false;
	}

    private void removeTilesInChunk(int chunkPos)
    {
        Vector3Int tilePos = new Vector3Int(chunkPos, SpawningChunkData.maxBuildHeight); // upper leftmost tile in chunk
        for(int i = 0; i < SpawningChunkData.blocksInChunk; i++)
        {
            for(int j = 0; j <=  SpawningChunkData.maxBuildHeight + Math.Abs(lowestBlockPos); j++)
            {
                if (tilemap.HasTile(tilePos)) tilemap.SetTile(tilePos, null); // if tile exists, then remove tile
				if (backgroundVisualTiles.HasTile(tilePos)) backgroundVisualTiles.SetTile(tilePos, null); // the same, but for background visual tiles
				tilePos.y--;
            }
            tilePos.y = SpawningChunkData.maxBuildHeight;
            tilePos.x++;
		}
    }


	// lighting

	/**
     * goes through all the blocks on the Default layer in the chunk and for every block that 
     * is exposed to air, it will call putLightingOnBlock which adds lighting on the block.
     */
	private void addLightingToBlocks(int chunkPos)
    {
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default"));

		List<Collider2D> blocks = getCollidersWithinChunk(chunkPos, filter);

        // could remove this by setting the prefabs initially to black
		foreach (Collider2D block in blocks)
        {
            block.gameObject.GetComponent<BlockLighting>().setStage();
        }


		foreach (Collider2D block in blocks)
        {
            if (isExposedToAir(block.gameObject, chunkPos))
            {
                putLightingOnBlock(block.gameObject);
            }
        }
    }
    /**
     * uses BFS to add lighting to the block and the surrounding blocks
     */
    private void putLightingOnBlock(GameObject block)
	{
		HashSet<int> visitedHashSet = new HashSet<int>(); // if a gameObject.instanceID is in this, then it is visited

		LinkedList<object[]> queue = new LinkedList<object[]>(); // {[BlockLighting, int stage], [BlockLighting, int stage], ...}

		// Mark the current block as visited and enqueue it
		visitedHashSet.Add(block.GetInstanceID());
		queue.AddLast(new object[] { block.GetComponent<BlockLighting>(), 0 });

		while (queue.Any())
		{
			// Dequeue a vertex
			object[] s = queue.First();
			queue.RemoveFirst();

            BlockLighting sScript = (BlockLighting)s[0];
            int sStage = (int)s[1];





            // only go on neighbors if their value is supposed to be 3 or less
            
            Debug.Log(sScript.getStage());
			if (sStage < 3 && sScript.getStage() > sStage)
			{
				// Get all adjacent vertices of the
				// dequeued vertex s.
				// If an adjacent has not been visited,
				// then mark it visited and enqueue it

				List<BlockLighting> neighbors = sScript.getNeighbors();
                Debug.Log("Has " +  neighbors.Count + " neighbors!");
				foreach (BlockLighting n in neighbors)
				{
					if (!visitedHashSet.Contains(n.gameObject.GetInstanceID())) // if not visited
					{
						visitedHashSet.Add(n.gameObject.GetInstanceID());
						queue.AddLast(new object[] { n, sStage + 1 });
					}
				}
			}

			if (sScript.getStage() > sStage) // only change its stage if its darker than s[1]
			{
				sScript.setStage(sStage);
			}

		}
	}

    /**
     * checks if the block is exposed to air
     */
	private bool isExposedToAir(GameObject block, int chunkPos)
    {
		int mask = LayerMask.GetMask("Default");

		// check above block
		Vector2 aboveBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y + block.GetComponent<SpriteRenderer>().bounds.size.y);
        if (!Physics2D.OverlapCircle(aboveBlockPosition, 0.1f, mask)) return true;

        // check below block
		Vector2 belowBlockPosition = new Vector2(block.transform.position.x, block.transform.position.y - block.GetComponent<SpriteRenderer>().bounds.size.y);
        if (!Physics2D.OverlapCircle(belowBlockPosition, 0.1f, mask)) return true;

        if (block.transform.position.x != chunkPos + SpawningChunkData.blocksInChunk - .5f) // if its not the rightmost block in the chunk
        {
            // check right side
            Vector2 rightBlockPosition = new Vector2(block.transform.position.x + block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
            if (!Physics2D.OverlapCircle(rightBlockPosition, 0.1f, mask)) return true; // if there is not a block to the right
        }

        if(block.transform.position.x != chunkPos + .5f) // if it is not the block on the leftmost side of the chunk
        {
			// check left side
			Vector2 leftBlockPosition = new Vector2(block.transform.position.x - block.GetComponent<SpriteRenderer>().bounds.size.x, block.transform.position.y);
			if (!Physics2D.OverlapCircle(leftBlockPosition, 0.1f, mask)) return true;
		}


		return false;
    }
    /**
     * when spawning in a chunk that has already spawned before we need to:
     *      * check if its daytime, then despawn mobs that are above ground
     *      * maybe spawn in new animals
     *      
     * this method takes in a list of entities that are in the saved chunk that is being spawned in and
     * adds/removes entities from it.
     * 
     */
    private List<object[]> editEntities(List<object[]> chunkEntities, ChunkData chunkData)
    {
        List<object[]> entities = new List<object[]>();
		if (dayProcessScript.isDaytime()) // if its daytime then we want to remove the mobs, if they're above ground
		{
            int amountOfAnimalsInChunk = 0;
			foreach (object[] entity in chunkEntities)
			{
                // if the entity is a mob
                if (Array.Exists(SpawnMobScript.getMobs(), element => element.Equals(entity[2])))
                {
					if ((float)entity[0] < 0)
                    {
                        // if the vertical line height is higher than the entity's y position
                        if (chunkData.getVerticalLineHeight(9 - convertWorldXPosToChunkIndex((float)entity[0])) > (float)entity[1])
                        {
							// don't remove this entity
							entities.Add(entity);
                        }
					}
                    else
                    {
                        // if the vertical line height is higher than the entity's y position
                        if (chunkData.getVerticalLineHeight(convertWorldXPosToChunkIndex((float)entity[0])) > (float)entity[1])
                        {
                            // don't remove this entity
                            entities.Add(entity);
                        }
                    }

                }
				else // if the entity is an animal
				{
                    entities.Add(entity);
                    amountOfAnimalsInChunk++;
                }
			}

            if(amountOfAnimalsInChunk == 0)
            {
                // spawn in animals possibly
                foreach (object[] animal in SpawnAnimalScript.decideIfSpawnAnimalsOnSavedChunk(chunkData.getChunkPosition(), chunkData.getVerticalLineHeights())){
                    entities.Add(animal);
                }
			}
		}
        else // if its nighttime, then we want to spawn in mobs
        {
			int amountOfMobsInChunk = 0;
			foreach (object[] entity in chunkEntities)
			{
				// if the entity is a mob
				if (Array.Exists(SpawnMobScript.getMobs(), element => element.Equals(entity[2])))
				{
                    amountOfMobsInChunk++;
				}
				entities.Add(entity);
			}
            if (amountOfMobsInChunk <= 1)
            {
				for (int _ = 0; _ < 2; _++) // do it x amount of times so x different types of mobs can spawn in the same chunk
				{
					entities.AddRange(SpawnMobScript.decideIfSpawnMob(chunkData.getChunkPosition(), chunkData.getVerticalLineHeights()));
				}
			}

        }

        return entities;
	}

    public int getLeftmostChunkPos()
    {
        return rendered;
    }

	public void setLeftmostChunkPos(int leftChunkPos)
	{
		rendered = leftChunkPos;
	}

	public int getAmountOfChunksRendered()
    {
        return amountOfChunksToRender;
    }

    public void setAmountOfChunksToRender(int num)
    {
        amountOfChunksToRender = num;
    }

	private int convertWorldXPosToChunkIndex(float x)
    {
        return Mathf.Abs(Mathf.FloorToInt(x)) % SpawningChunkData.blocksInChunk;

	}

	public float blockIndexToYPosition(int blockIndex)
	{
		return SpawningChunkData.maxBuildHeight - blockIndex - 0.5f;
	}
}
