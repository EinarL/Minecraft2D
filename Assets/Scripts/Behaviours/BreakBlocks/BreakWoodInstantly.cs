using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakWoodInstantly : BreakBehaviour
{
	// Start is called before the first frame update
	public BreakWoodInstantly() : base("wood", 6, 1000f)
	{

	}

	public override AudioClip getDigSound() // no dig sound
	{
		return null;
	}
}
