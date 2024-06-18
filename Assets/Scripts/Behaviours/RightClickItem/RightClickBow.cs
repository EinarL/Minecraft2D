using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RightClickBow : RightClickItemBehaviour
{
	private Animator bowAnim;
	private Animator playerAnim;
	private SpriteRenderer bowAnimRenderer;
	private SpriteRenderer holdingItemRenderer;
	private Transform throwFromTransform;
	private MainThreadDispatcher mainThreadDispatcher;
	private Coroutine pointHandTowardsCursorCoroutine;

	private Transform playerTransform;
	private Transform frontArm;
	private Transform backArm;

	private float drawStartTime; // the time when the player started drawing back the bow
	private float maxDrawTime = 0.9f; // the time it takes to get maximum power on the arrow
	private float maxShootForce = 18f; // Maximum shoot force
	private float minShootForce = 10f; // Minimum shoot force
	private float minDamage = 1f;
	private float maxDamage = 7f;

	public RightClickBow()
	{
		playerTransform = GameObject.Find("SteveContainer").transform;
		frontArm = playerTransform.Find("Steve").Find("Arm Front Parent");
		backArm = playerTransform.Find("Steve").Find("Arm Back Parent");
		playerAnim = playerTransform.Find("Steve").GetComponent<Animator>();

		Transform holdingItem = playerTransform.transform.Find("Steve").Find("Arm Front Parent").Find("Arm Front").Find("HoldingItemPosition").Find("HoldingItem");
		if(holdingItem == null) holdingItem = playerTransform.transform.Find("Steve").Find("Arm Back Parent").Find("Arm Back").Find("HoldingItemPosition").Find("HoldingItem");
		throwFromTransform = holdingItem.transform;

		bowAnim = holdingItem.transform.Find("AnimatedItem").GetComponent<Animator>();
		bowAnimRenderer = bowAnim.GetComponent<SpriteRenderer>();
		holdingItemRenderer = holdingItem.GetComponent<SpriteRenderer>();
		mainThreadDispatcher = GameObject.Find("EventSystem").GetComponent<MainThreadDispatcher>();
	}

	public override void rightClickItem()
	{
		if (!InventoryScript.hasArrow()) return;
		drawStartTime = Time.time;

		IEnumerator pointHandTowardsCursor()
		{
			while (true)
			{
				Vector3 worldMousePos = getMousePosition();

				bool facingRight = worldMousePos.x > playerTransform.position.x;
				Transform arm = facingRight ? frontArm : backArm;

				// now rotate arm to point towards mouse position
				Vector3 diff = worldMousePos - arm.position;
				diff.Normalize();

				float zRotation = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

				if (!facingRight)
				{
					backArm.rotation = Quaternion.Euler(-180f, 0f, -zRotation + 90f);
					frontArm.rotation = Quaternion.Euler(-180f, 0f, -zRotation + 80f);
				}
				else
				{
					frontArm.rotation = Quaternion.Euler(0f, 0f, zRotation + 90f);
					backArm.rotation = Quaternion.Euler(0f, 0f, zRotation + 80f);
				}

				yield return null;
			}
		}

		pointHandTowardsCursorCoroutine = mainThreadDispatcher.startCoroutine(pointHandTowardsCursor());


		holdingItemRenderer.color = new Color(1, 1, 1, 0);
		playerAnim.SetBool("handsIdle", true);
		bowAnim.SetBool("isDrawingBackBow", true);
	}

	public override void stopHoldingRightClick(bool executeDefaultBehaviour = true) {

		Vector2 mousePos = getMousePosition();
		bool facingRight = mousePos.x > playerTransform.position.x;
		if (facingRight)
		{
			frontArm.rotation = Quaternion.Euler(0f, 0f, 0f);
			backArm.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else
		{
			frontArm.rotation = Quaternion.Euler(-180f, 0f, 180f);
			backArm.rotation = Quaternion.Euler(-180f, 0f, 180f);
		}


		bowAnim.SetBool("isDrawingBackBow", false);
		playerAnim.SetBool("handsIdle", false);
		bowAnimRenderer.sprite = null;
		holdingItemRenderer.color = new Color(1, 1, 1, 1);
		if (pointHandTowardsCursorCoroutine != null)
		{
			mainThreadDispatcher.stopCoroutine(pointHandTowardsCursorCoroutine);
			pointHandTowardsCursorCoroutine = null;
		}

		if (!executeDefaultBehaviour) return;
		if (!InventoryScript.hasArrow()) return;

		// shoot arrow
		AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Sounds\\Random\\bow"), throwFromTransform.position);
		InventoryScript.removeArrow();
		ToolInstance bow = InventoryScript.getHeldTool();
		if (bow != null) bow.reduceDurability();
		else Debug.LogError("The player is no ToolInstance connected to this bow");

		GameObject arrowPrefab = Resources.Load<GameObject>("Prefabs\\Throwables\\Arrow");
		Vector2 playerPos = new Vector2(throwFromTransform.position.x, throwFromTransform.position.y + 0.5f); // the position where the arrow will be shot from



		// Calculate the direction from the player to the mouse position
		Vector2 direction = mousePos - playerPos;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		float angleAdjustment = facingRight ? -40 : -30;
		Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle + angleAdjustment));

		// Instantiate the arrow at the player's position
		GameObject arrow = GameObject.Instantiate(arrowPrefab, playerPos, rotation); 

		Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();

		// calculate speed and damage for the arrow
		float drawTime = Time.time - drawStartTime;
		float shootForce;
		float damage;
		if (drawTime >= maxDrawTime)
		{
			shootForce = maxShootForce;
			damage = maxDamage;
		}
		else
		{
			float t = Mathf.InverseLerp(0f, maxDrawTime, drawTime); // gets a number between 0 and 1 where drawtime is between 0 and maxDrawTime
			shootForce = Mathf.Lerp(minShootForce, maxShootForce, t); // gets the shootForce based on where t is proportionally between the min and max force
			damage = Mathf.Lerp(minDamage, maxDamage, t);
		}
		rb.velocity = direction.normalized * shootForce;
		arrow.GetComponent<ArrowScript>().setDamage(damage);
		arrow.GetComponent<ArrowScript>().EnableCollider(playerTransform.GetComponent<Collider2D>());
	}



}
