using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BreakSnow : BreakBehaviour
{
	ShovelBlockSpeed breakSpeed = new ShovelBlockSpeed();
	// Start is called before the first frame update
	public BreakSnow() : base("cloth", 4, 1.2f)
    {

	}

	public override float getBreakingSpeed(ToolInstance usingTool)
	{
		return breakSpeed.getBreakingSpeed(usingTool, breakingSpeed);
	}
}
