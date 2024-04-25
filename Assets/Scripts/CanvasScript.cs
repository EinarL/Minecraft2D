using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    GameObject deathScreen;

    // Start is called before the first frame update
    void Start()
    {
        deathScreen = gameObject.transform.Find("DeathScreen").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showDeathScreen()
    {
        deathScreen.SetActive(true);
    }

    public void closeDeathScreen()
    {
        deathScreen.SetActive(false);
    }
}
