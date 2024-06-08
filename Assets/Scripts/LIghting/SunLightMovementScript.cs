using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this script is responsible for moving the sunlight (the 2D spotlight) with the player and move it up
 * and down depending on the height of the landscape.
 * 
 * also moves the void (a black colored gameobject) with the x axis of the player
 */
public class SunLightMovementScript : MonoBehaviour
{
    private Transform playerPos;
    private List<float> chunkLowestHeights = new List<float>(); // lowest vertical line height for each chunk that is rendered
    private Transform minecraftVoid;
    private int targetHeight = 1940;
    private bool isChangeSunHeightCoroutineRunning = false;
    private MainThreadDispatcher mainThreadDispatcher;
    private bool adjustAtStart = true; // adjust the suns position at the start of the game
    private int adjustAtStartCounter = 0;

	// Start is called before the first frame update
	void Start()
    {
        playerPos = GameObject.Find("SteveContainer").transform;
        minecraftVoid = GameObject.Find("Void").transform;
		mainThreadDispatcher = GameObject.Find("EventSystem").GetComponent<MainThreadDispatcher>();
        StartCoroutine(followPlayer());

        IEnumerator adjustPositionAtStart()
        {
            yield return new WaitForSeconds(0.1f);
            minecraftVoid.position = new Vector2(playerPos.position.x, minecraftVoid.position.y);
		}
		StartCoroutine(adjustPositionAtStart());

	}

    public void addChunkHeight(float[] heights)
    {
		float lowHeight = getLowestHeight(heights);
		chunkLowestHeights.Add(lowHeight);
        if (adjustAtStart)
        {
            adjustAtStartCounter++;
			float lowestHeight = getLowestHeight(chunkLowestHeights);
			transform.position = new Vector2(playerPos.position.x, 1940 + lowestHeight + 3);

            if(adjustAtStartCounter % 10 == 0) adjustAtStart = false;
		}
	}

	public void removeChunkHeight(float[] heights)
    {
        float lowHeight = getLowestHeight(heights);
        bool removed = chunkLowestHeights.Remove(lowHeight);
        if (!removed) Debug.LogError("Lowest height " + lowHeight + " wasn't found in the list, so it wasn't removed. The list: " + chunkLowestHeights);
           
        float lowestHeight = getLowestHeight(chunkLowestHeights);
        adjustSunHeight((int)Mathf.Round(lowestHeight));
    }

    private void adjustSunHeight(int lowestChunkHeight)
    {
        targetHeight = 1940 + lowestChunkHeight + 3;
        if(!isChangeSunHeightCoroutineRunning)
        {
			mainThreadDispatcher.enqueue(changeSunHeight());
        }
	}

    // follows player on the x axis
    private IEnumerator followPlayer()
    {
        while (true)
        {
            transform.position = new Vector2(playerPos.position.x, transform.position.y);
			minecraftVoid.position = new Vector2(playerPos.position.x, minecraftVoid.position.y);
			yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator changeSunHeight()
    {
        isChangeSunHeightCoroutineRunning = true;
        while (transform.position.y + 0.2f < targetHeight || transform.position.y - 0.2f > targetHeight)
        {
            if(targetHeight > transform.position.y)
            {
				transform.position = new Vector2(transform.position.x, transform.position.y + 0.01f);
            }
            else
            {
				transform.position = new Vector2(transform.position.x, transform.position.y - 0.01f);
			}
			
			yield return new WaitForSeconds(0.01f);
        }
		isChangeSunHeightCoroutineRunning = false;
	}

	private float getLowestHeight(float[] heights)
	{
		float lowest = heights[0];
        for (int i = 1; i < heights.Length; i++)
        {
            if (heights[i] < lowest) lowest = heights[i];
        }

        return lowest;
	}

	private float getLowestHeight(List<float> heights)
	{
        if (heights.Count == 0) return -2.5f;

		float lowest = heights[0];
		for (int i = 1; i < heights.Count; i++)
		{
			if (heights[i] < lowest) lowest = heights[i];
		}

        return lowest;
	}
}
