using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private Animator canvasAnimator;
    [SerializeField] private Image sliderBar;
    [SerializeField] private float sliderMax = 0.9f;
    private ColorsSO colors;
    private Coroutine blinkCoroutine;
    [SerializeField] float blinkSpeed = 2.0f;
    [SerializeField] float saturationIntensity = 0.5f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        canvasAnimator = GetComponent<Animator>();
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
        if (sliderBar == null) return;
        sliderBar.fillAmount = currentValue / maxValue * sliderMax;
    }

    public void StartBlinkEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkEffectCoroutine());
    }

    public void StopBlinkEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            // Reset to original color (optional)
            // You might want to store the original color before starting the blink
        }
    }

    private IEnumerator BlinkEffectCoroutine()
    {
        if (sliderBar == null) yield break;

        Color originalColor = sliderBar.color;
        Color.RGBToHSV(originalColor, out float originalH, out float originalS, out float originalV);

        while (true)
        {
            // Increase saturation
            for (float t = 0; t < 1; t += Time.deltaTime * blinkSpeed)
            {
                if (sliderBar == null) yield break;

                float currentS = Mathf.Lerp(originalS, originalS + saturationIntensity, t);
                currentS = Mathf.Clamp01(currentS);
                sliderBar.color = Color.HSVToRGB(originalH, currentS, originalV);
                yield return null;
            }

            // Decrease saturation
            for (float t = 0; t < 1; t += Time.deltaTime * blinkSpeed)
            {
                if (sliderBar == null) yield break;

                float currentS = Mathf.Lerp(originalS + saturationIntensity, originalS, t);
                currentS = Mathf.Clamp01(currentS);
                sliderBar.color = Color.HSVToRGB(originalH, currentS, originalV);
                yield return null;
            }
        }
    }
}
