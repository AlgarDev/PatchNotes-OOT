using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIFloat : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatDistance = 10f;
    [SerializeField] private bool floatHorizontal = true;
    [SerializeField] private bool floatVertical = true;
    [SerializeField] private bool randomizeStart = true;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float randomOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;

        if (randomizeStart)
        {
            randomOffset = Random.Range(0f, 2f * Mathf.PI);
        }
    }

    private void Update()
    {
        // Calculate the floating offset using sine wave for smooth movement
        float time = Time.time * floatSpeed + randomOffset;

        float xOffset = floatHorizontal ? Mathf.Sin(time) * floatDistance : 0f;
        float yOffset = floatVertical ? Mathf.Cos(time * 0.8f) * floatDistance * 0.5f : 0f;

        // Apply the offset to the original position
        rectTransform.anchoredPosition = startPosition + new Vector2(xOffset, yOffset);
    }

    // Optionally reset position when disabled
    private void OnDisable()
    {
        rectTransform.anchoredPosition = startPosition;
    }
}