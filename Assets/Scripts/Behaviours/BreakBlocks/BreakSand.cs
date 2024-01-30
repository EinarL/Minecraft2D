using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BreakSand : BreakBehaviour
{
	private ShovelBlockSpeed breakSpeed = new ShovelBlockSpeed();
	// Start is called before the first frame update
	public BreakSand() : base("sand", 5, 1)
    {

	}

	public override float getBreakingSpeed(ToolInstance usingTool)
	{
		return breakSpeed.getBreakingSpeed(usingTool, breakingSpeed);
	}
}
