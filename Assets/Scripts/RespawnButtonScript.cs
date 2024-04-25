using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnButtonScript : MonoBehaviour
{

	private CanvasScript canvasScript;
	private PlayerControllerScript playerControllerScript;
	private HealthbarScript healthbarScript;
	private HungerbarScript hungerbarScript;

	private spawnChunkScript scScript;
	private GameObject mainCam;


	// Start is called before the first frame update
	void Start()
    {
		canvasScript = GameObject.Find("Canvas").transform.GetComponent<CanvasScript>();
		GameObject steveContainer = GameObject.Find("SteveContainer");
		playerControllerScript = steveContainer.gameObject.GetComponent<PlayerControllerScript>();
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
		scScript = GameObject.Find("Main Camera").transform.GetComponent<spawnChunkScript>();
		mainCam = GameObject.Find("Main Camera");
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void respawn()
    {
		InventoryScript.setIsInUI(false);
        playerControllerScript.removeDeathAnimation(); //  remove death animation
        // TODO: save inventory at the place of death
        // unrender chunks
        for(int i = scScript.getLeftmostChunkPos(); i < scScript.getLeftmostChunkPos() + (scScript.getAmountOfChunksRendered() * SpawningChunkData.blocksInChunk); i += SpawningChunkData.blocksInChunk)
        {
			scScript.unrenderChunk(i);
		}
		// send steve to spawnpoint
		playerControllerScript.teleportToSpawn();
		scScript.setAmountOfChunksToRender(4);
		scScript.setLeftmostChunkPos(-20);
		mainCam.transform.position = new Vector2(0, mainCam.transform.position.y);
		// render chunks at spawnpoint
		// TODO: save spawnpoint, because it will be different when beds are implemented
		scScript.renderChunk(0);
		scScript.renderChunk(-10);
		scScript.renderChunk(-20);
		scScript.renderChunk(10);


		// reset health and hunger
		healthbarScript.setFullHealth();
		healthbarScript.startHealCoroutine();
		hungerbarScript.setFullHunger();
		hungerbarScript.startHungerCoroutine();

		// reset inventory

		canvasScript.closeDeathScreen(); // remove death screen
	}
}
