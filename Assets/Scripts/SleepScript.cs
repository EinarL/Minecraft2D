using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SleepScript : MonoBehaviour
{
    private DayProcessScript dayProcessScript;
	private PlayerControllerScript playerController;

	private bool isSleeping = false;
    private Image tint;

    // Start is called before the first frame update
    void Start()
    {
        tint = GetComponent<Image>();
		dayProcessScript = GameObject.Find("CM vcam").transform.Find("SunAndMoonTexture").GetComponent<DayProcessScript>();
		playerController = GameObject.Find("SteveContainer").GetComponent<PlayerControllerScript>();
	}

    // Update is called once per frame
    void Update()
    {
        if (isSleeping)
        {
            tint.color = new Color(0f, 0f, 0f, tint.color.a + 0.7f * Time.deltaTime);
            if(tint.color.a >= 0.7f)
            {
                isSleeping = false;
                StartCoroutine(wakeUp());
            }
        }
    }

    public void sleep()
    {
        isSleeping = true;
    }

    public void stopSleeping()
    {
        isSleeping = false;
        tint.color = new Color(0f, 0f, 0f, 0f);
        StopAllCoroutines();
	}

    private IEnumerator wakeUp()
    {
        yield return new WaitForSeconds(1.5f);
        dayProcessScript.setTimeToDay();
		tint.color = new Color(0f, 0f, 0f, 0f);
        playerController.stopSleeping(true);
	}
}
