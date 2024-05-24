using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    GameObject deathScreen;
    PlayerControllerScript playerController;

    // Start is called before the first frame update
    void Start()
    {
        deathScreen = gameObject.transform.Find("DeathScreen").gameObject;
		playerController = GameObject.Find("SteveContainer").GetComponent<PlayerControllerScript>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
        {

			if (playerController.isSleeping()) // stop sleeping
			{
				playerController.stopSleeping();
			}
			else if (!InventoryScript.getIsInUI())
            {
                Debug.Log("open menu");
            }

        }

	}

    public void showDeathScreen()
    {
        deathScreen.SetActive(true);
    }

    public void closeDeathScreen()
    {
        deathScreen.SetActive(false);
    }
}
