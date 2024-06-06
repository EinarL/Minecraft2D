using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityBar : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;


    public void updateDurability(DurabilityItem tool) // tool is a tool or armor
    {
        slider.value = tool.getDurability();

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void setMaximumDurability(DurabilityItem tool)
    {
        slider.maxValue = tool.getStartingDurability();
    }

}
