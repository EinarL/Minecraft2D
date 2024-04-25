using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript : MonoBehaviour
{

    private Image[] heartBackgrounds = new Image[10]; // the background images for the hearts, the child of a background image is the fill image
	private int health = 20;
    private Sprite fullHeart;
	private Sprite halfHeart;
    private AudioClip[] hitSounds = new AudioClip[3]; // sounds when taking damage
    private AudioSource audioSource;
    private bool isTakingHungerDamage = false;

	private Sprite normalBackground;
	private Sprite whiteBackground; // background for the hearts

    private GameObject steve;
    private PlayerControllerScript playerControllerScript;
    private HungerbarScript hungerbarScript;
    private CanvasScript canvasScript;

	private static IDataService dataService = new JsonDataService();


	// Start is called before the first frame update
	void Start()
    {
        for (int i = 0; i < heartBackgrounds.Length; i++)
        {
            heartBackgrounds[i] = transform.Find("Heart" + i).GetComponent<Image>();
        }
        Sprite[] heartImages = Resources.LoadAll<Sprite>("Textures/UI/icons");
        
		halfHeart = getSpriteWithName(heartImages, "icons_2");
		fullHeart = getSpriteWithName(heartImages, "icons_1");

        normalBackground = getSpriteWithName(heartImages, "icons_0");
		whiteBackground = getSpriteWithName(heartImages, "icons_3");

        GameObject steveContainer = GameObject.Find("SteveContainer");
		steve = steveContainer.transform.Find("Steve").gameObject;
		playerControllerScript = steveContainer.gameObject.GetComponent<PlayerControllerScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
        canvasScript = GameObject.Find("Canvas").transform.GetComponent<CanvasScript>();


		for (int i = 0; i < hitSounds.Length; i++)
		{
			hitSounds[i] = Resources.Load<AudioClip>($"Sounds/Damage/hit{i + 1}");
		}
        audioSource = gameObject.GetComponent<AudioSource>();

        // if a saved health file exist, then put the health as the value in the file
		if (dataService.exists("health-and-hunger-bar.json"))
		{
			health = (int)dataService.loadData<float[]>("health-and-hunger-bar.json")[0];
            updateHeartImages();
		}

		StartCoroutine(healUp());
	}

	public void takeDamage(int damage)
    {
        health -= damage;
        updateHeartImages();
        playDamageSound();
        displayTint();
        StartCoroutine(removeRedTint());
        if (health > 0) StartCoroutine(flashingAnimationCoroutine());
	}

    // updates the heart images to display how much health the player has left
    private void updateHeartImages()
    {
        int i;
        for (i = 0; i < Mathf.Floor(health/2f); i++)
        {
            heartBackgrounds[i].transform.GetChild(0).GetComponent<Image>().sprite = fullHeart;
			heartBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
		}
        if (health % 2 != 0) // if health is an odd number, then we need to display half a heart
        {
			heartBackgrounds[i].transform.GetChild(0).GetComponent<Image>().sprite = halfHeart;
			heartBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
			i++;
		}
        for(;i < 10; i++) // no fill image for the rest of the hearts
        {
			heartBackgrounds[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1,1,1,0); // make it invisible
		}

        if(health <= 0)
        {
            die();
		}
    }

    private void die()
    {
		playerControllerScript.die(); // so that the death animation occurs
		canvasScript.showDeathScreen();
		StopAllCoroutines(); // stop healing and other stuff
		InventoryScript.setIsInUI(true);
        health = 0;
	}

    // Coroutine that flashes the hearts to be white when taking damage
    private IEnumerator flashingAnimationCoroutine()
    {
        bool turnWhite = true;
        for (int i = 0; i < 6; i++)
        {
			for (int j = 0; j < heartBackgrounds.Length; j++)
			{
				heartBackgrounds[j].sprite = turnWhite ? whiteBackground : normalBackground;
			}
            turnWhite = !turnWhite;
			if (i != 5) yield return new WaitForSeconds(.2f);
		}
    }

    // different kind od flash animation when the player is healing
    private IEnumerator flashAnimationHealing()
    {
        int howManyHearts = health % 2 == 0 ? health/2 : (health + 1)/2;
		bool turnWhite = true;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < howManyHearts; j++)
			{
				heartBackgrounds[j].sprite = turnWhite ? whiteBackground : normalBackground;
			}
			turnWhite = !turnWhite;
			if (i != 3) yield return new WaitForSeconds(.1f);
		}
	}


    // take damage because of hunger
    public IEnumerator takeHungerDamage()
    {
        while (isTakingHungerDamage)
        {
            if (health > 1) takeDamage(1);
            yield return new WaitForSeconds(2);
        }
    }
    // checks if the player should heal
    private IEnumerator healUp()
    {
        while (true){
            yield return new WaitForSeconds(6);
			if (health < 20 && hungerbarScript.getHunger() > 17)
            {
                heal(1);
            }
        }
    }

    private void heal(int healAddition)
    {
        health = Mathf.Min(20, health + healAddition);
        updateHeartImages();
        StartCoroutine(flashAnimationHealing());
    }

    public void setIsTakingHungerDamage(bool isTakingHungerDamage)
    {
        this.isTakingHungerDamage = isTakingHungerDamage;
    }


    private Sprite getSpriteWithName(Sprite[] list, string name)
    {
        for (int i = 0; i < list.Length; i++) {
            if (list[i].name == name)
            {
                return list[i];
            }
        }
        Debug.LogError("ERROR: sprite with name " + name + " was not located in the icons sprite");
        return null;
    }

	/**
 * displays a red tint on the entity if red remains true, otherwise returns the entity color back to normal
 */
	private void displayTint(bool red = true)
	{
		SpriteRenderer[] spriteRenderers = steve.GetComponentsInChildren<SpriteRenderer>();
		Color color;
		if (red) color = new Color(1, 179f / 255f, 179f / 255f);
		else color = Color.white;

		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			spriteRenderer.color = color;
		}
	}

	private IEnumerator removeRedTint()
	{
		yield return new WaitForSeconds(.4f);
		displayTint(false);
	}

    private void playDamageSound()
    {
		var random = new System.Random();
		int randIndex = random.Next(hitSounds.Length);
		AudioClip randClip = hitSounds[randIndex];
		audioSource.clip = randClip;
		audioSource.Play();
	}

    public int getHealth()
    {
        return health;
    }
}
