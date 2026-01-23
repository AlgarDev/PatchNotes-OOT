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
    [Header("Hourglass Sand")]
    [SerializeField] public Color HGreen;
    [SerializeField] public Color HBlue;
    [SerializeField] public Color HRed;
    [SerializeField] public Color HPink;
    [Header("Platform")]
    [SerializeField] public Color PGreen;
    [SerializeField] public Color PBlue;
    [SerializeField] public Color PRed;
    [SerializeField] public Color PPink;

    public Color ReturnHourglassColor(ColorRef refName)
    {
        Color color = Color.white;
        switch (refName)
        {
            case ColorRef.Green:
                color = HGreen;
                break;
            case ColorRef.Blue:
                color = HBlue;
                break;
            case ColorRef.Red:
                color = HRed;
                break;
            case ColorRef.Pink:
                color = HPink;
                break;
            default:
                color = HGreen;
                break;
        }
        return color;
    }

    public Color ReturnPlatformColor(ColorRef refName)
    {
        Color color = Color.white;
        switch (refName)
        {
            case ColorRef.Green:
                color = PGreen;
                break;
            case ColorRef.Blue:
                color = PBlue;
                break;
            case ColorRef.Red:
                color = PRed;
                break;
            case ColorRef.Pink:
                color = PPink;
                break;
            default:
                color = PGreen;
                break;
        }
        return color;
    }

}
