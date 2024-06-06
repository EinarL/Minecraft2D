using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// interface for InventorySlotScript and CraftingSlotScript and ArmorSlotScript
public interface Slot
{

	void leftClickSlot();

	void OnPointerExit(PointerEventData eventData);
}
