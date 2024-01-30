using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using System.Runtime.Serialization;


[CreateAssetMenu(menuName = "Tool")]
public class Tool : ScriptableObject
{

	public ToolType toolType;

	public ToolMaterial toolMaterial;

	public float damage = 2; // how much damage the tool does
	// breakSpeed variable is only relevant for shovels, axes, and pickaxes
	// breakSpeed variable defines how fast the tool is at breaking a block
	public float breakSpeed = 1;
	public int durability = 100; 



	/*
	private void setupTool() // maybe set durability also here
	{
		switch(toolMaterial)
		{
			case ToolMaterial.Wood:
				breakSpeed = 1.4f;
				setDurability(5);
				break;
			case ToolMaterial.Stone:
				breakSpeed = 1.7f;
				setDurability(131);
				break;
			case ToolMaterial.Gold:
				breakSpeed = 3f;
				setDurability(50);
				break;
			case ToolMaterial.Iron:
				breakSpeed = 2;
				setDurability(250);
				break;
			case ToolMaterial.Diamond:
				breakSpeed = 2.3f;
				setDurability(1025);
				break;
		}

	}
	*/

}
