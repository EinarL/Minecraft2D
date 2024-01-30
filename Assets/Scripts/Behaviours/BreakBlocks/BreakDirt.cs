using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BreakDirt : BreakBehaviour
{
	ShovelBlockSpeed breakSpeed = new ShovelBlockSpeed();
	// Start is called before the first frame update
	public BreakDirt() : base("dirt", 4, 1)
    {

	}

	public override float getBreakingSpeed(ToolInstance usingTool)
	{
		return breakSpeed.getBreakingSpeed(usingTool, breakingSpeed);
	}
}
