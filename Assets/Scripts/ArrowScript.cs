using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
	Rigidbody2D rb;
	bool collided = false;
	private float damage = 7f;
	private Collider2D arrowCollider;
	private Collider2D playerCollider;

	// Start is called before the first frame update
	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		arrowCollider = transform.Find("Collider").GetComponent<Collider2D>();

		playerCollider = GameObject.Find("SteveContainer").GetComponent<Collider2D>();
		StartCoroutine(IgnorePlayerCollision());
	}

	// Update is called once per frame
	void Update()
	{
		if(!collided) UpdateRotation();
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
		
        if(collision.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            Entity entityScript = collision.gameObject.GetComponent<Entity>();

            if (entityScript != null)
            {
                entityScript.takeDamage(damage, transform.position.x);
            }
            else Debug.LogError("Entity script was not found on the entity that the arrow collided with");
        }
		else if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>().takeDamage(Mathf.RoundToInt(damage));
		}
        IEnumerator destroyArrow()
		{
			yield return new WaitForSeconds(60f);
			Destroy(gameObject);
		}
		StartCoroutine(destroyArrow());
	}

	private IEnumerator IgnorePlayerCollision()
	{
		Physics2D.IgnoreCollision(arrowCollider, playerCollider, true);
		yield return new WaitForSeconds(0.5f);
		Physics2D.IgnoreCollision(arrowCollider, playerCollider, false);
	}
}
