using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorRef
{
    Green,
    Blue,
    Red,
    Pink
}
[CreateAssetMenu(fileName = "ColorsSO", menuName = "ScriptableObjects/ColorsSO")]
public class ColorsSO : ScriptableObject
{
    [Header("Bricks and Enemies")]
    [SerializeField] public Color Green;
    [SerializeField] public Color Blue;
    [SerializeField] public Color Red;
    [SerializeField] public Color Pink;

    public Color ReturnColor(ColorRef refName)
    {
        Color color = Color.white;
        switch (refName)
        {
            case ColorRef.Green:
                color = Green;
                break;
            case ColorRef.Blue:
                color = Blue;
                break;
            case ColorRef.Red:
                color = Red;
                break;
            case ColorRef.Pink:
                color = Pink;
                break;
            default:
                color = Green;
                break;
        }
        return color;
    }

}
