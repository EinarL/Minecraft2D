using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// interface for InventorySlotScript and CraftingSlotScript
public interface Slot
{

	void leftClickSlot();

	void OnPointerExit(PointerEventData eventData);
}
