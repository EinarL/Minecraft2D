using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * this script is responsible for moving the sunlight (the 2D spotlight) with the player and move it up
 * and down depending on the height of the landscape.
 */
public class SunLightMovementScript : MonoBehaviour
{
    private Transform playerPos;
    private List<float> chunkLowestHeights = new List<float>(); // lowest vertical line height for each chunk that is rendered
    private Coroutine adjustHeightCoroutine;
    private int targetHeight = 1940;

    // Start is called before the first frame update
    void Start()
    {
        playerPos = GameObject.Find("SteveContainer").transform;
        StartCoroutine(followPlayer());
    }

    public void addChunkHeight(float[] heights)
    {
		float lowHeight = getLowestHeight(heights);
		chunkLowestHeights.Add(lowHeight);
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
        if(adjustHeightCoroutine == null)
        {
            adjustHeightCoroutine = StartCoroutine(changeSunHeight());
        }
	}

    // follows player on the x axis
    private IEnumerator followPlayer()
    {
        while (true)
        {
            transform.position = new Vector2(playerPos.position.x, transform.position.y);
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator changeSunHeight()
    {
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
        adjustHeightCoroutine = null;
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
		float lowest = heights[0];
		for (int i = 1; i < heights.Count; i++)
		{
			if (heights[i] < lowest) lowest = heights[i];
		}

        return lowest;
	}
}
