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

    // Start is called before the first frame update
    void Start()
    {
		openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.I))
		{
			InventoryScript.saveInventory(); // save inventory
			openFurnaceScript.saveFurnaces(); // save furnaces

		}
	}
}
