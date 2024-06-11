using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private CanvasGroup selectedTexture;
    private TitleScreenScript titleScreenScript;

    private bool isSelected = false;
    // Start is called before the first frame update
    void Start()
    {
        selectedTexture = transform.Find("Selected").GetComponent<CanvasGroup>();
        titleScreenScript = GameObject.Find("Canvas").transform.Find("Panel").GetComponent<TitleScreenScript>();
    }

    public string getWorldName()
    {
        return transform.Find("WorldName").GetComponent<TextMeshProUGUI>().text;
    }
    /**
     * gets called from TitleScreenScript when another world gets selected 
     */
    public void deselectWorld()
    {
        isSelected = false;
		selectedTexture.alpha = 0f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
        if(!isSelected) selectedTexture.alpha = 0.1f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!isSelected) selectedTexture.alpha = 0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
        if (isSelected) return;
		selectedTexture.alpha = 1f;
		isSelected = true;
		titleScreenScript.setSelectedWorld(this);
	}
}
