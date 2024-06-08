using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnButtonScript : MonoBehaviour
{

	private CanvasScript canvasScript;
	private PlayerControllerScript playerControllerScript;
	private HealthbarScript healthbarScript;
	private HungerbarScript hungerbarScript;
	private ArmorScript armorScript;
	private IDataService dataService = JsonDataService.Instance;

	private spawnChunkScript scScript;
	private GameObject mainCam;
	private CinemachineVirtualCamera vcam;


	// Start is called before the first frame update
	void Start()
    {
		canvasScript = GameObject.Find("Canvas").transform.GetComponent<CanvasScript>();
		GameObject steveContainer = GameObject.Find("SteveContainer");
		playerControllerScript = steveContainer.gameObject.GetComponent<PlayerControllerScript>();
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
		armorScript = GameObject.Find("Canvas").transform.Find("Armorbar").GetComponent<ArmorScript>();
		scScript = GameObject.Find("Main Camera").transform.GetComponent<spawnChunkScript>();
		mainCam = GameObject.Find("Main Camera");
		vcam = GameObject.Find("CM vcam").GetComponent<CinemachineVirtualCamera>();
	}

    public void respawn()
    {
		scScript.pauseChunkRendering = true; // pause the rendering of chunks
		InventoryScript.setIsInUI(false);
        playerControllerScript.removeDeathAnimation(); //  remove death animation

		// place tombstone at place of death
		createTombstone();

		Vector2 spawnPoint = new Vector2(0, 0);
		if (dataService.exists("spawn-point.json"))
		{
			float[] sp = dataService.loadData<float[]>("spawn-point.json");
			spawnPoint = new Vector2(sp[0], sp[1]);
		}

		int leftMostChunkToRenderAtSpawn = scScript.getChunkNumber(spawnPoint.x);
		int leftMostChunkToRenderAtDeathPosition = scScript.getLeftmostChunkPos();

		// unrender chunks
		for (int i = leftMostChunkToRenderAtDeathPosition; i < scScript.getLeftmostChunkPos() + (scScript.getAmountOfChunksRendered() * SpawningChunkData.blocksInChunk); i += SpawningChunkData.blocksInChunk)
        {
			if(i < leftMostChunkToRenderAtSpawn || i >= leftMostChunkToRenderAtSpawn + (10 * SpawningChunkData.blocksInChunk)) scScript.unrenderChunk(i); // unrender chunk if the chunk is not in the spawns position
		}

		vcam.m_Lens.OrthographicSize = 5; // reset zoom back to default
		float prevSoftZoneWidth = vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_SoftZoneWidth;
		vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_SoftZoneWidth = 0f;



		mainCam.transform.position = spawnPoint;

		playerControllerScript.teleportToSpawn(spawnPoint); // teleport steve to spawn

		scScript.loadSpawn(spawnPoint, leftMostChunkToRenderAtDeathPosition); // render chunks at spawnpoint

		// reset health and hunger
		healthbarScript.setFullHealth();
		healthbarScript.startHealCoroutine();
		hungerbarScript.setFullHunger();
		hungerbarScript.startHungerCoroutine();

		// reset inventory

		canvasScript.closeDeathScreen(); // remove death screen
		scScript.pauseChunkRendering = false; // resume chunk rendering
		scScript.setCamSettingsBackToNormal(vcam.GetCinemachineComponent<CinemachineFramingTransposer>(), prevSoftZoneWidth);
	}

	private void createTombstone()
	{ 
		Vector2 deathPos = playerControllerScript.gameObject.transform.position;
		float roundedXPos = (int)deathPos.x;

		if (deathPos.x < 0) roundedXPos -= .5f;
		else roundedXPos += .5f;

		float roundedYPos = Mathf.RoundToInt(deathPos.y);
		roundedYPos -= .5f;

		SpawningChunkData.updateChunkData(roundedXPos, roundedYPos, 23, "FrontBackground" ); // update chunk with the tombstone

		// save the contents of the tombstone (players inventory)
		// it will look like: [xPos, yPos, inventory]
		object[] tombstoneData = new object[] { roundedXPos, roundedYPos, InventoryScript.getInventory(), armorScript.getArmorSlots() };
		if (!dataService.appendToData("tombstone.json", tombstoneData)) // save tombstone data
		{
			Debug.LogError("Could not save tombstone file :(");
		}

		InventoryScript.setEmptyInventory();
		armorScript.removeAllArmor();
	}
}
