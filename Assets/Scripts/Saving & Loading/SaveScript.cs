using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * this script is responsible for saving inventory and furnaces (in the future: chests, unsaved chunks, etc.)
 * this script is not used for saving chunks when unrendering/rendering them.
 * 
 */
public class SaveScript : MonoBehaviour
{

	private OpenFurnaceScript openFurnaceScript;
	private OpenChestScript openChestScript;
	private HealthbarScript healthbarScript;
	private HungerbarScript hungerbarScript;
	private DayProcessScript dayProcessScript;
	private ArmorScript armorScript;
	private static IDataService dataService = JsonDataService.Instance;
	private Transform steve;

	// Start is called before the first frame update
	void Start()
    {
		openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();
		openChestScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenChestScript>();
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
		dayProcessScript = GameObject.Find("CM vcam").transform.Find("SunAndMoonTexture").GetComponent<DayProcessScript>();
		armorScript = GameObject.Find("Canvas").transform.Find("Armorbar").GetComponent<ArmorScript>();
		steve = GameObject.Find("SteveContainer").transform;
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.I))
		{
			save();
		}
	}

	// saves everything except the chunks
	public void save()
	{
		InventoryScript.saveInventory(); // save inventory
		openChestScript.saveChests();
		openFurnaceScript.saveFurnaces(); // save furnaces
		armorScript.saveArmor(); // save which armor you have on

		int health = healthbarScript.getHealth();
		float hunger = hungerbarScript.getHunger();
		if (!dataService.saveData("health-and-hunger-bar.json", new float[] { health, hunger })) // save health bar and food bar
		{
			Debug.LogError("Could not save health and hunger bar file :(");
		}
		// save day time
		if (!dataService.saveData("day-time.json", dayProcessScript.getDataToSave()))
		{
			Debug.LogError("Could not save day time file :(");
		}

		// save player position
		if (!dataService.saveData("player-position.json", new float[] { steve.position.x, steve.position.y }))
		{
			Debug.LogError("Could not save player position :(");
		}
	}
}
