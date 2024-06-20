using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class spawnChunkScript : MonoBehaviour
{
	private static System.Random random = new System.Random();

	private int chunkSize;
    private int amountOfChunksToRender = 10; // amount of chunks to be rendered at the same time
    private float defaultStartSpawnY = -2.5f; // the default height of the vertical line that is to be spawned

    public Sprite grassTexture;
	public Sprite snowyGrassTexture;
    public Tilemap tilemap;
    public Tilemap backgroundVisualTiles;
    public ParticleSystem snowParticleSystem;

	// the snow particle systems that are being rendered, the key is the chunk position
	private Dictionary<int, ParticleSystem> snowParticleSystems = new Dictionary<int, ParticleSystem>(); 
	private HashSet<WaterScript> waterToFlow = new HashSet<WaterScript>(); // when we finish rendering a chunk then we need all of these water blocks to flow

	// water by the right and leftmost chunk border needs to be notified when to flow in order to not flow outside of the chunk and into the void
	// therefore we have a list of observers which will be notified when the next chunk is rendered and then they will flow
	public HashSet<WaterScript> rightChunkWaterToFlow = new HashSet<WaterScript>();
	public HashSet<WaterScript> leftChunkWaterToFlow = new HashSet<WaterScript>();

	private int lowestBlockPos = -60;
    private int rendered; // leftmost chunk that is rendered
    public bool pauseChunkRendering = false;

    private Biome spawnChunkStrategy;
    private List<Biome> biomes = new List<Biome> { new Plains(), new Desert(), new Tundra() }; // new List<Biome> { new Plains(), new Desert(), new Tundra() };

	// how long the biome is (in chunks), this counts down every time a new chunk is rendered and when
	// it hits 0, then a new random chunk gets generated
	private int biomeLength; 

    private SunLightMovementScript sunLightMovementScript;
    private DayProcessScript dayProcessScript;
	private MainThreadDispatcher mainThreadDispatcher;
	private IDataService dataService = JsonDataService.Instance;

	// Start is called before the first frame update
	void Start()
    {
		sunLightMovementScript = GameObject.Find("Sun").GetComponent<SunLightMovementScript>();
        dayProcessScript = GameObject.Find("CM vcam").transform.Find("SunAndMoonTexture").GetComponent<DayProcessScript>();
		mainThreadDispatcher = GameObject.Find("EventSystem").GetComponent<MainThreadDispatcher>();

		BlockHashtable.initializeBlockHashtable();
        spawnChunkStrategy = decideBiome(); // dont do this if the biome is already decided, we need to save which biome was rendering when we quit the game

		chunkSize = (int)(SpawningChunkData.blockSize * SpawningChunkData.blocksInChunk);
        SpawningChunkData.setRightMostY(defaultStartSpawnY);
		SpawningChunkData.setLeftMostY(defaultStartSpawnY);

        if (dataService.exists("player-position.json")) // if there is a saved player position
        {
            float[] playerPosition = dataService.loadData<float[]>("player-position.json");

			loadSpawn(new Vector2(playerPosition[0], playerPosition[1]));
        }
        else
        {
            loadSpawn(new Vector2(0, 0));
        }
	}



    // Update is called once per frame
    void Update()
    {
        if (pauseChunkRendering) return;
		/*
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
			for (int i = 1; i <= diff/2; i++)
            {
                unrenderChunk(rendered + (chunkSize * (i - 1))); //despawn left chunk
                unrenderChunk(rendered + chunkSize * (amountOfChunksToRender-1) + (chunkSize*(i-1))); // despawn right chunk
            }
            amountOfChunksToRender = newAmountOfChunksToRender;
            rendered = getChunkNumber();
		}
		*/
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

    /**
     * when the player respawns the cams soft zone setting is set to 0 to make the camera snap into the spawns position
     * so we need to put the soft zone setting back to normal after the player has respawned, therefore we have a coroutine here that puts it back to normal.
     */
    public void setCamSettingsBackToNormal(CinemachineFramingTransposer vcam, float value)
    {
		IEnumerator putSoftZoneBackToNormal()
		{
			yield return new WaitForSeconds(0.5f);
			vcam.m_SoftZoneWidth = value;
		}
		StartCoroutine(putSoftZoneBackToNormal());
	}

	public void loadSpawn(Vector2 playerPos)
	{
		transform.position = playerPos;
		rendered = getChunkNumber(playerPos.x);
		// spawn in the chunks at "camera x position"
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 5);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 4);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 3);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 6);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 2);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 7);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 1);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 8);
		renderChunk(rendered);
		renderChunk(rendered + SpawningChunkData.blocksInChunk * 9);
	}

	/**
	 * alreadyRenderedLeftMostChunk is the leftmost chunk that is rendered already (so we dont need to spawn in those chunks)
	 * since we spawn in 10 chunks at a time, then we dont need to render the chunks that are from 
	 * alreadyRenderedLeftMostChunk to alreadyRenderedLeftMostChunk + 10 * blocksInChunk (non-inclusive)
	 */
	public void loadSpawn(Vector2 playerPos, int alreadyRenderedLeftMostChunk)
	{
		transform.position = playerPos;
		rendered = getChunkNumber(playerPos.x);
		// spawn in the chunks at "camera x position"
		for(int i = rendered; i < rendered + SpawningChunkData.blocksInChunk * 10; i += SpawningChunkData.blocksInChunk)
		{
			if (i < alreadyRenderedLeftMostChunk || i >= alreadyRenderedLeftMostChunk + (10 * SpawningChunkData.blocksInChunk)) renderChunk(i); // render chunk if it isnt already rendered
		}
	}

	private Biome decideBiome()
    {

        Biome currentBiome = spawnChunkStrategy;

        if(currentBiome != null) biomes.Remove(currentBiome);

		System.Random rand = new System.Random();
		int randIndex = rand.Next(biomes.Count); // get random index

		Biome newBiome = biomes[randIndex];
        biomeLength = rand.Next(newBiome.biomeLength[0], newBiome.biomeLength[1] + 1);
        
        if (currentBiome != null) biomes.Add(currentBiome);

        return newBiome; // newBiome
    }

	/*
	public int getAmountOfChunksToRender()
	{
		int size = (int)cam.orthographicSize;
		Debug.Assert(size <= 20);

		if (size <= 5) return 4;
		if (size <= 7) return 6;
		if (size <= 11) return 8;
		return 10;
	}
	*/

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

	public int getChunkNumber(float xPos)
	{
		float closeToChunkNumber = xPos - (chunkSize * amountOfChunksToRender / 2); // round this number to the closest 10
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
	public async void renderChunk(int chunkStart)
    {
		ChunkData chunkData;

		bool fromRight = chunkStart < transform.position.x;

		// if chunk has been rendered, then render the saved chunk
		if (SaveChunk.exists(chunkStart))
        {
            chunkData = await Task.Run(() => SaveChunk.load(chunkStart));

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
        SpawningChunkData.addRenderedChunk(chunkData, !fromRight);

		if (!fromRight) // if rendering right chunk (i.e. rendering from left to right)
		{
			waterToFlow.UnionWith(rightChunkWaterToFlow);
			rightChunkWaterToFlow = new HashSet<WaterScript>();
		}
		else
		{
			waterToFlow.UnionWith(leftChunkWaterToFlow);
			leftChunkWaterToFlow = new HashSet<WaterScript>();
		}

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
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground") | LayerMask.GetMask("Entity") | LayerMask.GetMask("Item") | LayerMask.GetMask("Water"));

		List<Collider2D> results = getCollidersWithinChunk(chunkPos, filter);

        List<object[]> entities = new List<object[]>();
		List<GameObject> toDestroy = new List<GameObject>();

		foreach (Collider2D collider in results)
		{
			// add entites in this chunk to the list
			if (collider.gameObject.layer == 10)
			{
				entities.Add(new object[] { collider.gameObject.transform.position.x, collider.gameObject.transform.position.y, collider.gameObject.name });
			}
			toDestroy.Add(collider.gameObject);
		}
		foreach (GameObject obj in toDestroy)
		{
			Destroy(obj);
		}

		if (snowParticleSystems.ContainsKey(chunkPos)) // destroy the snow particle system if there is one in this chunk
        {
            Destroy(snowParticleSystems[chunkPos].gameObject);
            snowParticleSystems.Remove(chunkPos);
        }


		Task.Run(() => SpawningChunkData.overwriteEntities(chunkPos, entities)); // save entities


		Task.Run(() => {
			ChunkData chunkToRemove = SpawningChunkData.getChunkByChunkPos(chunkPos);
			if (chunkToRemove != null) sunLightMovementScript.removeChunkHeight(chunkToRemove.getVerticalLineHeights()); // for sun position adjustment
		});

		Task.Run(() => SpawningChunkData.removeAndSaveChunkByChunkPosition(chunkPos)); // save

		addWaterToObserverList(chunkPos > transform.position.x, chunkPos > transform.position.x ? chunkPos - 0.5f : chunkPos + 10.5f);

		// remove tiles
		removeTilesInChunk(chunkPos);
	}
	/**
	 * when we unrender a chunk we need to add the water by the chunk edge on the left/right-most chunk (not the chunk being unrendered but the one next to it)
	 * to the observer list so that the water will then be notified to flow again when the chunk (that was being unrendered)
	 * will render again
	 */
	private void addWaterToObserverList(bool rightChunk, float vLineXPosition)
	{
		if(rightChunk) rightChunkWaterToFlow = new HashSet<WaterScript>();
		else leftChunkWaterToFlow = new HashSet<WaterScript>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		Vector2 center = new Vector2(vLineXPosition, (SpawningChunkData.maxBuildHeight + lowestBlockPos) / 2);
		Vector2 size = new Vector2(0.5f, SpawningChunkData.maxBuildHeight + Math.Abs(lowestBlockPos));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapBox(center, size, 0f, filter, results);

		foreach(Collider2D collider in results)
		{
			if(rightChunk) rightChunkWaterToFlow.Add(collider.gameObject.GetComponent<WaterScript>());
			else leftChunkWaterToFlow.Add(collider.gameObject.GetComponent<WaterScript>());
		}
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

		float xPos = chunkPos + SpawningChunkData.blockSize/ 2;
        float yPos = SpawningChunkData.maxBuildHeight - SpawningChunkData.blockSize/ 2;

        renderDefaultLayer(chunk, xPos, yPos);

		// if this chunk is in a tundra biome, then randomly add snow on top of the topmost blocks
		if (chunkData.getBiome().Equals("Tundra"))
		{
			renderSnow(chunkData, chunk, frontBackgroundBlocks, chunkPos, height);
		}

		StartCoroutine(renderOtherLayers(new List<float[]>(frontBackgroundBlocks), backBackgroundBlocks)); // renders the blocks on the frontBackground and BackBackground

		renderBackgroundVisualLayer(backgroundVisualBlocks);

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
		StartCoroutine(spawnEntities(entities));
	}


	private async void renderSnow(ChunkData chunkData, int[,] chunk, List<float[]> frontBackgroundBlocks ,int chunkPos, float height)
	{
		List<float[]> snow = await Task.Run(() => generateSnow(chunk, frontBackgroundBlocks, chunkPos));
		foreach (float[] block in snow) // spawn the thin snow blocks and add them to the chunk data
		{
			chunkData.changeBlock(block[0], block[1], (int)block[2], "FrontBackground");
			instantiateBlock((int)block[2], block[0], block[1], "FrontBackground");
		}
		// spawn the falling snow at y=chunkYposition + x
		snowParticleSystems[chunkPos] = Instantiate(snowParticleSystem, new Vector2(chunkPos + 5, height + 60), Quaternion.Euler(90f, 0f, 0f));

	}

	/**
	 * renders blocks on frontBackground and backBackground layer
	 * 
	 * front- and backBackground blocks are a list of float[] where each float[] contains 3 values, {blockID, xPos, yPos}
	 */
	private IEnumerator renderOtherLayers(List<float[]> frontBackgroundBlocks, List<float[]> backBackgroundBlocks)
	{
		int i = 0;
		foreach (float[] block in frontBackgroundBlocks)
		{
			instantiateBlock((int)block[2], block[0], block[1], "FrontBackground");
			i++;
			if (i % 5 == 0) yield return null;
		}

		foreach (float[] block in backBackgroundBlocks)
		{
			instantiateBlock((int)block[2], block[0], block[1], "BackBackground");
			i++;
			if (i % 5 == 0) yield return null;
		}
	}


    private async void renderDefaultLayer(int[,] chunk, float xPos, float yPos)
    {
		TileBase[] tiles = new TileBase[chunk.GetLength(0) * chunk.GetLength(1)];
		Vector3Int[] tilePositions = new Vector3Int[chunk.GetLength(0) * chunk.GetLength(1)];

		await Task.Run(() =>
		{
			int index = 0;
			for (int x = 0; x < chunk.GetLength(0); x++) // spawn blocks on the "Default" layer
			{
				for (int y = 0; y < chunk.GetLength(1); y++)
				{
					if ((41 <= chunk[x, y] && chunk[x, y] <= 56) || chunk[x, y] == 61) // if its a door || water
					{
						int blockID = chunk[x, y];
						float xBlockPos = xPos + SpawningChunkData.blockSize * x;
						float yBlockPos = yPos - SpawningChunkData.blockSize * y;
						mainThreadDispatcher.enqueue(() => instantiateBlock(blockID, xBlockPos, yBlockPos, blockID == 61 ? "Water" : "Default")); // make the main thread do this
						tiles[index] = null;
					}
					else
					{
						tiles[index] = BlockHashtable.getTileByID(chunk[x, y]);
					}
					tilePositions[index] = new Vector3Int((int)(xPos + SpawningChunkData.blockSize * x - 0.5f), (int)(yPos - SpawningChunkData.blockSize * y - 0.5f));
					index++;
				}
			}
			mainThreadDispatcher.enqueue(() =>
			{
				foreach (WaterScript water in waterToFlow)
				{
						if(water != null) water.startFlowing();
				}
				waterToFlow = new HashSet<WaterScript>();
			});
			
		});

        tilemap.SetTiles(tilePositions, tiles);
	}

	private async void renderBackgroundVisualLayer(List<float[]> backgroundVisualBlocks)
	{
		TileBase[] tiles = new TileBase[backgroundVisualBlocks.Count];
		Vector3Int[] tilePositions = new Vector3Int[backgroundVisualBlocks.Count];
		await Task.Run(() =>
		{
			int index = 0;
			foreach (float[] block in backgroundVisualBlocks)
			{
				tiles[index] = BlockHashtable.getTileByID((int)block[2]);
				tilePositions[index] = new Vector3Int((int)(block[0] - 0.5f), (int)(block[1] - 0.5f));
				index++;
			}
		});

		backgroundVisualTiles.SetTiles(tilePositions, tiles);
	}


	// returns a list of SnowBlockThin that will be on the tundra chunk
	private List<float[]> generateSnow(int[,] chunk, List<float[]> frontBackgroundBlocks, int chunkPos)
    {
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
                Vector2 snowPosition = new Vector2(chunkPos + i + 0.5f, blockIndexToYPosition(index - 1));
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

    private IEnumerator spawnEntities(List<object[]> entities)
    {
        foreach (object[] entity in entities)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs\\Entities\\" + entity[2]), new Vector2((float)entity[0], (float)entity[1] + 1), Quaternion.identity);
			yield return null;
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

		GameObject spawnedBlock = Instantiate(BlockHashtable.getBlockByID(blockID), new Vector3(xPos, yPos, 0), transform.rotation); // create block

		if (blockID == 2) spawnedBlock.GetComponent<SpriteRenderer>().sprite = grassTexture; // grass block
		else if (blockID == 28) spawnedBlock.GetComponent<SpriteRenderer>().sprite = snowyGrassTexture; // snowy grass block
		spawnedBlock.layer = LayerMask.NameToLayer(layer);

        if (layer.Equals("BackBackground"))
        {
			SpriteRenderer blockRenderer = spawnedBlock.GetComponent<SpriteRenderer>();
			blockRenderer.color = new Color(170f / 255f, 170f / 255f, 170f / 255f); // dark tint
			blockRenderer.sortingOrder = -10;
		}

		if (layer.Equals("Water")) waterToFlow.Add(spawnedBlock.GetComponent<WaterScript>());

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
            if (41 <= blockID && blockID <= 56) // if its a door
            {
                instantiateBlock(blockID, xPos, yPos);
                return tilePos;
            }
			tilePos = tilemap.WorldToCell(new Vector2(xPos, yPos));

			Tile tile = BlockHashtable.getTileByID(blockID);
			tilemap.SetTile(tilePos, tile);
		}


        return tilePos;
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

	private void removeTilesInChunk(int chunkPos)
	{
		// Define the bounds of the chunk
		int chunkWidth = SpawningChunkData.blocksInChunk;
		int chunkHeight = SpawningChunkData.maxBuildHeight + Mathf.Abs(lowestBlockPos);

		// Top-left corner of the chunk
		Vector3Int topLeft = new Vector3Int(chunkPos, SpawningChunkData.maxBuildHeight, 0);
		// Bottom-right corner of the chunk
		Vector3Int bottomRight = new Vector3Int(chunkPos + chunkWidth - 1, -Mathf.Abs(lowestBlockPos), 0);

		// Define the bounds
		BoundsInt bounds = new BoundsInt(topLeft.x, bottomRight.y, 0, chunkWidth, chunkHeight, 1);

		// Clear the area in the tilemap and backgroundVisualTiles
		StartCoroutine(clearArea(tilemap, bounds));
		StartCoroutine(clearArea(backgroundVisualTiles, bounds));
	}


	private IEnumerator clearArea(Tilemap tMap, BoundsInt bounds)
	{
		int width = bounds.size.x;
		int height = bounds.size.y;

		int batchSize = 100; // Adjust batch size as needed
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x += batchSize)
			{
				int remainingWidth = Mathf.Min(batchSize, width - x);
				BoundsInt batchBounds = new BoundsInt(bounds.x + x, bounds.y + y, 0, remainingWidth, 1, 1);

				tMap.SetTilesBlock(batchBounds, new TileBase[remainingWidth]);

				// Yield to allow frame to update
				yield return null;
			}
		}
	}

	/**
     * when spawning in either a new chunk or a chunk that has already spawned before we need to:
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

	private int convertWorldXPosToChunkIndex(float x)
    {
        return Mathf.Abs(Mathf.FloorToInt(x)) % SpawningChunkData.blocksInChunk;

	}

	public float blockIndexToYPosition(int blockIndex)
	{
		return SpawningChunkData.maxBuildHeight - blockIndex - 0.5f;
	}
}
