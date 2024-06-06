using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using System.Runtime.Serialization;


[CreateAssetMenu(menuName = "Armor")]
public class Armor : ScriptableObject
{

	public ArmorType armorType;

	public ArmorMaterial armorMaterial;

	public int armorPoints;
	public int durability;

}
