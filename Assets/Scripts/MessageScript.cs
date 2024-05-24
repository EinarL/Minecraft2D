using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/**
 * this class is used to display a message to the player
 */
public class MessageScript : MonoBehaviour
{

    private TextMeshProUGUI message;
    private bool makeMessageFadeOut = false;

    // Start is called before the first frame update
    void Start()
    {
        message = GetComponent<TextMeshProUGUI>();
        message.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (makeMessageFadeOut)
        {
            message.alpha -= 0.01f;
            if (message.alpha <= 0f) makeMessageFadeOut = false;
        }
    }

    public void displayMessage(string text)
    {
        StopAllCoroutines();
        message.text = text;
        message.alpha = 1f;

        IEnumerator fadeOutTimer()
        {
            yield return new WaitForSeconds(3f);
            makeMessageFadeOut = true;
        }
        StartCoroutine(fadeOutTimer());
    }
}
