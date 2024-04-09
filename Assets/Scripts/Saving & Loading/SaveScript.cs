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
	private HealthbarScript healthbarScript;
	private HungerbarScript hungerbarScript;
	private static IDataService dataService = new JsonDataService();

	// Start is called before the first frame update
	void Start()
    {
		openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.I))
		{
			InventoryScript.saveInventory(); // save inventory
			openFurnaceScript.saveFurnaces(); // save furnaces
											  
			int health = healthbarScript.getHealth();
			float hunger = hungerbarScript.getHunger();
			if (!dataService.saveData("health-and-hunger-bar.json", new float[] { health, hunger })) // save health bar and food bar
			{
				Debug.LogError("Could not save health and hunger bar file :(");
			}
		}
	}
}
