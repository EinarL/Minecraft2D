using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/**
 * this static class holds data that is neccessary to know when spawning in chunks
 */
public static class SpawningChunkData
{
	private static float leftMostY; // height of the left most vertical line on the left most chunk
	private static float rightMostY;// height of the right most vertical line on the right most chunk
	private static Hashtable prevSpawnedOresRight = new Hashtable();
	private static Hashtable prevSpawnedOresLeft = new Hashtable(); // ores in the previously spawned vLine. (vLineIndex, blockID)
	public static int[] prevVerticalLineRight; // the previous vertical line that was rendered on a right chunk, represented with block ID's
	public static int[] prevVerticalLineLeft;
	private static int dontGoDownLeft = 0; // 0 represents that it can go down, but some number n means that it cant go down for the next n blocks
	private static int dontGoDownRight = 0;
	private static int dontGoUpLeft = 0;
	private static int dontGoUpRight = 0;
	private static List<ChunkData> renderedChunks = new List<ChunkData>(); // these are the chunks that are currently rendered
	public static int blocksInChunk = 10; // width of chunk, in blocks
	public static int maxBuildHeight = 80;
	public static float blockSize = 1;

	private static int leftMostChunkEdge = 0;
	private static int rightMostChunkEdge = 0;

	/**
	 * runs after the user places/breaks a block.
	 * this function updates a chunk in the list "renderedChunks" with the new blockID. 
	 */ 
	public static void updateChunkData(float x, float y, int newBlockID, string layer = "Default")
	{
		ChunkData correspondingChunk = null;
		foreach(ChunkData chunk in renderedChunks)
		{
			if (chunk.getChunkPosition() < x && chunk.getChunkPosition() + blocksInChunk > x) // if the blocks x position is in this chunk
			{
				correspondingChunk = chunk;
				break;
			}
		}

		switch (layer)
		{
			case "Default":
				int[] indexes = worldPosToChunkArrayIndex(x, y, correspondingChunk.getChunkPosition());
				correspondingChunk.changeBlock(indexes[0], indexes[1], newBlockID, layer);
				break;
			case "FrontBackground":
				correspondingChunk.changeBlock(x, y, newBlockID, layer);
				break;
			case "BackBackground":
				correspondingChunk.changeBlock(x, y, newBlockID, layer);
				break;
			default:
				Debug.LogError("incorrect layer parameter: " + layer);
				break;
		}
	}

	/**
	 * gets in a blocks position in the world and returns the indexes for the chunkArray for this block position. 
	 */
	private static int[] worldPosToChunkArrayIndex(float x, float y, int chunkPos)
	{
		int xIndex;
		if (chunkPos >= 0) xIndex = (int)x - chunkPos;
		else xIndex = Mathf.Abs(chunkPos) - Mathf.Abs((int)x) - 1;
		int yIndex;
		if (y < 0) yIndex = maxBuildHeight - (int)y;
		else yIndex = maxBuildHeight - (int)y - 1;

		return new int[] { xIndex, yIndex };
	}

	private static float[] chunkArrayIndexToWorldPos(int xIndex, int yIndex, int chunkPos)
	{
		float x = chunkPos + xIndex + blockSize/2;
		float y = maxBuildHeight - yIndex - blockSize/2;

		return new float[] { x, y };
	}

	public static void addRenderedChunk(ChunkData chunk)
	{
		// we know that the chunk being added is either the rightmost or leftmost, so its position is either lower or higher than all the other chunks
		if(renderedChunks.Count > 0 )
		{
			bool rightChunk = renderedChunks[0].getChunkPosition() < chunk.getChunkPosition(); // is the new chunk the rightmost chunk?
			if(rightChunk) 
			{
				rightMostChunkEdge = chunk.getChunkPosition() + blocksInChunk;
				rightMostY = chunk.getStartHeight();
			}
			else
			{
				leftMostChunkEdge = chunk.getChunkPosition();
				leftMostY = chunk.getStartHeight();
			}
		}
		else
		{
			rightMostChunkEdge = chunk.getChunkPosition() + blocksInChunk;
			leftMostChunkEdge = chunk.getChunkPosition();
		}

		renderedChunks.Add(chunk);
	}

