using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DroppedItemScript : MonoBehaviour
{

	private bool pickupable = true;
	private float pickupableTimer = 0f;
	private float pickupableAfter = 1.5f; // can again pickup the item after x many seconds
	private float dropVelocity = 5f; // the velocity when player drops this item out of his inventory

	private Rigidbody2D rb;
	private Camera cam;

	public ToolInstance tool; // this is null if its not a tool
	//public Image image;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		cam = Camera.main;
	}

	// Update is called once per frame
	void Update()
	{
		if (!pickupable)
		{
			checkIfChangePickupable();
		}
	}

	/**
     * checks if we need to change pickupable to true so the player can pickup the item.
     */
	private void checkIfChangePickupable()
	{
		pickupableTimer += Time.deltaTime;
		if (pickupableTimer > pickupableAfter)
		{
			pickupable = true;
			pickupableTimer = 0f;
		}
	}

	public void addDropVelocity(Vector2 stevePosition)
	{
		if (cam == null) Start();

		Vector3 mousePos = Input.mousePosition;
		Vector2 worldMousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

		Vector2 dropDirection = (worldMousePos - stevePosition).normalized;

		rb.velocity += dropDirection * dropVelocity;

	}

	public void setPickupable(bool value)
	{
		pickupable = value;
	}

	public bool isPickupable()
	{
		return pickupable;
	}
}
