using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlaceBlockBehaviour
{
	List<GameObject> placeBlock(GameObject blockToPlace, PlaceBlockScript pbScript, BreakBlockScript bbScript);
}