	public static void removeAndSaveChunkByChunkPosition(int chunkPos)
	{
		for (int i = 0; i < renderedChunks.Count; i++)
		{
			if (renderedChunks[i].getChunkPosition() == chunkPos)
			{
				SaveChunk.save(renderedChunks[i]); // save chunk
				renderedChunks.RemoveAt(i);

				// we know that the chunk being removed is either the rightmost or leftmost, so its position is either lower or higher than all the other chunks
				if (renderedChunks.Count == 0) // if this happens, then we are in the process of unrendering all chunks
				{
					leftMostChunkEdge = -20; // TODO: need to change this number when i implement spawning elsewhere
				}
				else if(chunkPos < renderedChunks[0].getChunkPosition()) // if we removed the leftmost chunk
				{
					leftMostChunkEdge = chunkPos + blocksInChunk;
				}
				else // if we removed the rightmost chunk
				{
					rightMostChunkEdge = chunkPos;
				}

                return;
			}
		}
		Debug.LogError("Did not find a chunk with chunkPosition: " + chunkPos);
	}
	/**
	 * Saves the entites in the chunk, and overwrites the old saved entities
	 */
	public static void overwriteEntities(int chunkPos, List<object[]> entities)
	{
		ChunkData correspondingChunk = getChunkByChunkPos(chunkPos);
		if(correspondingChunk == null)
		{
			Debug.LogError("Did not find a chunk with chunkPosition: " + chunkPos);
			return;
		}
		correspondingChunk.setEntities(entities);
	}

	public static ChunkData getChunkByChunkPos(int chunkPos)
	{
		for (int i = 0; i < renderedChunks.Count; i++)
		{
			if (renderedChunks[i].getChunkPosition() == chunkPos)
			{
				return renderedChunks[i];
			}
		}
		return null;
	}
	// returns true if it added the block
	public static bool addBackgroundVisualBlock(float x, float y, int blockID)
	{
		foreach (ChunkData chunk in renderedChunks)
		{
			if (chunk.getChunkPosition() < x && chunk.getChunkPosition() + blocksInChunk > x) // we found the chunk that the block is in
			{
				return chunk.addBackgroundVisualBlock(x, y, blockID);
			}
		}
		Debug.LogError("Unable to find the chunk to place the background visual block in");
		return false;
	}

	/**
	 * Given an x position for a block, this function returns the vertical line height,
	 * i.e. the height of the grass block at the top.
	 * this is neccesary to determine when we mine a block whether to display a background block behind the block or not
	 */
	public static float getVerticalLineHeight(float x)
	{
		foreach (ChunkData chunk in renderedChunks)
		{
			if (chunk.getChunkPosition() < x && chunk.getChunkPosition() + blocksInChunk > x) // we found the chunk that the block is in
			{
				return chunk.getVerticalLineHeight(Mathf.Abs((int)x) % 10);
			}
		}

		return -1;
	}

	public static List<ChunkData> getRenderedChunks()
	{
		return renderedChunks;
	}

	public static int getLeftMostChunkEdge()
	{
		return leftMostChunkEdge;
	}

	public static int getRightMostChunkEdge()
	{
		return rightMostChunkEdge;
	}

	public static float getLeftMostY()
	{
		return leftMostY;
	}

	public static float getRightMostY()
	{
		return rightMostY;
	}

	public static void setLeftMostY(float value)
	{
		leftMostY = value;
	}

	public static void setRightMostY(float value)
	{
		rightMostY = value;
	}

	public static Hashtable getPrevSpawnedOresRight()
	{
		return prevSpawnedOresRight;
	}

	public static Hashtable getPrevSpawnedOresLeft()
	{
		return prevSpawnedOresLeft;
	}

	public static void setPrevSpawnedOresRight(Hashtable prevSpawnedOres)
	{
		prevSpawnedOresRight = prevSpawnedOres;
	}

	public static void setPrevSpawnedOresLeft(Hashtable prevSpawnedOres)
	{
		prevSpawnedOresLeft = prevSpawnedOres;
	}

	public static int getDontGoDownLeft()
	{
		return dontGoDownLeft;
	}

	public static int getDontGoDownRight()
	{
		return dontGoDownRight;
	}

	public static void setDontGoDownLeft(int value)
	{
		dontGoDownLeft = value;
	}

	public static void setDontGoDownRight(int value)
	{
		dontGoDownRight = value;
	}

	public static int getDontGoUpLeft()
	{
		return dontGoUpLeft;
	}

	public static int getDontGoUpRight()
	{
		return dontGoUpRight;
	}

	public static void setDontGoUpLeft(int value)
	{
		dontGoUpLeft = value;
	}

	public static void setDontGoUpRight(int value)
	{
		dontGoUpRight = value;
	}
}
