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
		if(!dayProcessScript.canSleep()) { // if the player cannot sleep then display a message telling the player that
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