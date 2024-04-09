using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerbarScript : MonoBehaviour
{
    
    private Image[] foodBackgrounds = new Image[10];
	private float hunger = 20f;
	private Sprite fullFood;
	private Sprite halfFood;

    private PlayerControllerScript playerControllerScript; // used to check if the player is running
	private HealthbarScript healthbarScript;

    private float hungerLossWhenWalking = .12f;
    private float hungerLossWhenRunning = .25f;

    private int lastUpdatedHunger = 20; // used to check if we need to update hunger, if the current hunger is +/- 1 away from this value, then we need to update 
	private bool isTakingHungerDamage = false; // TODO: need to save this to json file also
	private IEnumerator hungerDamageCoroutine = null;

	private static IDataService dataService = new JsonDataService();

	void Start()
    {
        for (int i = 0; i < foodBackgrounds.Length; i++)
        {
            foodBackgrounds[i] = transform.Find("Food" + i).GetComponent<Image>();
        }

        fullFood = Resources.Load<Sprite>("Textures/UI/food_full");
		halfFood = Resources.Load<Sprite>("Textures/UI/food_half");

        playerControllerScript = GameObject.Find("SteveContainer").GetComponent<PlayerControllerScript>();
		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerDamageCoroutine = healthbarScript.takeHungerDamage();

		// if a saved hunger file exist, then put the hunger as the value in the file
		if (dataService.exists("health-and-hunger-bar.json"))
		{
			hunger = dataService.loadData<float[]>("health-and-hunger-bar.json")[1];
			updateFoodImages();
		}

		StartCoroutine(decreaseHunger());
	}


	public void eatFood(int hungerGain)
	{
		hunger = Mathf.Min(20, hunger + hungerGain);
		updateFoodImages();
		

		if (isTakingHungerDamage)
		{
			healthbarScript.setIsTakingHungerDamage(false);
			StopCoroutine(hungerDamageCoroutine);
			isTakingHungerDamage = false;
		}
	}

    private IEnumerator decreaseHunger()
    {
		while (true)
		{
			yield return new WaitForSeconds(10f); // decreases hunger every 10 seconds
			bool isRunning = playerControllerScript.getIsRunning();

			if (isRunning) hunger = Mathf.Max(0, hunger - hungerLossWhenRunning);
			else hunger = Mathf.Max(0, hunger - hungerLossWhenWalking);

			float difference = Mathf.Abs(hunger - lastUpdatedHunger);
			if (difference >= 1f) updateFoodImages();  // we dont need to update hunger if difference < 1 because the food images would not change
		}
	}

    private void updateFoodImages()
    {
        lastUpdatedHunger = Mathf.RoundToInt(hunger);
		hunger = Mathf.Round(hunger);

		int i = 0;
		for (; i < Mathf.Floor(hunger / 2f); i++)
		{
			foodBackgrounds[i].transform.GetChild(0).GetComponent<Image>().sprite = fullFood;
			foodBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
		}
		if (hunger % 2 != 0) // if hunger is an odd number, then we need to display half a food
		{
			foodBackgrounds[i].transform.GetChild(0).GetComponent<Image>().sprite = halfFood;
			foodBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
			i++;
		}
		for (; i < 10; i++) // no fill image for the rest of the foods
		{
			foodBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0); // make it invisible
		}

		if (hunger <= 0)
		{
			startTakingHungerDamage();
		}
	}

	private void startTakingHungerDamage()
	{
		healthbarScript.setIsTakingHungerDamage(true);
		isTakingHungerDamage = true;
		StartCoroutine(hungerDamageCoroutine);
	}

	// player can eat if he has atleast lost half a hunger
	public bool canEat()
	{
		return hunger <= 19;
	}
	// player cant run if he has only 3 hungerbars left or less
	public bool canRun()
	{
		return hunger > 6;
	}

	public float getHunger()
	{
		return hunger;
	}
}
