using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVChangerTest : MonoBehaviour
{
    [SerializeField] private int UVint;
    [SerializeField] private MeshRenderer[] meshRenderers;

    void Start()
    {
        foreach (MeshRenderer mr in meshRenderers)
        {
            Material mat = mr.material;
            mat.SetFloat("_UV", UVint);
        }
    }


}
