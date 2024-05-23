using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	protected float speed = 4;
	protected float jumpPower = 10f; // how high the entity jumps

	protected float startWalkingChance = .8f; // 0.4
	protected bool isWalking = false;
	protected bool justWalked = false; // so that it doesnt decide to walk again immediately after finishing walking
	protected Vector3 direction;
	protected int[] walkingTime = new int[] { 1,5}; // min and max walking time
	protected float health = 10;
	protected float makeNoiseChance = 0.04f;

	//public AudioClip[] stepSounds = new AudioClip[5];
	protected AudioClip[] saySounds = new AudioClip[3];
	//public AudioSource stepAudioSource;
	protected AudioSource sayAudioSource;

	protected Rigidbody2D rb;
	protected Transform higherBlockCheck; // this checks if there is a 2-high block in the way of where the entity is going
	protected Transform lowerBlockCheck; // this checks if there is a block where the entity is going (then it needs to jump if there is no higher block)
	protected Animator anim;
	protected Coroutine walkingCoroutine;
	protected Transform playerTransform;

	public void initializeEntity()
	{
		higherBlockCheck = transform.Find("HigherBlockCheck");
		lowerBlockCheck = transform.Find("LowerBlockCheck");
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		sayAudioSource = GetComponent<AudioSource>();
		playerTransform = GameObject.Find("SteveContainer").transform;

		walkingCoroutine = StartCoroutine(decideIfWalk());
		//StartCoroutine(makeStepSound());
	}

	public void Update()
	{
		// If the entity is walking, move it in the walk direction
		if (isWalking)
		{
			if (!isPathBlocked())
			{
				if (isWalkingOffTheEdge())
				{
					direction = new Vector3(direction.x * -1, 0, 0);
					faceDirection();
				}
				rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

			}
			else // if path is blocked
			{
				stopWalking();
			}

			if (isBlockInPath()) jump();
		}
	}

	public virtual void takeDamage(float damage, float playerXPos)
	{
		if (anim.GetBool("isDead")) return;
		health -= damage;
		displayTint();
		takeKnockBack(playerXPos);
		if (health <= 0) die();
		StartCoroutine(removeRedTint());
	}

	/**
	 * displays a red tint on the entity if red remains true, otherwise returns the entity color back to normal
	 */
	private void displayTint(bool red = true)
	{
		SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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
		yield return new WaitForSeconds(.2f);
		displayTint(false);
	}

	private void takeKnockBack(float playerXPos)
	{
		bool knockBackRight = playerXPos < transform.position.x;
		if(knockBackRight) rb.velocity = new Vector2(10, 5);
		else rb.velocity = new Vector2(-10, 5);
	}

	public virtual void die()
	{
		isWalking = false;
		anim.SetBool("isDead", true);
		anim.SetBool("isWalking", false);
		
		StopAllCoroutines();
		// call death coroutine
		StartCoroutine(destroyEntity());
	}

	/**
	 * drops loot and destroys the gameobject
	 */
	public abstract IEnumerator destroyEntity();

	/**
	 * drops item from the entity, called when the entity dies
	 */
	public void dropItem(string itemName)
	{
		GameObject itemContainer = Resources.Load<GameObject>("Prefabs\\ItemContainer"); // get itemContainer
		GameObject item = itemContainer.transform.Find("Item").gameObject; // get item within itemContainer


		Sprite itemImage = Resources.Load<Sprite>("Textures\\ItemTextures\\" + itemName); // get the image for the item
		item.GetComponent<SpriteRenderer>().sprite = itemImage; // put the image on the SpriteRenderer

		// maybe do some random point to spawn the item at?
		Instantiate(itemContainer, new Vector2(transform.position.x, transform.position.y), itemContainer.transform.rotation); // spawn the item
	}

	/**
	 * returns true if the entity is about to walk off of the left or right most chunk
	 */
	public bool isWalkingOffTheEdge()
	{
		if (direction.x < 0 && transform.position.x <= SpawningChunkData.getLeftMostChunkEdge() + 1) return true; // if going left 
		if (direction.x > 0 && transform.position.x >= SpawningChunkData.getRightMostChunkEdge() - 1) return true; // if going right
		return false;
	}

	public void jump()
	{
		rb.velocity = new Vector2(rb.velocity.x, jumpPower);
	}

	public void stopWalking()
	{
		isWalking = false;
		anim.SetBool("isWalking", false);
	}

	protected bool isFacingRight()
	{
		return transform.rotation.y == -1;
	}

	public void faceDirection()
	{
		bool facingRight = isFacingRight();
		bool goingRight = direction.x > 0;

		if(goingRight && !facingRight) // if going right && the entity is not facing right
		{
			turnRight();
		}
		else if(!goingRight && facingRight) // if going left && the entity is not facing left
		{
			turnLeft();
		}
	}

	protected void turnRight()
	{
		var rotationVector = transform.rotation.eulerAngles;
		rotationVector.y = 180;
		transform.rotation = Quaternion.Euler(rotationVector); // rotate
	}

	protected void turnLeft()
	{
		var rotationVector = transform.rotation.eulerAngles;
		rotationVector.y = 0;
		transform.rotation = Quaternion.Euler(rotationVector); // rotate
	}

	/**
	 * checks if there is a block above and next to the path the entity is going to
	 * 
	 * this needs to be edited for small animals like chickens because they can enter 1-block high areas
	 */
	public virtual bool isPathBlocked()
	{
		return Physics2D.OverlapCircle(higherBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
	}

	/**
	 * returns true if there is only a 1-high block in the way, not if there are 2 blocks in the way
	 */
	public bool isBlockInPath()
	{
		// if there is a block in the way && there is not a block above the block that is in the way
		return Physics2D.OverlapCircle(lowerBlockCheck.position, 0.05f, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap")) && !isPathBlocked();
	}

	protected virtual IEnumerator decideIfMakeNoise()
	{
		while (true)
		{
			// if the entity is within 10 blocks on the y value, then it can make noise
			if(Mathf.Abs(playerTransform.position.y - transform.position.y) <= 10)
			{
				float rand = Random.value;
				if (rand < makeNoiseChance) makeNoise();
			}
			yield return new WaitForSeconds(2.5f); // Wait
		}
	}

	protected void makeNoise()
	{
		var random = new System.Random();
		int randIndex = random.Next(saySounds.Length);
		AudioClip randClip = saySounds[randIndex];
		sayAudioSource.clip = randClip;
		sayAudioSource.Play();
	}

	// A coroutine that checks the walking condition every 3 seconds
	public virtual IEnumerator decideIfWalk()
	{
		// Loop indefinitely
		while (true)
		{
			// Generate a random number between 0 and 1
			float random = Random.value;

			// If the random number is less than the walk chance, start walking
			if (random < startWalkingChance && !isWalking && !justWalked)
			{
				isWalking = true;
				anim.SetBool("isWalking", true);
				float randomDirection = Random.value;
				// Generate a random direction for the entity's movement
				if (randomDirection < 0.5) makeDirectionRight();
				else makeDirectionLeft();
				faceDirection();

				yield return new WaitForSeconds(Random.Range(walkingTime[0], walkingTime[1])); // walk for a random amount of time
				justWalked = true;
			}
			else
			{
				yield return new WaitForSeconds(3f); // Wait for 3 seconds before checking again
				justWalked = false;
			}
			stopWalking();
		}
	}

	protected void makeDirectionRight()
	{
		direction = new Vector2(1, 0).normalized;
	}

	protected void makeDirectionLeft()
	{
		direction = new Vector2(-1, 0).normalized;
	}

	//public abstract IEnumerator makeStepSound();
}