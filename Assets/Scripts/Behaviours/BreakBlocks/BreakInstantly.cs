using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakInstantly : BreakBehaviour
{
	// Start is called before the first frame update
	public BreakInstantly() : base("grass", 6, 1000f)
	{

	}

	public override AudioClip getDigSound() // no dig sound
	{
		return null;
	}
}
