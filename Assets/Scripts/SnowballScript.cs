using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (transform.position.y < -100) Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
        if (collision.gameObject.layer == LayerMask.NameToLayer("Throwable")) return;

        if(collision.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            Entity entityScript = collision.gameObject.GetComponent<Entity>();

            if (entityScript != null)
            {
                entityScript.takeDamage(1, transform.position.x);
            }
            else Debug.LogError("Entity script was not found on the entity that the snowball collided with");
        }
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			GameObject.Find("Canvas").transform.Find("Healthbar").GetComponent<HealthbarScript>().takeDamage(1);
		}
		// TODO: make particle system for the snowball
		Destroy(gameObject);
	}
}
