using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelBlockSpeed
{

	public float getBreakingSpeed(ToolInstance usingTool, float breakingSpeed)
	{
		if (usingTool == null) return breakingSpeed;

		// if its a shovel, then break the "shovel type" block faster
		if (usingTool.getToolType().Equals(ToolType.Shovel)) return breakingSpeed * usingTool.getBreakSpeed();
		return breakingSpeed;
	}
}
