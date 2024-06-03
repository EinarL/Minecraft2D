using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RightClickSnowball : RightClickItemBehaviour
{
	private Animator anim;
	private Transform throwFromTransform;
	private CoroutineManager cManager;

	public RightClickSnowball()
	{
		anim = GameObject.Find("SteveContainer").transform.Find("Steve").GetComponent<Animator>();
		throwFromTransform = GameObject.Find("SteveContainer").transform.Find("ThrowFromPosition").transform;
		cManager = GameObject.Find("EventSystem").GetComponent<CoroutineManager>();
	}

	public override void rightClickItem()
	{
		doThrowAnimation();

		IEnumerator throwSnowball(int theSlotThatTheSnowballIsIn)
		{
			yield return new WaitForSeconds(0.1f);
			GameObject snowballPrefab = Resources.Load<GameObject>("Prefabs\\Throwables\\Snowball");
			Vector2 mousePos = getMousePosition();
			Vector2 playerPos = throwFromTransform.position; // the position where the snowball will be thrown from

			// Instantiate the snowball at the player's position
			GameObject snowball = GameObject.Instantiate(snowballPrefab, playerPos, Quaternion.identity);

			// Calculate the direction from the player to the mouse position
			Vector2 direction = (mousePos - playerPos).normalized;

			// Set the velocity or add force to the snowball to make it move towards the mouse position
			float throwForce = 15f;
			Rigidbody2D rb = snowball.GetComponent<Rigidbody2D>();

			rb.velocity = direction * throwForce;

			InventoryScript.decrementSlot(theSlotThatTheSnowballIsIn); // remove the snowball from the inventory
		}
		cManager.startCoroutine(throwSnowball(InventoryScript.getSelectedSlot()));

		playThrowSound();
	}

	private void playThrowSound()
	{
		// Create an empty GameObject at the desired position
		GameObject temporaryAudioSource = new GameObject("TempAudio");
		temporaryAudioSource.transform.position = throwFromTransform.position;

		// Add an AudioSource component to the GameObject
		AudioSource audioSource = temporaryAudioSource.AddComponent<AudioSource>();
		AudioClip throwSound = Resources.Load<AudioClip>("Sounds\\Random\\bow");

		// Set the audio clip to the AudioSource
		audioSource.clip = throwSound;

		// Set the pitch of the AudioSource
		audioSource.pitch = 0.5f;
		audioSource.volume = 0.5f;

		// Play the audio clip
		audioSource.Play();

		// Destroy the GameObject after the clip finishes playing
		GameObject.Destroy(temporaryAudioSource, throwSound.length / audioSource.pitch);
	}

	private void doThrowAnimation()
	{
		bool facingRight = anim.GetBool("isFacingRight");

		if (facingRight) anim.Play("throwFrontArm");
		else anim.Play("throwBackArm");
	}

	// this function should be empty:

	public override void stopHoldingRightClick() { }
}
