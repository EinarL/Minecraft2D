using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * this class defines what happens when you right click a bed
 */
public class OpenDoorBehaviour : RightClickBlockBehaviour
{
	private GameObject block;
	private Vector2 blockPos;
	private Tilemap tilemap;
	private AudioSource doorAudioSource;
	private AudioClip closeDoorSound;
	private AudioClip openDoorSound;

	public OpenDoorBehaviour(GameObject block, Vector2 blockPos)
	{
		this.block = block;
		this.blockPos = blockPos;
		tilemap = GameObject.Find("Grid").transform.Find("Tilemap").GetComponent<Tilemap>();
		doorAudioSource = GameObject.Find("Audio").transform.Find("BreakBlockSound").GetComponent<AudioSource>();
		closeDoorSound = Resources.Load<AudioClip>("Sounds\\Random\\door_close");
		openDoorSound = Resources.Load<AudioClip>("Sounds\\Random\\door_open");
	}

	public void rightClickBlock()
	{
		bool isOpen = !block.name.Contains("Side");
		bool isTopDoorBlock = block.name.Contains("Top");
		playDoorSound(isOpen);

		string topDoorBlockName;
		string bottomDoorBlockName;

		if (!isOpen) // if the door is closed
        {
			if(isTopDoorBlock)
			{
				topDoorBlockName = block.name.Replace("Side", "");
				bottomDoorBlockName = block.name.Replace("Side", "").Replace("Top", "Bottom");
				replaceBlock(blockPos, topDoorBlockName, true, true);
				replaceBlock(new Vector2(blockPos.x, blockPos.y - 1), bottomDoorBlockName);

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, BlockHashtable.getIDByBlockName(topDoorBlockName), "FrontBackground");
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y - 1, BlockHashtable.getIDByBlockName(bottomDoorBlockName), "FrontBackground");

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, 0);
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y - 1, 0);

			}
			else {
				topDoorBlockName = block.name.Replace("Side", "").Replace("Bottom", "Top");
				bottomDoorBlockName = block.name.Replace("Side", "");
				replaceBlock(blockPos, bottomDoorBlockName, true, true);
				replaceBlock(new Vector2(blockPos.x, blockPos.y + 1), topDoorBlockName);

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, BlockHashtable.getIDByBlockName(bottomDoorBlockName), "FrontBackground");
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y + 1, BlockHashtable.getIDByBlockName(topDoorBlockName), "FrontBackground");

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, 0);
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y + 1, 0);
			}

		}
		else // if the door is open
		{
			if (isTopDoorBlock)
			{
				topDoorBlockName = block.name.Replace("Top", "TopSide");
				bottomDoorBlockName = block.name.Replace("Top", "TopSide").Replace("Top", "Bottom");
				replaceBlock(blockPos, topDoorBlockName, true, true);
				replaceBlock(new Vector2(blockPos.x, blockPos.y - 1), bottomDoorBlockName);

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, BlockHashtable.getIDByBlockName(topDoorBlockName));
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y - 1, BlockHashtable.getIDByBlockName(bottomDoorBlockName));

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, 0, "FrontBackground");
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y - 1, 0, "FrontBackground");
			}
			else
			{
				topDoorBlockName = block.name.Replace("Bottom", "BottomSide").Replace("Bottom", "Top");
				bottomDoorBlockName = block.name.Replace("Bottom", "BottomSide");
				replaceBlock(blockPos, bottomDoorBlockName, true, true);
				replaceBlock(new Vector2(blockPos.x, blockPos.y + 1), topDoorBlockName);

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, BlockHashtable.getIDByBlockName(bottomDoorBlockName));
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y + 1, BlockHashtable.getIDByBlockName(topDoorBlockName));

				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y, 0, "FrontBackground");
				SpawningChunkData.updateChunkData(blockPos.x, blockPos.y + 1, 0, "FrontBackground");
			}
		}

		

    }

	private void playDoorSound(bool closeSound)
	{
		if (closeSound) doorAudioSource.clip = closeDoorSound;
		else doorAudioSource.clip = openDoorSound;

		doorAudioSource.Play();
	}

	private void replaceBlock(Vector2 blockPos, string newBlockName, bool isGameObject = false, bool thisGameObject = false)
	{
		if (thisGameObject)
		{
			GameObject.Destroy(block.gameObject);
			spawnBlock(blockPos, newBlockName);
		}

		if(!isGameObject)
		{
			if (tilemap.HasTile(tilemap.WorldToCell(blockPos))) { // if there is a tile
				tilemap.SetTile(tilemap.WorldToCell(blockPos), null); // remove it
				spawnBlock(blockPos, newBlockName);
				return;
			}
		}

		// Create a list to store the results
		List<Collider2D> blocks = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "FrontBackground" or "BackBackground"

		// Check for overlaps
		Physics2D.OverlapCircle(blockPos, 0.45f, filter, blocks);

		foreach (Collider2D col in blocks)
		{
			if (col.gameObject.name.Contains("Door")) // if its a door
			{
				spawnBlock(blockPos, newBlockName);
				GameObject.Destroy(col.gameObject);
				return;
			}
		}
	}

	private void spawnBlock(Vector2 blockPos, string blockName)
	{
		GameObject blockToSpawn = Resources.Load<GameObject>("Prefabs\\Blocks\\" + blockName);
		GameObject.Instantiate(blockToSpawn, blockPos, Quaternion.identity);
	}
}