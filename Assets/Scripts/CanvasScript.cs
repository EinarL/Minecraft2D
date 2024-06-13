using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    GameObject deathScreen;
    PlayerControllerScript playerController;
    GameObject gameMenuScreen;
    bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        deathScreen = gameObject.transform.Find("DeathScreen").gameObject;
		playerController = GameObject.Find("SteveContainer").GetComponent<PlayerControllerScript>();
        gameMenuScreen = gameObject.transform.Find("GameMenu").gameObject;

	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
				closeMenuAndResumeGame();

			}
			else if (playerController.isSleeping()) // stop sleeping
			{
				playerController.stopSleeping();
			}
			else if (!InventoryScript.getIsInUI())
            {
                openMenuAndPauseGame();
            }

        }

	}

    private void openMenuAndPauseGame()
    {
		Time.timeScale = 0;
        isPaused = true;
		gameMenuScreen.SetActive(true);
		InventoryScript.setIsInUI(true);
	}

    public void closeMenuAndResumeGame()
    {
		Time.timeScale = 1;
        isPaused = false;
		gameMenuScreen.SetActive(false);
		InventoryScript.setIsInUI(false);
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
