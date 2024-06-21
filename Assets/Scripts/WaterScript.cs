using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class WaterScript : MonoBehaviour
{

    public int waterState = 0; // goes from 0 to 7 (inclusive), where 0 is full water (still water) and 7 is very low water
    public bool isFlowing { get; private set; } = false; // is the water stillwater or is it flowing
    private Animator anim;
    private spawnChunkScript scScript;
    private int checkedWaterState = 0; // the state on the water block that we previously found by calling getBlockAtPosition()
    private float flowSpeed = 0.4f;

	void Awake()
	{
		anim = transform.Find("Image").GetComponent<Animator>();
        scScript = GameObject.Find("Main Camera").GetComponent<spawnChunkScript>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    public void setFlow(bool flowing)
    {
        isFlowing = flowing;
        anim.SetBool("isFlowing", flowing);
    }

    private void changeWaterState(int newState)
    {
        Assert.IsTrue(newState >= 0 && newState <= 8);
        if(newState == 8)
        {
            Destroy(gameObject);
            return;
        }
        GameObject newStatePrefab = Resources.Load<GameObject>($"Prefabs\\Blocks\\Water{newState}");
        if(newState == 0) newStatePrefab = Resources.Load<GameObject>("Prefabs\\Blocks\\Water");

        GameObject newStateInstance = Instantiate(newStatePrefab);

        GameObject image = newStateInstance.transform.Find("Image").gameObject;
        GameObject spriteMask = newStateInstance.transform.Find("Sprite Mask").gameObject;

        Destroy(transform.Find("Image").gameObject);
        Destroy(transform.Find("Sprite Mask").gameObject);

		// Set image and spriteMask as children of the current GameObject
		image.transform.SetParent(this.transform, false);
		spriteMask.transform.SetParent(this.transform, false);
		anim = transform.Find("Image").GetComponent<Animator>();

		waterState = newState;

		Destroy(newStateInstance);
	}

    public void startFlowing(int state = -1, bool flowing = false)
    {
        StartCoroutine(flow(state, flowing));
    }

    public void startDeflowing()
    {
        StartCoroutine(deflow());
    }

    /**
     * makes the water expand,
     * 
     * 
     * if there is no block below, then flow down with state 0 (and put isFlowing to true)
     * otherwise if waterType == 7, then return
     * 
     * otherwise expand left and right:
     *          if there is NOT a block to the right, then expand to the right with state = waterState + 1
     *          if there is NOT a block to the left, then expand to the left with state = waterState + 1
     * 
     */
    private IEnumerator flow(int state = -1, bool flowing = false) // flowing is true when we want a flow animation on the block
    {
		if (flowing) setFlow(flowing);
		yield return new WaitForSeconds(flowSpeed);

        if(state != -1) waterState = state;

        BlockType belowBlock = getBelowBlock();
        if (belowBlock == BlockType.None) {
            createWaterBlock(Direction.Down, 0); // no block below, then flow down
             yield break;
        }
        else if (belowBlock == BlockType.Water) {
            replaceWaterBlock(Direction.Down, 0); // water block below, then replace it with the flow of this water block
			yield break;
        }
        if (waterState == 7) yield break; // the final state cant flow further, so return

        // at this point we know that there is a block below us, so we can flow right and/or left (depending on what blocks are right and left)

        BlockType rightBlock = getRightBlock();
        if (rightBlock == BlockType.None)
        {
            if (SpawningChunkData.getRightMostChunkEdge() > transform.position.x + 1) createWaterBlock(Direction.Right, waterState + 1); 
            else scScript.rightChunkWaterToFlow.Add(this); // if water is flowing out of map
		}
        else if (rightBlock == BlockType.Water && waterState < checkedWaterState) replaceWaterBlock(Direction.Right, waterState + 1);

		BlockType leftBlock = getLeftBlock();
        if (leftBlock == BlockType.None)
        {
            if (SpawningChunkData.getLeftMostChunkEdge() < transform.position.x - 1) createWaterBlock(Direction.Left, waterState + 1);
			else scScript.leftChunkWaterToFlow.Add(this); // if water is flowing out of map
		}
        else if (leftBlock == BlockType.Water && waterState < checkedWaterState) replaceWaterBlock(Direction.Left, waterState + 1);
	}
    /**
     * makes the water diminish
     * 
     * if the water is at stage 0 && !isFlowing, then return
     * call .deflow() on all of the water blocks around this one that has waterStage <= this.waterStage,
     * if it doesnt have any waterblocks around it with less waterStage, then lower the waterstage and call deflow() on waterblocks
     * around this waterblock that have waterStage >= this.waterstage
     */
    private IEnumerator deflow()
    {
        yield return new WaitForSeconds(flowSpeed);
        if (waterState == 0 && !isFlowing) yield break;

        List<WaterScript> surroundingWater = getSurroundingWaterBlocks(); // this list is sorted, lowest waterState first

        bool deflowingOtherWater = false;
        foreach(WaterScript waterScript in surroundingWater)
        {
            if (waterScript == null) continue;
            if(waterScript.waterState < waterState || (waterState == 0 && waterScript.waterState == 0 && waterScript.transform.position.y > transform.position.y))
            {
				waterScript.startDeflowing();
                deflowingOtherWater = true;
			}
        }
        if(!deflowingOtherWater)
        {
            bool dontDeflow = false;
			foreach (WaterScript waterScript in surroundingWater)
			{
                if (waterScript == null) continue;
				if (waterScript.waterState >= waterState && waterScript.transform.position.y <= transform.position.y && waterScript.waterState != 0)
				{
					waterScript.startDeflowing();
                    deflowingOtherWater = true;
				}
                else if(waterScript.transform.position.y > transform.position.y && waterScript.waterState >= waterState) dontDeflow = true;
			}
            if(deflowingOtherWater) changeWaterState(waterState + 1);
            else if(!dontDeflow)
            {
                StartCoroutine(deflowEntireBlock(surroundingWater));
            }
		}

	}

    private IEnumerator deflowEntireBlock(List<WaterScript> waterToDeflowAfter)
    {
        while(waterState < 7)
        {
            changeWaterState(waterState + 1);
			yield return new WaitForSeconds(flowSpeed);
		}
        foreach(WaterScript waterScript in waterToDeflowAfter)
        {
            if(waterScript == null) continue;
            if(waterScript.transform.position.y < transform.position.y)
            {
                waterScript.startDeflowing();
            }
        }
        Destroy(gameObject);
    }

	private void createWaterBlock(Direction direction, int state)
	{
        GameObject WaterPrefab = Resources.Load<GameObject>($"Prefabs\\Blocks\\Water{state}");
        if(state == 0) WaterPrefab = Resources.Load<GameObject>($"Prefabs\\Blocks\\Water");
		GameObject water = null;

		switch (direction)
		{
			case Direction.Down:
				water = Instantiate(WaterPrefab, new Vector2(transform.position.x, transform.position.y - 1), Quaternion.identity);
				break;
			case Direction.Right:
				water = Instantiate(WaterPrefab, new Vector2(transform.position.x + 1, transform.position.y), Quaternion.identity);
				break;
			case Direction.Left:
				water = Instantiate(WaterPrefab, new Vector2(transform.position.x - 1, transform.position.y), Quaternion.Euler(0f, 180f, 0f));
				break;
		}
		water.GetComponent<WaterScript>().startFlowing(state, true);

	}

	private void replaceWaterBlock(Direction direction, int state)
	{
        // destroy previous water block
		switch (direction)
		{
			case Direction.Down:
                destroyWaterBlockAtPosition(new Vector2(transform.position.x, transform.position.y - 1));
				break;
			case Direction.Right:
				destroyWaterBlockAtPosition(new Vector2(transform.position.x + 1, transform.position.y));
				break;
			case Direction.Left:
				destroyWaterBlockAtPosition(new Vector2(transform.position.x - 1, transform.position.y));
				break;
		}
        createWaterBlock(direction, state); // replace water block
	}

	private List<WaterScript> getSurroundingWaterBlocks()
	{
        List<WaterScript> water = new List<WaterScript>();
        WaterScript aboveWater = getWaterAtPosition(new Vector2(transform.position.x, transform.position.y + 1));
        WaterScript rightWater = getWaterAtPosition(new Vector2(transform.position.x + 1, transform.position.y));
        WaterScript belowWater = getWaterAtPosition(new Vector2(transform.position.x, transform.position.y - 1));
        WaterScript leftWater = getWaterAtPosition(new Vector2(transform.position.x - 1, transform.position.y));
        if(aboveWater != null) water.Add(aboveWater);
        if(rightWater != null) water.Add(rightWater);
        if(belowWater != null) water.Add(belowWater);
        if(leftWater != null) water.Add(leftWater);

		water.Sort((a, b) => a.waterState.CompareTo(b.waterState)); // Sort the water list by waterState, lowest first

		return water;
	}

    private WaterScript getWaterAtPosition(Vector2 pos)
    {
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(pos, 0.45f, filter, results);
		if (results.Count == 0) return null;
        if (results.Count > 1) Debug.LogError("Found more than 1 water blocks at position: " + pos);
        return results[0].gameObject.GetComponent<WaterScript>();
	}

	private BlockType getBlockAtPosition(Vector2 pos)
    {
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Default") | LayerMask.GetMask("Tilemap") | LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(pos, 0.45f, filter, results);
        if (results.Count == 0) return BlockType.None;
        if (results[0].gameObject.name.StartsWith("Water"))
        {
            checkedWaterState = results[0].gameObject.GetComponent<WaterScript>().waterState;
			return BlockType.Water;
        }
        return BlockType.Block;
	}

    private void destroyWaterBlockAtPosition(Vector2 pos)
    {
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(LayerMask.GetMask("Water"));

		// Create a list to store the results
		List<Collider2D> results = new List<Collider2D>();

		// Check for overlaps
		Physics2D.OverlapCircle(pos, 0.45f, filter, results);
        if (results.Count == 0) {
            Debug.LogError("Tried to destroy water block at position: " + pos + " but no water block was found");
            return;
        }
        else if (results.Count > 1) Debug.LogError("Found more than one water blocks at position: " + pos);
        Destroy(results[0].gameObject);
	}

    private BlockType getBelowBlock()
    {
        return getBlockAtPosition(new Vector2(transform.position.x, transform.position.y - 1));
    }

    private BlockType getRightBlock()
    {
		return getBlockAtPosition(new Vector2(transform.position.x + 1, transform.position.y));
	}

	private BlockType getLeftBlock()
	{
		return getBlockAtPosition(new Vector2(transform.position.x - 1, transform.position.y));
	}

}

public enum BlockType
{
    Block,
    Water,
    None
}

public enum Direction
{
	Right,
	Left,
	Down,
    Up
}