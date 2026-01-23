using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HourglassManager : MonoBehaviour
{
    private ColorsSO colors;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Image sliderBar;
    [SerializeField]
    private Animator canvasAnimator;

    void Start()
    {
        colors = Resources.Load<ColorsSO>("ColorsSO");
    }

    void UIState(bool state)
    {
        if (state)
        {
            canvasAnimator.Play("CanvasAppear");
        }
        else
        {
            canvasAnimator.Play("CanvasDisappear");
        }
    }

    void ChangeSliderColor(ColorRef color)
    {
        if (colors == null) return;
        sliderBar.color = colors.ReturnHourglassColor(color);
    }

    void UpdateSliderValue(float currentValue, float maxValue)
    {
        if (slider == null) return;
        slider.value = currentValue / maxValue;
    }
}
