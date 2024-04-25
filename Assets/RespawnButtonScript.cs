using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnButtonScript : MonoBehaviour
{

	private CanvasScript canvasScript;
	private PlayerControllerScript playerControllerScript;
    private spawnChunkScript scScript;



	// Start is called before the first frame update
	void Start()
    {
		canvasScript = GameObject.Find("Canvas").transform.GetComponent<CanvasScript>();
		GameObject steveContainer = GameObject.Find("SteveContainer");
		playerControllerScript = steveContainer.gameObject.GetComponent<PlayerControllerScript>();
        scScript = GameObject.Find("Main Camera").transform.GetComponent<spawnChunkScript>();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void respawn()
    {
		InventoryScript.setIsInUI(false);
        canvasScript.closeDeathScreen(); // remove death screen
        playerControllerScript.removeDeathAnimation(); //  remove death animation
        // TODO: save inventory at the place of death
        // unrender chunks
        for(int i = scScript.getLeftmostChunkPos(); i < scScript.getLeftmostChunkPos() + (scScript.getAmountOfChunksRendered() * SpawningChunkData.blocksInChunk); i += SpawningChunkData.blocksInChunk)
        {
			scScript.unrenderChunk(i);
		}
		// render chunks at spawnpoint
		// TODO: save spawnpoint, because it will be different when beds are implemented
		scScript.renderChunk(0);
		scScript.renderChunk(-10);
		scScript.renderChunk(-20);
		scScript.renderChunk(10);
		// send steve to spawnpoint
		playerControllerScript.teleportToSpawn();

		// reset health and hunger and inventory
	}
}
