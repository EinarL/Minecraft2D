using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
	Rigidbody2D rb;
	bool collided = false;
	private float damage = 5f;
	private GameObject arrowCollider;
	private Transform playerTransform;
	private bool pickupable = false;

	private void Awake()
	{
		arrowCollider = transform.Find("Collider").gameObject;
		arrowCollider.SetActive(false);
	}

	// Start is called before the first frame update
	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		playerTransform = GameObject.Find("SteveContainer").transform.Find("Steve").Find("Torso").transform;
	}

	// Update is called once per frame
	void Update()
	{
		if(!collided) UpdateRotation();

		if (transform.position.y < -100) Destroy(gameObject);
		if (pickupable)
		{
			float distance = Vector2.Distance(transform.position, playerTransform.position);
			if (distance <= 1.5f && InventoryScript.hasSpaceFor("Arrow"))
			{
				GameObject itemContainer = Resources.Load<GameObject>("Prefabs\\ItemContainer"); // get itemContainer
				GameObject item = itemContainer.transform.Find("Item").gameObject; // get item within itemContainer

				Sprite itemImage = Resources.Load<Sprite>("Textures\\ItemTextures\\Arrow"); // get the image for the item
				item.GetComponent<SpriteRenderer>().sprite = itemImage; // put the image on the SpriteRenderer

				GameObject.Find("SteveContainer").transform.Find("Steve").GetComponent<PlayerInventory>().pickupItem(itemContainer, false);
				Destroy(gameObject);
			}
		}
	}

	void UpdateRotation()
	{
		// Get the current velocity of the arrow
		Vector2 velocity = rb.velocity;

		// Calculate the angle in degrees based on the velocity
		float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

		// Apply the angle to the arrow's rotation
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 40));
	}

	public void setDamage(float damage)
	{
		this.damage = damage;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collided || collision.gameObject.layer == LayerMask.NameToLayer("Throwable")) return;
		collided = true;

		// Get the arrow's current rotation angle
		float angle = transform.eulerAngles.z + 45f;

		// Calculate the direction vector from the rotation angle
		Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

		// Move the arrow 0.2 units further in the direction it is pointing
		transform.position += (Vector3)(direction * 0.4f);

		transform.parent = collision.transform;
		if (collision.gameObject.layer == LayerMask.NameToLayer("Entity"))
		{
			Entity entityScript = collision.gameObject.GetComponent<Entity>();

			if (entityScript != null)
			{
				entityScript.takeDamage(damage, transform.position.x);
			}
			else Debug.LogError("Entity script was not found on the entity that the arrow collided with");
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>().takeDamage(Mathf.RoundToInt(damage));
			Transform playerHead = collision.transform.Find("Steve").Find("Head").transform;
			if (transform.position.y >= playerHead.position.y) transform.parent = playerHead.transform;
		}
		else StartCoroutine(setPickupable());

		Destroy(rb);
		Destroy(arrowCollider);
        IEnumerator destroyArrow()
		{
			yield return new WaitForSeconds(60f);
			Destroy(gameObject);
		}
		StartCoroutine(destroyArrow());
	}

	public void EnableCollider(Collider2D collider)
	{
		arrowCollider.SetActive(true); // enable all collisions
		Physics2D.IgnoreCollision(arrowCollider.GetComponent<Collider2D>(), collider, true); // disable collision with "collider"
		StartCoroutine(removeColliderIgnore(collider));
	}

	private IEnumerator removeColliderIgnore(Collider2D collider)
	{
		yield return new WaitForSeconds(0.5f);
		if (arrowCollider != null) Physics2D.IgnoreCollision(arrowCollider.GetComponent<Collider2D>(), collider, false); // enable collision with "collider"
	}

	private IEnumerator setPickupable()
	{
		yield return new WaitForSeconds(0.5f);
		pickupable = true;
	}
}
