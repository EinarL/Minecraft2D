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
    private List<float> chunkAvgHeights = new List<float>(); // avg heights of the chunks that are rendered
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
		float avgHeight = getAverageHeight(heights);
        chunkAvgHeights.Add(avgHeight);
	}

	public void removeChunkHeight(float[] heights)
    {
        float avgHeight = getAverageHeight(heights);
        bool removed = chunkAvgHeights.Remove(avgHeight);
        if (!removed) Debug.LogError("Avg height " + avgHeight + " wasn't found in the list, so it wasn't removed. The list: " + chunkAvgHeights);
           
        float overallAvgHeight = getAverageHeight(chunkAvgHeights);
        adjustSunHeight((int)Mathf.Round(overallAvgHeight));
    }

    private void adjustSunHeight(int avgChunkHeight)
    {
        
        targetHeight = 1940 + avgChunkHeight + 1;
        if(adjustHeightCoroutine == null)
        {
            adjustHeightCoroutine = StartCoroutine(changeSunHeight());
        }
	}


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

	private float getAverageHeight(float[] heights)
	{
		float avg = 0;
		foreach (float height in heights)
		{
			avg += height;
		}

		return avg / SpawningChunkData.blocksInChunk;
	}

	private float getAverageHeight(List<float> heights)
	{
		float avg = 0;
		foreach (float height in heights)
		{
			avg += height;
		}

		return avg / SpawningChunkData.blocksInChunk;
	}
}
