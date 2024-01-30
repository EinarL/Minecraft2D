using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnTreeScript
{

	private static float treeSpawnChance = 10; // odds of a tree spawning, 0-100
	private static int minHeight = 4;
	private static int maxHeight = 7;
	private static int treeProgressRight = 0;
	private static int treeProgressLeft = 0;
	private static int treeHeightRight = 0; // the height of the tree that is currently being spawned on the right
	private static int treeHeightLeft = 0;
	private static float bottomPosRight; // the bottom index of the tree that is currently being spawned on the right
	private static float bottomPosLeft;

	/**
	 * decides if there should be a tree here, 
	 * 
	 * returns a list that tells what blocks to add to the previous lines, this is for spawning leaves in vLines that are already rendered
	 */
	public static List<float[]> decideIfSpawnTree(float treeBottomYPos, int chunkPos, float xPos)
	{
		List<float[]> blockPosAndID = new List<float[]>(); // {{x, y, blockID}, {x, y, blockID}, ...}
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		bool goingRight = chunkPos >= 0;


		// dont spawn a tree at the first two blocks on the first chunk. this is to fix a bug where the tree doesnt spawn leaves on the left chunk because that chunk hasnt rendered yet 
		if (chunkPos == 0 && (xPos == 0.5f || xPos == 1.5f)) return new List<float[]>();

		// if we are in the process of spawning a tree
		else if ((goingRight && treeProgressRight > 0) || (!goingRight && treeProgressLeft > 0))
		{
			float bottomPos = goingRight ? bottomPosRight : bottomPosLeft;
			if ((goingRight && treeProgressRight == 3) || (!goingRight && treeProgressLeft == 3)) bottomPos = treeBottomYPos;

			blockPosAndID = spawnTree(bottomPos, goingRight, xPos);
			if (goingRight) treeProgressRight--;
			else treeProgressLeft--;
		}
		else if (rand < treeSpawnChance)
		{
			if (goingRight) treeProgressRight = 5;
			else treeProgressLeft = 5;
			blockPosAndID = spawnTree(treeBottomYPos, goingRight, xPos);
			if (goingRight) treeProgressRight--;
			else treeProgressLeft--;
		}

		return blockPosAndID;
	}

	/*+
	 * 
	 * returns blockPosAndID List<int[]> of type: {{x, y, blockID}, {x, y, blockID}, ...} which is info about the position
	 *		   of the blocks for the trees.
	 */
	private static List<float[]> spawnTree(float treeBottomYPos, bool goingRight, float xPos)
	{
		int process = goingRight ? treeProgressRight : treeProgressLeft;
		int height = goingRight ? treeHeightRight : treeHeightLeft;
		List<float[]> blockPosAndID = new List<float[]>(); // {{x, y, blockID}, {x, y, blockID}, ...}

		if (process == 5 || process == 1)
		{
			if (process == 5)
			{
				System.Random rand = new System.Random();
				height = rand.Next(minHeight, maxHeight + 1); // get random height for the tree
				if (goingRight)
				{
					treeHeightRight = height;
					bottomPosRight = treeBottomYPos;
				}
				else
				{
					treeHeightLeft = height;
					bottomPosLeft = treeBottomYPos;
				}

			}

			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 7 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 1, 7 });
		}
		else if (process == 4 || process == 2)
		{
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height + 1, 7 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height, 7 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 1, 7 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 7 });
		}
		else if (process == 3)
		{
			float originalBottomPos = goingRight ? bottomPosRight : bottomPosLeft;

			float i;
			for (i = treeBottomYPos; i < originalBottomPos + height; i++)
			{
				if (i > originalBottomPos + height - 3) // spawn logs and leaves
				{
					blockPosAndID.Add(new float[] { xPos, i, 6 });
					blockPosAndID.Add(new float[] { xPos, i, 7 });
				}
				else blockPosAndID.Add(new float[] { xPos, i, 6 }); // 6 represents the logs
			}

			blockPosAndID.Add(new float[] { xPos, i, 7 }); // the two blocks above the tree are oak leaves
			blockPosAndID.Add(new float[] { xPos, i + 1, 7 });
		}
		else Debug.LogError("Process given is not a valid number, should be 1-5, but is: " + process);

		return blockPosAndID;
	}


	public static bool isSpawningTree(bool goingRight)
	{
		if (goingRight) return treeProgressRight > 0;
		return treeProgressLeft > 0;
	}
}
