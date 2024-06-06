using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface DurabilityItem
{
	public int getStartingDurability();

	public void reduceDurability();

	public int getDurability();
}
