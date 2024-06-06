using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

public class ToolInstance : DurabilityItem
{


	public float damage = 2; // how much damage the tool does
	// breakSpeed variable is only relevant for shovels, axes, and pickaxes
	// breakSpeed variable defines how fast the tool is at breaking a block
	public float breakSpeed = 1;
	public int durability;
	public int STARTING_DURABILITY;

	public string toolType;
	public string toolMaterial;

	public ToolInstance() { }

	public ToolInstance(Tool tool)
	{
		toolType = tool.toolType.ToString();
		toolMaterial = tool.toolMaterial.ToString();
		damage = tool.damage;
		breakSpeed = tool.breakSpeed;
		durability = tool.durability;
		STARTING_DURABILITY = tool.durability;
	}

	public int getDurability()
	{
		return durability;
	}

	public ToolType getToolType()
	{
		return stringToEnum<ToolType>(toolType);
	}

	public ToolMaterial getToolMaterial()
	{
		return stringToEnum<ToolMaterial>(toolMaterial); ;
	}

	private T stringToEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	public float getDamage()
	{
		return damage;
	}

	public float getBreakSpeed()
	{
		return breakSpeed;
	}

	public int getStartingDurability()
	{
		return STARTING_DURABILITY;
	}

	/**
	 * runs when the player uses this tool to break a block/ fight entity
	 * reduces the durability of the tool
	 */
	public void reduceDurability()
	{
		durability--;

		if (durability <= 0) InventoryScript.breakToolBeingHeld();
		else InventoryScript.updateHeldToolDurability(this); 
	}

}


public enum ToolType
{
	[EnumMember(Value = "Sword")]
	Sword,
	[EnumMember(Value = "Shovel")]
	Shovel,
	[EnumMember(Value = "Pickaxe")]
	Pickaxe,
	[EnumMember(Value = "Axe")]
	Axe
}

public enum ToolMaterial
{
	[EnumMember(Value = "Wood")]
	Wood,
	[EnumMember(Value = "Stone")]
	Stone,
	[EnumMember(Value = "Gold")]
	Gold,
	[EnumMember(Value = "Iron")]
	Iron,
	[EnumMember(Value = "DIamond")]
	Diamond
}