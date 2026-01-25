using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private Animator canvasAnimator;
    [SerializeField] private Slider slider;
    [SerializeField] private Image sliderBar;
    private ColorsSO colors;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        canvasAnimator = GetComponent<Animator>();
        slider = GetComponentInChildren<Slider>();
        colors = Resources.Load<ColorsSO>("ColorsSO");
    }

    public void UIState(bool state)
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

    public void ChangeSliderColor(ColorRef color)
    {
        if (colors == null) return;
        sliderBar.color = colors.ReturnHourglassColor(color);
    }

    public void UpdateSliderValue(float currentValue, float maxValue)
    {
        if (slider == null) return;
        slider.value = currentValue / maxValue;
    }
}
