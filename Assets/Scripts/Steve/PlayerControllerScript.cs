using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerControllerScript : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform groundCheck;
    private Transform blockNextToPlayerLeft;
	private Transform blockNextToPlayerRight;
    private Transform steve; // character
    private Transform head;
    private Transform holdingItemObject; // the object that displays which item the player is holding
    private SpriteRenderer holdingItemObjectSpriteRenderer;
    private SpriteRenderer AnimatedItemSpriteRenderer;
	private Camera cam;
    private StepSoundScript stepSoundScript;
    private CapsuleCollider2D capCollider;
    private Vector2 defaultColliderSize;
    private Vector2 sleepingColliderSize;
    public Tilemap tilemap;

    private AudioClip splashSmall;
	private AudioClip splashBig;

	private float walkSpeed = 6;
	private float runSpeed = 10;
	private float jumpPower = 12f; // how high you jump
    private float jumpBoost;
	private float jumpBoostWhenWalking = 1.5f; // extra speed horizontally
	private float jumpBoostWhenRunning = 1.2f;
	private float speed; // this will change to runSpeed when runnning and walkSpeed when walking
    private bool isJumping = false;
    private bool isOnLadder = false;
    private LadderType ladderType = LadderType.Center; // what kind of ladder the player is next to
    private bool isInAirAfterJumping = false;
    private bool facingRight = true;
    private bool isSwimming = false;
	private float animationRunningSpeed = 1.5f;
    private string blockBelowPlayer; // the block name that the player is standing on

    private bool didFall = false; // used to know when the player hits the ground after falling
    private float fellFrom = float.NegativeInfinity; // how high the player fell from, this is a y value in the world space
    private HealthbarScript healthbarScript;
    private HungerbarScript hungerbarScript;
    private SleepScript sleepScript;
	private IDataService dataService = JsonDataService.Instance;

	private bool isRunning = false;

    private float horizontalMove = 0;
    // Start is called before the first frame update
    void Start()
    {
        steve = transform.Find("Steve");
        holdingItemObject = steve.Find("Arm Front Parent").Find("Arm Front").transform.Find("HoldingItemPosition").transform.Find("HoldingItem");
		holdingItemObjectSpriteRenderer = holdingItemObject.GetComponent<SpriteRenderer>();
		AnimatedItemSpriteRenderer = holdingItemObject.Find("AnimatedItem").GetComponent<SpriteRenderer>();
		anim = steve.GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
		stepSoundScript = steve.GetComponent<StepSoundScript>();
        groundCheck = transform.Find("GroundCheck");
        blockNextToPlayerLeft = transform.Find("BlockNextToPlayerLeft");
		blockNextToPlayerRight = transform.Find("BlockNextToPlayerRight");
		cam = Camera.main;
        speed = walkSpeed;
        jumpBoost = jumpBoostWhenWalking;
		capCollider = GetComponent<CapsuleCollider2D>();
        defaultColliderSize = capCollider.size;
        sleepingColliderSize = new Vector2(capCollider.size.x, 0.3f);
		head = steve.Find("Head"); // find player's head

        splashSmall = Resources.Load<AudioClip>("Sounds\\Liquid\\splash_small");
		splashBig = Resources.Load<AudioClip>("Sounds\\Liquid\\splash_big");

		healthbarScript = GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>();
		hungerbarScript = GameObject.Find("Canvas").transform.Find("Hungerbar").GetComponent<HungerbarScript>();
		sleepScript = GameObject.Find("Canvas").transform.Find("SleepTint").GetComponent<SleepScript>();

		if (dataService.exists("player-position.json")) // if there is a saved player position, then teleport the player to that position
		{
			float[] playerPosition = dataService.loadData<float[]>("player-position.json");

			transform.position = new Vector2(playerPosition[0], playerPosition[1]);
		}

        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation; // make the player not fall through blocks while the world is loading
        IEnumerator removeRBConstraints()
        {
            yield return new WaitForSeconds(0.5f);
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
        StartCoroutine(removeRBConstraints());
	}

	// Update is called once per frame
	void Update()
    {
		if (InventoryScript.getIsInUI() || anim.GetBool("isSleeping")) return; // if user is in the UI or if steve is sleeping, then we cant move

		horizontalMove = Input.GetAxisRaw("Horizontal"); // -1: left, 0: still, 1: right

        // jump
		if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || (Input.GetKey(KeyCode.W) && !isOnLadder)) && isGrounded() && !isJumping && !isInAirAfterJumping && rb.velocity.y <= 0 && !isSwimming)
        {
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x * jumpBoost, jumpPower);

		}
        if (isInAirAfterJumping && isGrounded()) 
        {
            isInAirAfterJumping = false;
        }
		if (isJumping)
        {
            if (hasBlockInPath())
            {
                isJumping = false;
            }
			isInAirAfterJumping = true;

			if (goingDown()) isJumping = false;

		}
        

        if(horizontalMove != 0 && !hasBlockInPath()) // moving left or right
		{
            anim.SetBool("isWalking", true);
            if(isGrounded()) stepSoundScript.playSound(blockBelowPlayer, isRunning); // make step sound
		}
        else
        {
			anim.SetBool("isWalking", false);
		}

        if (!isOnLadder && isLadderNextToPlayer()) // this runs once, when the player gets on a ladder
        {
            isOnLadder = true;
			rb.gravityScale = 0;
		}

        if (isOnLadder)
        {
			ladderLogic();

            if (!isLadderNextToPlayer())
            {
                isOnLadder = false;
				rb.gravityScale = 5;
                fellFrom = transform.position.y;
			}
		}

        if (!isGrounded() && !isOnLadder)
        {
            fellFrom = Mathf.Max(fellFrom, transform.position.y);
            didFall = true;
        }
        else if (didFall)
        {
            didFall = false;
            if(!isOnLadder && !isSwimming) checkIfTakeFallDamage();
        }

        if (!isSwimming)
        {
            if(isInWater())
            {
                toggleSwimmingPhysics();
            }
        }
        else if (!isInWater()) // if the player got out of the water
        {
            toggleSwimmingPhysics(false);
			fellFrom = transform.position.y;
		}
        else // if the player is swimming
        {
            swimmingControls();
        }

        checkIfRunning();
        lookTowardsMouse();
	}


    private void toggleSwimmingPhysics(bool on = true)
    {
        if (on) // swimming physics on
        {
			rb.gravityScale = 1;
            rb.drag = 5;

			walkSpeed = 3;
	        runSpeed = 5;

            // possibly play splash sound if player jumped in the water
            float fallDistance = fellFrom - transform.position.y;
            if (fallDistance > 2 && fallDistance < 7)
            {
				AudioSource.PlayClipAtPoint(splashSmall, transform.position);
			}
            else if(fallDistance >= 7)
            {
				AudioSource.PlayClipAtPoint(splashBig, transform.position);
			}
        }
        else
        {
            rb.gravityScale = 5;
            rb.drag = 0;

			walkSpeed = 6;
			runSpeed = 10;
		}
    }

    private void swimmingControls()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) rb.velocity = new Vector2(rb.velocity.x, 5);
		if (Input.GetKey(KeyCode.S)) rb.AddForce(new Vector2(0, -5));
	}

	// Move our character
	private void FixedUpdate()
	{
        if (InventoryScript.getIsInUI()) return; // if user is in the UI, then we cant move

		if (hasBlockInPath()) rb.velocity = new Vector2(0, rb.velocity.y);
		else if (isInAirAfterJumping && !hasBlockInPath()) rb.velocity = new Vector2(horizontalMove * speed * jumpBoost, rb.velocity.y);
		else if (isInAirAfterJumping) rb.velocity = new Vector2(0, rb.velocity.y);
		else rb.velocity = new Vector2(horizontalMove * speed, rb.velocity.y);
	}

    private void checkIfRunning()
    {
        if (!hungerbarScript.canRun()) // if player cant run due to hunger
        {
            isRunning = false;
			speed = walkSpeed;
			jumpBoost = jumpBoostWhenWalking;
			anim.SetFloat("movementSpeed", 1);
            return;
		}

        // if user presses left shift, then toggle running mode
        if (Input.GetKey(KeyCode.LeftShift)) isRunning = true;
        else isRunning = false;
        if (isRunning) // if the player is running we should check if we should stop running
        {
            if (horizontalMove == 0) isRunning = false; // if not moving then stop running

        }

        // set the speed to running speed if isRunning, otherwise walk speed
        if (isRunning)
        {
            speed = runSpeed;
            jumpBoost = jumpBoostWhenRunning;
			anim.SetFloat("movementSpeed", animationRunningSpeed);

		}
        else
        {
            speed = walkSpeed;
            jumpBoost = jumpBoostWhenWalking;
			anim.SetFloat("movementSpeed", 1);
		}
        
    }

    private bool isGrounded()
	{ 
        Collider2D[] results = new Collider2D[1];

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap");
        contactFilter.useLayerMask = true;

		int count = Physics2D.OverlapCircle(groundCheck.position, 0.05f, contactFilter, results);
        bool isGrounded = count > 0;

        if (isGrounded)
        {
            if (results[0].gameObject.layer == LayerMask.NameToLayer("Default")) // if its a gameObject (because GameObjects are on the default layer)
            {
                blockBelowPlayer = results[0].gameObject.name;
            }
            else // if its on the Tilemap layer
            {
				blockBelowPlayer = (tilemap.GetTile(tilemap.WorldToCell(new Vector2(groundCheck.position.x, groundCheck.position.y - 0.5f))) as Tile)?.sprite.name;
			}
            
        }

        return isGrounded;
    }

    private bool isInWater()
    {
		Collider2D[] results = new Collider2D[1];

		ContactFilter2D contactFilter = new ContactFilter2D();
		contactFilter.layerMask = LayerMask.GetMask("Water");
		contactFilter.useLayerMask = true;

		int count = Physics2D.OverlapCircle(groundCheck.position, 0.01f, contactFilter, results);
	    isSwimming = count > 0;
        return isSwimming;
	}

	/**
     * checks if there is a block in the way of the player
     * this is used to fix a bug where the player doesnt jump if its next to a block and going into the direction of the block
     */

	private bool hasBlockInPath() // OverlapBox(Vector2 point, Vector2 size, float angle)
	{
        if(goingLeft()) return Physics2D.OverlapBox(new Vector2(blockNextToPlayerLeft.position.x, blockNextToPlayerLeft.position.y), new Vector2(0.1f,1.8f), 0, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
		if(goingRight())return Physics2D.OverlapBox(new Vector2(blockNextToPlayerRight.position.x, blockNextToPlayerRight.position.y), new Vector2(0.1f, 1.8f), 0, LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap"));
        return false;
	}

    private void jump(float xVelocity)
    {
        rb.velocity = new Vector2(xVelocity, jumpPower);

	}

    private void lookTowardsMouse()
    {

		Vector3 mousePos = Input.mousePosition;
        Vector3 worldMousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        if (worldMousePos.x < transform.position.x && facingRight) // rotate character left
        {
            rotatePlayer(false);
		}
        else if (worldMousePos.x > transform.position.x && !facingRight) // rotate character right
        {
            rotatePlayer(true);
		}

		// now rotate head to look towards mouse position
		Vector3 diff = worldMousePos - head.position;
		diff.Normalize();

		float zRotation = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
		
        if(!facingRight) head.rotation = Quaternion.Euler(-180f, 0f, -zRotation);
        else head.rotation = Quaternion.Euler(0f, 0f, zRotation);
	}

    public void rotatePlayer(bool rotateRight)
    {
		Vector3 rotationVector = transform.rotation.eulerAngles;
		rotationVector.y = rotateRight ? 0f : 180f;
		gameObject.transform.rotation = Quaternion.Euler(rotationVector);
		facingRight = rotateRight;
		Transform temp = blockNextToPlayerLeft;
		blockNextToPlayerLeft = blockNextToPlayerRight;
		blockNextToPlayerRight = temp;
		putHoldingItemToOtherHand(rotateRight);
	}

    /**
     * make the item the player is holding be a child of the front Arm or back Arm
     */
    private void putHoldingItemToOtherHand(bool frontHand)
    {
        Transform newArm;
        if (frontHand) newArm = steve.Find("Arm Front Parent").Find("Arm Front");
        else newArm = steve.Find("Arm Back Parent").Find("Arm Back");

        Transform newParent = newArm.Find("HoldingItemPosition");

        holdingItemObject.rotation = newArm.rotation;
        holdingItemObject.position = newParent.position;
        holdingItemObject.localScale = new Vector3(0.0622607f, 0.0622607f, 0.0622607f);

        holdingItemObject.parent = newParent;

        if (frontHand)
        {
            holdingItemObjectSpriteRenderer.sortingOrder = 12;
            AnimatedItemSpriteRenderer.sortingOrder = 12;
        }
        else
        {
            holdingItemObjectSpriteRenderer.sortingOrder = 10;
			AnimatedItemSpriteRenderer.sortingOrder = 10;
		}

		anim.SetBool("isFacingRight", frontHand);
	}

    private void checkIfTakeFallDamage()
    {
        int fallHeight = Mathf.RoundToInt(fellFrom - transform.position.y);
        fallHeight -= 3; // only take damage if fell from 4 blocks or more
        if (fallHeight > 0) // if we need to reduce health
        {
            healthbarScript.takeDamage(fallHeight, false);
        }
        fellFrom = float.NegativeInfinity;
    }

    private void ladderLogic()
    {
        if (ladderType == LadderType.Right && Input.GetKey(KeyCode.D)) rb.velocity = new Vector2(rb.velocity.x, 5);
        else if (ladderType == LadderType.Left && Input.GetKey(KeyCode.A)) rb.velocity = new Vector2(rb.velocity.x, 5);
		else if (Input.GetKey(KeyCode.W)) rb.velocity = new Vector2(rb.velocity.x, 5);
        else if (Input.GetKey(KeyCode.S)) rb.velocity = new Vector2(rb.velocity.x, -6);
		else rb.velocity = new Vector2(rb.velocity.x, -2.5f);

	}

	private bool isLadderNextToPlayer()
    {
        // first lets check if there is a ladder tile at the players position
        TileBase tile1 = tilemap.GetTile(tilemap.WorldToCell(new Vector2(groundCheck.position.x, groundCheck.position.y + 1)));
        TileBase tile2 = tilemap.GetTile(tilemap.WorldToCell(groundCheck.position));
        // if the tile at groundChecks position or the tile above groundChecks position is a ladder, then return true
        bool tile1IsLadder = tile1 != null && tile1.name.StartsWith("Ladder");
        bool tile2IsLadder = tile2 != null && tile2.name.StartsWith("Ladder");

		if (tile1IsLadder || tile2IsLadder)
        {
            if (tile1IsLadder) ladderType = tile1.name.EndsWith("Right") ? LadderType.Right : (tile1.name.EndsWith("Left") ? LadderType.Left : LadderType.Center);
            else if (tile2IsLadder) ladderType = tile2.name.EndsWith("Right") ? LadderType.Right : (tile2.name.EndsWith("Left") ? LadderType.Left : LadderType.Center);
			return true;
        }

		// now we will check if there is a ladder gameobject at the players position
		// Create a list to store the results
		List<Collider2D> blocks = new List<Collider2D>();

		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("FrontBackground") | LayerMask.GetMask("BackBackground")); // only blocks on layer "Default" or "FrontBackground" or "BackBackground"

		// Check for overlaps
		Physics2D.OverlapCircle(groundCheck.position, 0.30f, filter, blocks);

        foreach (Collider2D blockCollider in blocks)
        {
            if (blockCollider.gameObject.name.StartsWith("Ladder"))
            {
				ladderType = blockCollider.gameObject.name.EndsWith("Right") ? LadderType.Right : (blockCollider.gameObject.name.EndsWith("Left") ? LadderType.Left : LadderType.Center);
				return true;
            }
        }

		return false;
    }

    public void teleportToSpawn(Vector2 pos)
    {
        gameObject.transform.position = pos;
    }

	public void die()
    {
        anim.SetBool("isDead", true);
    }

    public void goToSleep()
    {
		if (anim.GetBool("isSleeping")) return;
		anim.SetBool("isSleeping", true);
		capCollider.size = sleepingColliderSize;

		if (!facingRight) head.rotation = Quaternion.Euler(-180f, 0f, -180f);
		else head.rotation = Quaternion.Euler(0f, 0f, 0f);

        sleepScript.sleep(); // create the dark tint & which will later make the player wake up
	}
    public void stopSleeping(bool wokeUp = false)
    {
        if (!anim.GetBool("isSleeping")) return;
		anim.SetBool("isSleeping", false);
		capCollider.size = defaultColliderSize;
        transform.position = new Vector2(transform.position.x, transform.position.y + 1f);

        if(!wokeUp) sleepScript.stopSleeping();
	}

    public bool isSleeping()
    {
        return anim.GetBool("isSleeping");

	}

	public void removeDeathAnimation()
	{
		anim.SetBool("isDead", false);
	}

	private bool goingRight()
    {
        return horizontalMove > 0;
    }

    private bool goingLeft()
    {
        return horizontalMove < 0;
    }

    private bool goingDown()
    {
        return rb.velocity.y < 0;
    }

    public bool getIsRunning()
    {
        return isRunning;
    }


    public enum LadderType
    {
        Right,
        Left,
        Center
    }
}
