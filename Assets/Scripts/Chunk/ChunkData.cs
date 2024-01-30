using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
	private int[,] chunkData;
	private List<float[]> frontBackgroundBlocks = new List<float[]>(); // list of type {[x,y, blockID]}
	private List<float[]> backBackgroundBlocks = new List<float[]>(); // list of type {[x,y, blockID]}
	private int chunkPosition;
	private float startHeight; // height of the chunk (height of grass block)
	// need to also have here the variables dontGoUpRight, dontGoUpLeft, etc.
	private List<object[]> entities; // list of entities in this chunk // of type {[x, y, entityName], [x, y, entityName], ...}


	// keeps track of where ores spawned, this is needed for a smooth transition for ores in the next chunk
	private Hashtable prevOreSpawns = new Hashtable(); // (y, blockID) 

	public ChunkData(int chunkPosition, int[,] chunkData, float startHeight, Hashtable prevOreSpawns, List<float[]> frontBackgroundBlocks, List<object[]> entities)
	{
		this.chunkData = chunkData;
		this.chunkPosition = chunkPosition;
		this.startHeight = startHeight;
		this.prevOreSpawns = prevOreSpawns;
		this.frontBackgroundBlocks = frontBackgroundBlocks;
		this.entities = entities;
	}

	public void changeBlock(float x, float y, int newBlockID, string layer = "Default")
	{
		if (layer.Equals("Default"))
		{
			chunkData[(int)x, (int)y] = newBlockID;
		}
		else if (layer.Equals("FrontBackground"))
		{
			if (newBlockID == 0) // if we're removing a block
			{
				removeFrontBackgroundBlockByPosition(x, y);
			}
			else frontBackgroundBlocks.Add(new float[] { x, y, newBlockID });
		}
		else if (layer.Equals("BackBackground"))
		{
			if (newBlockID == 0) // if we're removing a block
			{
				removeBackBackgroundBlockByPosition(x, y);
			}
			else backBackgroundBlocks.Add(new float[] { x, y, newBlockID });
		}
		else Debug.LogError("The layer you're trying to access doesnt exist. layer: " + layer);
		
	}

	private void removeBackBackgroundBlockByPosition(float x, float y)
	{
		for (int i = 0; i < backBackgroundBlocks.Count; i++)
		{
			if (backBackgroundBlocks[i][0] == x && backBackgroundBlocks[i][1] == y)
			{
				backBackgroundBlocks.RemoveAt(i);
				return;
			}
		}
		Debug.LogError("Did not find a block in BackBackground layer to remove at: " + x + ", " + y);
	}

	private void removeFrontBackgroundBlockByPosition(float x, float y)
	{
		for (int i = 0; i < frontBackgroundBlocks.Count; i++)
		{
			if (frontBackgroundBlocks[i][0] == x && frontBackgroundBlocks[i][1] == y)
			{
				frontBackgroundBlocks.RemoveAt(i);
				return;
			}
		}
		Debug.LogError("Did not find a block in FrontBackground layer to remove at: " + x + ", " + y);
	}

	public List<object[]> getEntities()
	{
		return entities;
	}

	public void setEntities(List<object[]> entities)
	{
		this.entities = entities;
	}

	public List<float[]> getFrontBackgroundBlocks() 
	{
		return frontBackgroundBlocks;
	}

	public List<float[]> getBackBackgroundBlocks()
	{
		return backBackgroundBlocks;
	}

	public int[,] getChunkData()
	{
		return chunkData;
	}

	public int getChunkPosition()
	{
		return chunkPosition;
	}

	public float getStartHeight()
	{
		return startHeight;
	}

	public Hashtable getPrevOreSpawns()
	{
		return prevOreSpawns;
	}
}
