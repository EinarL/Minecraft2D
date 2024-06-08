using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayProcessScript : MonoBehaviour
{
    float prevPositionY = 25f;

	private Transform centerPoint; // The center of the circular path
	private float radius = 30f;     // Radius of the circle
	private float daySpeed = 0.00261799f; //0.00261799f // Speed of the sun's movement, this should take 20 minutes
	private float nightSpeed = 0.00523598f; //0.00523598f // Speed of the moons's movement, this should take 10 minutes
	private float angle = 0.53f;     // Current angle in radians

	private bool isDay = true;
	private bool isTransitioningToDay = true; // false if going from day to night, true if going from night to day
	private float transitionProcess = 0f;
	private float starTransitionProcess = 0f;
	private float prevAngle = 170f;
	private bool hasResetVariables = false;

	public Sprite moonTexture;
	public Sprite sunTexture;
	private SpriteRenderer spriteRenderer;
	private GameObject stars;
	private SpriteRenderer[] starRenderers;

	private Camera mainCamera;
	public Color dayColor; // The color of the sky during the day
	public Color nightColor;
	private Light2D sunLight;

	private IDataService dataService = JsonDataService.Instance;

	// Start is called before the first frame update
	void Start()
    {
		centerPoint = transform.parent.Find("CircularPathCenter").transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
		mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		sunLight = GameObject.Find("Sun").GetComponent<Light2D>();
		stars = transform.parent.Find("Stars").gameObject;
		//StartCoroutine(checkIfSunIsHigh());
		starRenderers = stars.GetComponentsInChildren<SpriteRenderer>();

		if (dataService.exists("day-time.json"))
		{
			// returns: new object[] { angle, isDay, isTransitioningToDay, transitionProcess, prevAngle, hasResetVariables, starTransitionProcess };
			object[] timeInfo = dataService.loadData<object[]>("day-time.json");
			angle = Convert.ToSingle(timeInfo[0]);
			isDay = (bool)timeInfo[1];
			isTransitioningToDay = (bool)timeInfo[2];
			transitionProcess = Convert.ToSingle(timeInfo[3]);
			prevAngle = Convert.ToSingle(timeInfo[4]);
			hasResetVariables = (bool)timeInfo[5];
			starTransitionProcess = Convert.ToSingle(timeInfo[6]);

			if (!isDay) spriteRenderer.sprite = moonTexture;
			float angleInDegrees = angle * Mathf.Rad2Deg;
			if(angleInDegrees <= 170f && angleInDegrees >= 10f && !isDay)
			{
				sunLight.intensity = 0.2f;
				mainCamera.backgroundColor = nightColor;
			}
		} 
		if (isDay) UpdateStarsOpacity(0f);
	}

	// Update is called once per frame
	void Update()
	{
		// Update the angle based on time and speed
		if(isDay) angle += daySpeed * Time.deltaTime;
		else angle += nightSpeed * Time.deltaTime;

		// Check if the angle has passed 190 degrees
		if (angle * Mathf.Rad2Deg > 190f && angle * Mathf.Rad2Deg < 350f)
		{
			// Teleport to -10 degrees
			angle = -10f * Mathf.Deg2Rad;
			isDay = !isDay;
			if (isDay) spriteRenderer.sprite = sunTexture;
			else spriteRenderer.sprite = moonTexture;
		}

		// Calculate the new position using sine and cosine functions
		float x = Mathf.Cos(angle) * radius;
		float y = Mathf.Sin(angle) * radius;

		// Set the new position relative to the center point
		transform.position = new Vector3(centerPoint.position.x + x, centerPoint.position.y + y, transform.position.z);

		
		float angleInDegrees = angle * Mathf.Rad2Deg;
		if(angleInDegrees > 80 && angleInDegrees < 100)
		{
			if (!hasResetVariables)
			{
				isTransitioningToDay = !isTransitioningToDay;
				prevAngle = 170f;
				transitionProcess = 0f;
				starTransitionProcess = 0f;
				hasResetVariables = true;
			}
		}else hasResetVariables = false;
		// Gradually change the background color for day and night
		if (!isTransitioningToDay && (angleInDegrees > 170f || angleInDegrees < 10f)) // day to night
		{
			if (prevAngle >= 170f && angleInDegrees < 170f) prevAngle = -10f;

			transitionProcess += (angleInDegrees - prevAngle) / 40f;
			sunLight.intensity = Mathf.Lerp(1f, 0.2f, transitionProcess); // lower intensity of the sunlight
			mainCamera.backgroundColor = Color.Lerp(dayColor, nightColor, transitionProcess); // change background to night color
			if (transitionProcess >= .5f)
			{
				starTransitionProcess += (angleInDegrees - prevAngle) / 20f;
				UpdateStarsOpacity(starTransitionProcess); // Stars fade in
			}
			prevAngle = angleInDegrees;
		}
		else if (isTransitioningToDay && (angleInDegrees > 170f || angleInDegrees < 10f)) // night to day
		{
			if (prevAngle >= 170f && angleInDegrees < 170f) prevAngle = -10f;

			transitionProcess += (angleInDegrees - prevAngle) / 40f;
			sunLight.intensity = Mathf.Lerp(0.2f, 1f, transitionProcess); // increase intensity of the sunlight
			mainCamera.backgroundColor = Color.Lerp(nightColor, dayColor, transitionProcess);
			if (transitionProcess <= .5f)
			{
				starTransitionProcess += (angleInDegrees - prevAngle) / 20f;
				UpdateStarsOpacity(1 - starTransitionProcess); // Stars fade out
			}
			prevAngle = angleInDegrees;
		}
	}

	private void UpdateStarsOpacity(float alpha)
	{
		foreach (var renderer in starRenderers)
		{
			Color color = renderer.color;
			color.a = alpha;
			renderer.color = color;
		}
	}

	public void setTimeToDay()
	{
		angle = 0.53f;
		isDay = true;
		spriteRenderer.sprite = sunTexture;
		isTransitioningToDay = true;
		sunLight.intensity = 1f;
		mainCamera.backgroundColor = dayColor;
		UpdateStarsOpacity(0f);
		hasResetVariables = false; // this will refresh the variables in the Update() function
	}

	/**
	 * returns the data that is neccessary to save to save the time of day
	 */
	public object[] getDataToSave()
	{
		return new object[] { angle, isDay, isTransitioningToDay, transitionProcess, prevAngle, hasResetVariables, starTransitionProcess };
	}

	private void OnDrawGizmos()
	{
		if (centerPoint != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(centerPoint.position, radius);
		}
	}

	/**
     * checks if the camera is above some value, then if it goes higher then the sun goes a bit down
     * 
     */
	private IEnumerator checkIfSunIsHigh()
    {
        while (true)
        {
			if (transform.parent.position.y > 20)
			{
				if (transform.parent.position.y > prevPositionY) transform.position = new Vector2(transform.position.x, transform.position.y - (transform.parent.position.y - prevPositionY - (Mathf.Abs(transform.parent.position.y - prevPositionY) / 2)));
				prevPositionY = transform.parent.position.y;
				Debug.Log("aswdwad");
			}
            yield return new WaitForSeconds(0.01f);
		}
    }

	public bool isDaytime()
	{
		return isDay;
	}

	public bool canSleep()
	{
		return !isDay;
	}
}
