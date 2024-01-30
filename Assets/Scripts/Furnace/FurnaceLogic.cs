using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnaceLogic
{
	private Vector2 worldPosition;
	private InventorySlot furnaceTopSlot = new InventorySlot();
	private InventorySlot furnaceBottomSlot = new InventorySlot();
	private InventorySlot furnaceResultSlot = new InventorySlot();
	private bool isBurnableInTopSlot = false; // is the item in the top slot burnable
	private bool isBurnableInBottomSlot = false; // can the item in the bottom slot be used for fuel
	private string resultItem = "";
	private float burnTime = 0f;
	private bool isCookCoroutineRunning = false;
	private bool isArrowCoroutineRunning = false;

	private float fireProgress = 0; // this is 1 to display a full fire image, goes down to 0 when the furnace is burning
	private float arrowProgress = 0; // the arrow image is 24 pixels wide, so this should be some x/24 where 0 <= x <= 24

	private OpenFurnaceScript openFurnaceScript = GameObject.Find("Canvas").transform.Find("InventoryParent").GetComponent<OpenFurnaceScript>();

	public FurnaceLogic(Vector2 worldPosition)
	{
		this.worldPosition = worldPosition;
	}

	public FurnaceLogic(JArray furnaceData) // float xPos, float yPos, InventorySlot topSlot, InventorySlot bottomSlot, InventorySlot resultSlot, float fireProgress, float arrowProgress, float burnTime
	{
		worldPosition = new Vector2((float)furnaceData[0], (float)furnaceData[1]);
		furnaceTopSlot = furnaceData[2].ToObject<InventorySlot>();
		furnaceBottomSlot = furnaceData[3].ToObject<InventorySlot>();
		furnaceResultSlot = furnaceData[4].ToObject<InventorySlot>();
		fireProgress = (float)furnaceData[5];
		arrowProgress = (float)furnaceData[6];

		burnTime = (float)furnaceData[7];
		isBurnableInBottomSlot = burnTime >= 0;

		resultItem = FurnaceHashtable.getBurnedItem(furnaceTopSlot.itemName);
		isBurnableInTopSlot = resultItem != "";
	}


	// return true if the furnace should start cooking
	public bool putItemInBottomSlot(InventorySlot newItem)
	{
		furnaceBottomSlot = newItem;
		if (isCooking()) return false; // already cooking, dont need to start cooking again
		burnTime = FurnaceHashtable.getBurnTime(furnaceBottomSlot.itemName);
		Debug.Log("burn time: " + burnTime);
		if (burnTime != -1) // if the item can burn
		{
			isBurnableInBottomSlot = true;
			
			return checkIfStartToCook();
		}
		else
		{
			isBurnableInBottomSlot = false;
		}

		return false;
	}
	// return true if the furnace should start cooking
	public bool putItemInTopSlot(InventorySlot newItem)
	{
		furnaceTopSlot = newItem;
		resultItem = FurnaceHashtable.getBurnedItem(furnaceTopSlot.itemName); // the name of the item that will be the result of newItem burning
		if (resultItem != "") // if the item can burn
		{
			isBurnableInTopSlot = true;
			if (isCooking()) return true;
			return checkIfStartToCook();
		}
		else
		{
			isBurnableInTopSlot = false;
		}
		
		return false;
	}

	public bool putItemInResultSlot(InventorySlot newItem)
	{
		furnaceResultSlot = newItem;
		if (isCooking()) return true;
		return checkIfStartToCook();
	}

	private bool checkIfStartToCook()
	{
		if (!isBurnableInTopSlot || !isBurnableInBottomSlot) return false; // if we cannot start cooking
		if (!isResultSlotAvailable()) return false;
		fireProgress = 1f;
		furnaceBottomSlot.removeFromSlot(1);
		if (furnaceBottomSlot.amount <= 0) isBurnableInBottomSlot = false;
		openFurnaceScript.updateFurnaceSlotsVisually(this, furnaceBottomSlot, furnaceTopSlot, furnaceResultSlot);
		openFurnaceScript.updateFireAnimation(this, fireProgress);
		return true;
	}

	// coroutine that diminishes the fire animation
	public IEnumerator cookCoroutine()
	{
		if (isCookCoroutineRunning) yield break;
		while (isCooking())
		{
			Debug.Log("FIRE PROGRESS: " + fireProgress);
			isCookCoroutineRunning = true;
			yield return new WaitForSeconds(burnTime);
			fireProgress -= .1f;
			openFurnaceScript.updateFireAnimation(this, fireProgress);
			if (fireProgress <= 0f)
			{
				if (furnaceBottomSlot.amount > 0 && isBurnableInTopSlot && isResultSlotAvailable()) // if more fuel in slot
				{
					fireProgress = 1f; // continue burning
					// remove one item from bottom slot
					furnaceBottomSlot.removeFromSlot(1);
					openFurnaceScript.updateFurnaceSlotsVisually(this, furnaceBottomSlot, furnaceTopSlot, furnaceResultSlot);
					openFurnaceScript.updateFireAnimation(this, fireProgress);
				}
				else // stop burning
				{
					isBurnableInBottomSlot = false;
					fireProgress = 0f;
					openFurnaceScript.updateFireAnimation(this, fireProgress);
				}
			}
		}
		isCookCoroutineRunning = false;
	}

	// coroutine that expands the white arrow
	public IEnumerator arrowCoroutine()
	{
		if(isArrowCoroutineRunning) yield break;
		while(isCooking() && !resultItem.Equals("") && FurnaceHashtable.getBurnedItem(furnaceTopSlot.itemName) != "")
		{
			if (!isResultSlotAvailable()) break;
			isArrowCoroutineRunning = true;
			yield return new WaitForSeconds(.4f); // takes 9.6 sec for arrow animations to finish // 76.8 sec for 8 items
			arrowProgress += 1f / 24f;
			openFurnaceScript.updateArrowAnimation(this, arrowProgress);

			if (arrowProgress >= 1)
			{
				furnaceTopSlot.removeFromSlot(1);
				addItemToResultSlot();
				openFurnaceScript.updateFurnaceSlotsVisually(this, furnaceBottomSlot, furnaceTopSlot, furnaceResultSlot);
				if (furnaceTopSlot.amount <= 0)
				{
					isBurnableInTopSlot = false;
					break;
				}
				arrowProgress = 0f;
				openFurnaceScript.updateArrowAnimation(this, arrowProgress);
			}
		}
		arrowProgress = 0f;
		openFurnaceScript.updateArrowAnimation(this, arrowProgress);
		isArrowCoroutineRunning = false;
	}

	private void addItemToResultSlot()
	{
		// if result slot is not empty && the item in the slot is not the same as the item about to be added to the slot
		if (!furnaceResultSlot.isEmpty() && !furnaceResultSlot.itemName.Equals(resultItem)) return;
		furnaceResultSlot.putItemInSlot(resultItem);
	}

	private bool isResultSlotAvailable()
	{
		if (furnaceResultSlot.isEmpty()) return true;
		if (furnaceResultSlot.itemName.Equals(resultItem) && furnaceResultSlot.amount < 64) return true;
		return false;
	}

	// returns true if the fire animation is on
	private bool isCooking()
	{
		return fireProgress > 0f;
	}

	public float getFireProgress()
	{
		return fireProgress;
	}

	public float getArrowProgress()
	{
		return arrowProgress;
	}

	public InventorySlot getFurnaceBottomSlot()
	{
		return furnaceBottomSlot;
	}

	public InventorySlot getFurnaceTopSlot()
	{
		return furnaceTopSlot;
	}

	public InventorySlot getFurnaceResultSlot()
	{
		return furnaceResultSlot;
	}

	public bool getIsBurnableInTopSlot()
	{
		return isBurnableInTopSlot;
	}

	public Vector2 getFurnacePosition()
	{
		return worldPosition;
	}
	/**
	 * returns data that is needed to save the furnace
	 */
	public object[] getFurnaceData()
	{
		return new object[] { worldPosition.x, worldPosition.y, furnaceTopSlot, furnaceBottomSlot, furnaceResultSlot, fireProgress, arrowProgress, burnTime};
	}

}
