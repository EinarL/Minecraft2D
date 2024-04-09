using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EatScript : MonoBehaviour
{

    private bool isEating = false;
	private Animator anim;
	private AudioSource eatAudioSource;
    private AudioClip[] eatAudioClips;
    private AudioClip burpClip;

	// the slot with the food when the player started eating
	// we need to have this to check if the player switches slots when eating, then the player should stop eating
	private int selectedSlotWhenStartedEating = -1;

    private HungerbarScript hungerbarScript;

    // Start is called before the first frame update
    void Start()
    {
        hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
        anim = GetComponent<Animator>();
        eatAudioSource = GameObject.Find("Audio").transform.Find("BreakBlockSound").GetComponent<AudioSource>();
        eatAudioClips = new AudioClip[3];
        for(int i = 0; i < eatAudioClips.Length; i++)
        {
            eatAudioClips[i] = Resources.Load("Sounds\\Steve\\eat" + i) as AudioClip;
		}
        burpClip = Resources.Load("Sounds\\Steve\\burp") as AudioClip;
	}

    // Update is called once per frame
    void Update()
    {
        // if clicking right click and the player is holding food
        if (Input.GetMouseButtonDown(1) && InventoryScript.isHoldingFood()) // TODO: check if full, then you cant eat
        {
            StartCoroutine(finishEating());
            selectedSlotWhenStartedEating = InventoryScript.getSelectedSlot();
        }
		// if the player is eating && (he is not holding down right click || switched selected slots in hotbar)
		else if (isEating && (!Input.GetMouseButton(1) || selectedSlotWhenStartedEating != InventoryScript.getSelectedSlot())) 
        {
            StopAllCoroutines(); // stop eating
            isEating = false;
            anim.SetBool("isEating", false);
            // TODO: eating particles
        }
        else if (isEating && !eatAudioSource.isPlaying)
        {
			var rand = new System.Random();
			int randIndex = rand.Next(eatAudioClips.Length);
			eatAudioSource.clip = eatAudioClips[randIndex];
			eatAudioSource.Play();
		}
    }

    private IEnumerator finishEating()
    {
        isEating = true;
		anim.SetBool("isEating", true);
		yield return new WaitForSeconds(2);
		int foodAddition = FoodHashtable.getFoodAddition(InventoryScript.getHeldItemName());
        Assert.IsTrue(foodAddition >= 0);
		InventoryScript.decrementSlot(InventoryScript.getSelectedSlot()); // remove food

        hungerbarScript.eatFood(foodAddition); // restore hunger
        if (InventoryScript.isHoldingFood()) // TODO: check if full, then you cant eat again
        {
            StartCoroutine(finishEating());
            yield break;
        }
        isEating = false;
		anim.SetBool("isEating", false);
	}

    
}
