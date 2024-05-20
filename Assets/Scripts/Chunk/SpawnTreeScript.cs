using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public static class SpawnTreeScript
{
	private static Dictionary<string, int> treeSpawnChance = new Dictionary<string, int>() { // odds of a tree spawning
		{"oak", 10 },
		{"spruce",30},
	};

	private static int minHeight = 4;
	private static int maxHeight = 7;
	private static int treeProgressRight = 0;
	private static int treeProgressLeft = 0;
	private static int treeHeightRight = 0; // the height of the tree that is currently being spawned on the right
	private static int treeHeightLeft = 0;
	private static float bottomPosRight; // the bottom index of the tree that is currently being spawned on the right
	private static float bottomPosLeft;

	private static System.Random random = new System.Random();

	/**
	 * decides if there should be a tree here, 
	 * 
	 * returns a list that tells what blocks to add to the previous lines, this is for spawning leaves in vLines that are already rendered
	 */
	public static List<float[]> decideIfSpawnTree(float treeBottomYPos, int chunkPos, float xPos, string tree = "oak")
	{
		List<float[]> blockPosAndID = new List<float[]>(); // {{x, y, blockID}, {x, y, blockID}, ...}
		float rand = Random.value * 100; // Generate a random value between 0 and 100

		bool goingRight = chunkPos >= 0;


		// dont spawn a tree at the first two blocks on the first chunk. this is to fix a bug where the tree doesnt spawn leaves on the left chunk because that chunk hasnt rendered yet 
		if (chunkPos == 0 && (xPos == 0.5f || xPos == 1.5f)) return new List<float[]>();

		// if we should spawn in a tree
		else if ((goingRight && treeProgressRight > 0) || (!goingRight && treeProgressLeft > 0) || rand < treeSpawnChance[tree])
		{
			if (tree.Equals("oak")) spawnOakTree(blockPosAndID, treeBottomYPos, goingRight, xPos);
			else if (tree.Equals("spruce")) spawnSpruceTree(blockPosAndID, treeBottomYPos, goingRight, xPos);
			else Debug.LogError("Error! variable tree is not a valid tree type");

			if (goingRight) treeProgressRight--;
			else treeProgressLeft--;
		}

		return blockPosAndID;
	}


	//---------------------------------------------------------------------------
	//								OAK
	//---------------------------------------------------------------------------

	private static int[] spawningOakTypeLeftAndRight = new int[] { -1, -1 }; // which oak tree we are in the process of spawning

	/**
	 * 
	 * returns blockPosAndID List<int[]> of type: {{x, y, blockID}, {x, y, blockID}, ...} which is info about the position
	 *		   of the blocks for the trees.
	 */
	private static void spawnOakTree(List<float[]> blockPosAndID, float treeBottomYPos, bool goingRight, float xPos)
	{
		int oakTypeIndex = goingRight ? 1 : 0;
		if (spawningOakTypeLeftAndRight[oakTypeIndex] == -1) // if we are NOT in the process of spawning a spruce tree
		{
			spawningOakTypeLeftAndRight[oakTypeIndex] = random.Next(0, 2);

			if (spawningOakTypeLeftAndRight[oakTypeIndex] == 0)
			{
				if (goingRight) treeProgressRight = 5;
				else treeProgressLeft = 5;
			}
			else if (spawningOakTypeLeftAndRight[oakTypeIndex] == 1)
			{
				if (goingRight) treeProgressRight = 3;
				else treeProgressLeft = 3;
			}

			if (goingRight) bottomPosRight = treeBottomYPos;
			else bottomPosLeft = treeBottomYPos;
		}

		if (spawningOakTypeLeftAndRight[oakTypeIndex] == 0)
		{
			float bottomPos = goingRight ? bottomPosRight : bottomPosLeft;
			// if the progress is at 3, i.e. where we spawn the logs, then we need to adjust the bottom position so that the logs will go down until they touch the ground
			if ((goingRight && treeProgressRight == 3) || (!goingRight && treeProgressLeft == 3)) bottomPos = treeBottomYPos;

			spawnOakTreeType0(blockPosAndID, bottomPos, goingRight, xPos);
		}

		int process = goingRight ? treeProgressRight : treeProgressLeft;
		if (process <= 1) spawningOakTypeLeftAndRight[oakTypeIndex] = -1; // if we finished spawning the tree

	}

	private static void spawnOakTreeType0(List<float[]> blockPosAndID, float treeBottomYPos, bool goingRight, float xPos)
	{
		int process = goingRight ? treeProgressRight : treeProgressLeft;
		int height = goingRight ? treeHeightRight : treeHeightLeft;

		if (process == 5 || process == 1)
		{
			if (process == 5)
			{
				height = random.Next(minHeight, maxHeight + 1); // get random height for the tree
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

			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 7 }); // 7 is id for oak leaves, this is right/left-most two leaves
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 1, 7 }); // upper leaf block
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
	}

	//---------------------------------------------------------------------------
	//								SPRUCE
	//---------------------------------------------------------------------------

	private static int[] spawningSpruceTypeLeftAndRight = new int[] { -1, -1 }; // which spruce tree we are in the process of spawning

	private static void spawnSpruceTree(List<float[]> blockPosAndID, float treeBottomYPos, bool goingRight, float xPos)
	{
		int spruceTypeIndex = goingRight ? 1 : 0;
		if (spawningSpruceTypeLeftAndRight[spruceTypeIndex] == -1) // if we are NOT in the process of spawning a spruce tree
		{
			spawningSpruceTypeLeftAndRight[spruceTypeIndex] = random.Next(0, 2); 

			if(spawningSpruceTypeLeftAndRight[spruceTypeIndex] == 0)
			{
				if (goingRight) treeProgressRight = 5;
				else treeProgressLeft = 5;
			}
			else if (spawningSpruceTypeLeftAndRight[spruceTypeIndex] == 1) 
			{
				if (goingRight) treeProgressRight = 3;
				else treeProgressLeft = 3;
			}

			if (goingRight) bottomPosRight = treeBottomYPos;
			else bottomPosLeft = treeBottomYPos;
		}

		if (spawningSpruceTypeLeftAndRight[spruceTypeIndex] == 0)
		{
			float bottomPos = goingRight ? bottomPosRight : bottomPosLeft;
			// if the progress is at 3, i.e. where we spawn the logs, then we need to adjust the bottom position so that the logs will go down until they touch the ground
			if ((goingRight && treeProgressRight == 3) || (!goingRight && treeProgressLeft == 3)) bottomPos = treeBottomYPos;

			spawnSpruceTreeType0(blockPosAndID, bottomPos, goingRight, xPos);
		}
		else if (spawningSpruceTypeLeftAndRight[spruceTypeIndex] == 1)
		{
			float bottomPos = goingRight ? bottomPosRight : bottomPosLeft;
			// if the progress is at 3, i.e. where we spawn the logs, then we need to adjust the bottom position so that the logs will go down until they touch the ground
			if ((goingRight && treeProgressRight == 2) || (!goingRight && treeProgressLeft == 2)) bottomPos = treeBottomYPos;

			spawnSpruceTreeType1(blockPosAndID, bottomPos, goingRight, xPos);
		}

		int process = goingRight ? treeProgressRight : treeProgressLeft;
		if(process <= 1) spawningSpruceTypeLeftAndRight[spruceTypeIndex] = -1; // if we finished spawning the tree

	}

	// looks like a christmas tree
	private static void spawnSpruceTreeType0(List<float[]> blockPosAndID, float treeBottomYPos, bool goingRight, float xPos)
	{
		int process = goingRight ? treeProgressRight : treeProgressLeft;
		int height = goingRight ? treeHeightRight : treeHeightLeft;

		if (process == 5 || process == 1)
		{
			if (process == 5)
			{
				height = random.Next(8, 11 + 1); // get random height for the tree
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

			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 31 }); // 31 is id for spruce leaves
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 4, 31 }); // spawns in the left-most or right-most leaves 
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 6, 31 });
		}
		else if (process == 4 || process == 2)
		{
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 1, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 3, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 4, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 5, 31 });
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 6, 31 });
		}
		else if (process == 3)
		{
			float originalBottomPos = goingRight ? bottomPosRight : bottomPosLeft;

			float i;
			for (i = treeBottomYPos; i < originalBottomPos + height; i++)
			{
				if (i > originalBottomPos + height - 7) // spawn logs and leaves
				{
					blockPosAndID.Add(new float[] { xPos, i, 30 });
					blockPosAndID.Add(new float[] { xPos, i, 31 });
				}
				else blockPosAndID.Add(new float[] { xPos, i, 30 }); // 30 represents the spruce logs
			}

			blockPosAndID.Add(new float[] { xPos, i, 31 }); // the two blocks above the tree are oak leaves
			blockPosAndID.Add(new float[] { xPos, i + 1, 31 });
		}
		else Debug.LogError("Process given is not a valid number, should be 1-5, but is: " + process);
	}

	// thin tree
	private static void spawnSpruceTreeType1(List<float[]> blockPosAndID, float treeBottomYPos, bool goingRight, float xPos)
	{
		int process = goingRight ? treeProgressRight : treeProgressLeft;
		int height = goingRight ? treeHeightRight : treeHeightLeft;

		if (process == 3 || process == 1)
		{
			if (process == 3)
			{
				height = random.Next(5, 8 + 1); // get random height for the tree
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

			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height, 31 }); // 31 is id for spruce leaves
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 1, 31 }); // spawns in the left-most or right-most leaves 
			blockPosAndID.Add(new float[] { xPos, treeBottomYPos + height - 2, 31 });
		}
		else if (process == 2)
		{
			float originalBottomPos = goingRight ? bottomPosRight : bottomPosLeft;

			float i;
			for (i = treeBottomYPos; i < originalBottomPos + height; i++)
			{
				if (i > originalBottomPos + height - 3) // spawn logs and leaves
				{
					blockPosAndID.Add(new float[] { xPos, i, 30 });
					blockPosAndID.Add(new float[] { xPos, i, 31 });
				}
				else blockPosAndID.Add(new float[] { xPos, i, 30 }); // 30 represents the spruce logs
			}

			blockPosAndID.Add(new float[] { xPos, i, 31 }); // the two blocks above the tree are oak leaves
			blockPosAndID.Add(new float[] { xPos, i + 1, 31 });
		}
		else Debug.LogError("Process given is not a valid number, should be 1-3, but is: " + process);
	}

	public static bool isSpawningTree(bool goingRight)
	{
		if (goingRight) return treeProgressRight > 0;
		return treeProgressLeft > 0;
	}
}
