using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BreakWood : BreakBehaviour
{

	// Start is called before the first frame update
	public BreakWood() : base("wood", 6, 0.6f)
    {

	}

	public override float getBreakingSpeed(ToolInstance usingTool)
	{
		if (usingTool == null) return breakingSpeed;

		// if its an axe, then break the "wood type" block faster
		if (usingTool.getToolType().Equals(ToolType.Axe)) return breakingSpeed * usingTool.getBreakSpeed();
		return breakingSpeed;
	}
}
