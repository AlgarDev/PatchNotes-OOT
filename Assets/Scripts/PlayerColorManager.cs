using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{

    [SerializeField] private MeshRenderer[] meshRenderers;
    [SerializeField] private MeshRenderer hourglassSand;
    [SerializeField] private MeshRenderer hourglassPlatform;
    [SerializeField] private GameObject hourglass;
    [SerializeField] private float maxSand = 0.7f;
    private ColorsSO colors;
    private float targetSandValue;
    //Main menu:
    [SerializeField] private ColorRef spawnColor;

    void Start()
    {
        //ChangeColor(ColorRef.Pink);
        //DisableHourglassSand();

        if (spawnColor != ColorRef.Green)
        {
            ChangeColor(spawnColor);
            GhostState(true);
            DisableHourglass();
        }
    }

    public void ChangeColor(ColorRef color)
    {
        int UVint = 0;
        if (color == ColorRef.Green) UVint = 1;
        if (color == ColorRef.Blue) UVint = 2;
        if (color == ColorRef.Red) UVint = 3;
        if (color == ColorRef.Pink) UVint = 4;

        foreach (MeshRenderer mr in meshRenderers)
        {
            Material mat = mr.material;
            mat.SetFloat("_ColorInt", UVint);
        }

        Material mat2 = hourglassSand.material;
        mat2.SetFloat("_ColorInt", UVint);

        if (hourglassPlatform != null)
        {
            Material mat3 = hourglassPlatform.material;
            mat3.SetFloat("_ColorInt", UVint);
        }

        //Change grabable hourglass as well
    }

    public void GhostState(bool state)
    {
        int i = 0;
        if (state) i = 1;
        foreach (MeshRenderer mr in meshRenderers)
        {
            Material mat = mr.material;
            mat.SetInt("_Ghost", i);
        }

    }

    public void EnableHourglassSand()
    {
        hourglassSand.gameObject.SetActive(true);
        Debug.Log(hourglassSand.gameObject.activeSelf);
    }

    public void UpdateTargetValue(float target)
    {
        if (target < 0) target = 0;
        if (target > 1) target = 1;

        target = target * maxSand;

        Material mat = hourglassSand.material;
        mat.SetFloat("_Fill", target);
    }

    public void PauseHourglass()
    {
        hourglassSand.gameObject.SetActive(true);
    }

    public void DisableHourglassSand()
    {
        hourglassSand.gameObject.SetActive(false);
    }

    public void DisableHourglass()
    {
        hourglass.SetActive(false);
    }
}
