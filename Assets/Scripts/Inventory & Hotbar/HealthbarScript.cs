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
 

    // Start is called before the first frame update
    void Start()
    {
        // TODO: load health save if it exists
        for (int i = 0; i < heartBackgrounds.Length; i++)
        {
            heartBackgrounds[i] = transform.Find("Heart" + i).GetComponent<Image>();
        }
        Sprite[] heartImages = Resources.LoadAll<Sprite>("Textures/UI/icons");
        
		halfHeart = getSpriteWithName(heartImages, "icons_2");
		fullHeart = getSpriteWithName(heartImages, "icons_1");

        normalBackground = getSpriteWithName(heartImages, "icons_0");
		whiteBackground = getSpriteWithName(heartImages, "icons_3");

        steve = GameObject.Find("SteveContainer").transform.Find("Steve").gameObject;

		for (int i = 0; i < hitSounds.Length; i++)
		{
			hitSounds[i] = Resources.Load<AudioClip>($"Sounds/Damage/hit{i + 1}");
		}
        audioSource = gameObject.GetComponent<AudioSource>();
	}

	public void takeDamage(int damage)
    {
        health -= damage;
        updateHeartImages();
        playDamageSound();
        displayTint();
        StartCoroutine(removeRedTint());
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
            Debug.Log("Health is less or equal to zero, you died!");
            // TODO: implement death
        }
        else
        {
            StartCoroutine(flashingAnimationCoroutine());
        }
    }
    // Coroutine that flashes the hearts to be white
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
    // take damage because of hunger
    public IEnumerator takeHungerDamage()
    {
        while (isTakingHungerDamage)
        {
            if (health > 1) takeDamage(1);
            yield return new WaitForSeconds(2);
        }
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
}
