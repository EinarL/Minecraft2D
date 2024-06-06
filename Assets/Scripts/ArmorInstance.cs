using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

public class ArmorInstance : DurabilityItem
{
	public int armorPoints;
	public int durability;
	public int STARTING_DURABILITY;

	public string armorType;
	public string armorMaterial;

	public ArmorInstance() { }

	public ArmorInstance(Armor armor)
	{
		armorType = armor.armorType.ToString();
		armorMaterial = armor.armorMaterial.ToString();
		armorPoints = armor.armorPoints;
		durability = armor.durability;
		STARTING_DURABILITY = armor.durability;
	}

	public int getDurability()
	{
		return durability;
	}

	public ArmorType getArmorType()
	{
		return stringToEnum<ArmorType>(armorType);
	}

	public ArmorMaterial getArmorMaterial()
	{
		return stringToEnum<ArmorMaterial>(armorMaterial); ;
	}

	private T stringToEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	public int getStartingDurability()
	{
		return STARTING_DURABILITY;
	}

	/**
	 * runs when the player takes damage
	 */
	public void reduceDurability()
	{
		durability--;
	}

}


public enum ArmorType
{
	[EnumMember(Value = "Helmet")]
	Helmet,
	[EnumMember(Value = "Chestplate")]
	Chestplate,
	[EnumMember(Value = "Leggings")]
	Leggings,
	[EnumMember(Value = "Boots")]
	Boots
}

public enum ArmorMaterial
{
	[EnumMember(Value = "Leather")]
	Leather,
	[EnumMember(Value = "Iron")]
	Iron,
	[EnumMember(Value = "Gold")]
	Gold,
	[EnumMember(Value = "Diamond")]
	Diamond
}