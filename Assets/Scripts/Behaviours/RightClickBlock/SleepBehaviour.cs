using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this class defines what happens when you right click a bed
 */
public class SleepBehaviour : RightClickBlockBehaviour
{
	private string blockName;
	private Vector2 blockPos;
	private DayProcessScript dayProcessScript;
	private MessageScript messageScript;
	private Transform player;
	private PlayerControllerScript playerController;

	private IDataService dataService = JsonDataService.Instance;

	public SleepBehaviour(string blockName, Vector2 blockPos)
	{
		this.blockName = blockName;
		this.blockPos = blockPos;
		dayProcessScript = GameObject.Find("CM vcam").transform.Find("SunAndMoonTexture").GetComponent<DayProcessScript>();
		messageScript = GameObject.Find("Canvas").transform.Find("Message").GetComponent<MessageScript>();
		player = GameObject.Find("SteveContainer").transform;
		playerController = GameObject.Find("SteveContainer").GetComponent<PlayerControllerScript>();
	}

	public void rightClickBlock()
	{

		// save [x,y, x1, y1, x2, y2] where x and y is the spawn point and (x1, y1) is bedBlock1 pos, and (x2, y2) is bedBlock2 pos
		if(blockName.EndsWith("LowerRight") || blockName.EndsWith("UpperLeft")) // if its the left bed block
		{
			dataService.saveData("spawn-point.json", new float[] { player.position.x, player.position.y, blockPos.x, blockPos.y, blockPos.x + 0.5f, blockPos.y });
		}
		else // if its the right bed block
		{
			dataService.saveData("spawn-point.json", new float[] { player.position.x, player.position.y, blockPos.x, blockPos.y, blockPos.x - 0.5f, blockPos.y });
		}
		// TODO: display a message to the player that the spawnpoint is saved

		if (!dayProcessScript.canSleep()) { // if the player cannot sleep then display a message telling the player that
			messageScript.displayMessage("You cannot sleep during the day"); 
			return;
		}
		Debug.Log("go to sleep");
		// otherwise sleep
		playerController.goToSleep();
		float xOffset = 0f;
		if (blockName.EndsWith("Right"))
		{
			playerController.rotatePlayer(false);
			if (blockName.Contains("Lower")) xOffset = 0.6f;
			else xOffset = -0.55f;
		}
		else
		{
			playerController.rotatePlayer(true);
			if (blockName.Contains("Lower")) xOffset = -0.6f;
			else xOffset = 0.55f;
		}
		player.transform.position = new Vector2(blockPos.x + xOffset, blockPos.y);


	}
}