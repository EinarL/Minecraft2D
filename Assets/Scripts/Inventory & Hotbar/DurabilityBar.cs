using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityBar : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;


    public void updateDurability(ToolInstance tool)
    {
        slider.value = tool.getDurability();

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void setMaximumDurability(ToolInstance tool)
    {
        slider.maxValue = tool.getStartingDurability();
    }

}
