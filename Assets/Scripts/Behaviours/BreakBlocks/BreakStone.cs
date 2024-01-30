using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BreakStone : BreakBehaviour
{

	// Start is called before the first frame update
	public BreakStone() : base("stone", 6, 0.2f)
    {

	}

	public override float getBreakingSpeed(ToolInstance usingTool)
	{
		if (usingTool == null) return breakingSpeed;

		// if its an axe, then break the "stone type" block faster
		if (usingTool.getToolType().Equals(ToolType.Pickaxe)) return breakingSpeed * usingTool.getBreakSpeed();
		return breakingSpeed;
	}
}
