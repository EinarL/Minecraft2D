using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class RightClickItemBehaviour
{
	public abstract void rightClickItem();

	public abstract void stopHoldingRightClick(bool executeDefaultBehaviour = true);

	protected Vector2 getMousePosition()
	{
		Vector3 mousePos = Input.mousePosition;
		Camera cam = Camera.main;
		return cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
	}
}
