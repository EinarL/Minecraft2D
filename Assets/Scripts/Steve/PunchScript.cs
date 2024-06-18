using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchScript : MonoBehaviour
{
    private GameObject steve;
    private Animator anim;
    private Transform head;
    private Camera cam;
    private BreakBlockScript breakScript;
    private float hitDistance = 4; // how far can you hit enemies

    // Start is called before the first frame update
    void Start()
    {
        steve = transform.Find("Steve").gameObject;
        anim = steve.GetComponent<Animator>();
        head = steve.transform.Find("Head");
        cam = Camera.main;
        breakScript = steve.GetComponent<BreakBlockScript>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0) && !InventoryScript.getIsInUI()) // if left click && not in UI
		{
			punch();
		}
	}

    private void punch()
    {
		Vector3 mousePos = Input.mousePosition;
		Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        Vector2 direction = mouseWorldPos - head.position;

		RaycastHit2D hit = Physics2D.Raycast(head.position, direction, hitDistance, LayerMask.GetMask("Entity") | LayerMask.GetMask("Default"));
        if (hit && hit.collider.gameObject.layer == 10) // if hit entity
        {
            Entity entityScript = hit.collider.gameObject.GetComponent<Entity>();
            if (entityScript != null)
            {
                ToolInstance tool = InventoryScript.getHeldTool();
                if (tool == null) entityScript.takeDamage(1, transform.position.x);
                else entityScript.takeDamage(tool.damage, transform.position.x);
            }
		}

        // do punch animation
        if (!breakScript.isHoveringOverBlock()) // if not hovering over a block
        {
			bool facingRight = anim.GetBool("isFacingRight");

			if (facingRight) anim.Play("fightFrontArm");
            else anim.Play("fightBackArm");
		}
        
	}
}
