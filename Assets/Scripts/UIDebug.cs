using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour
{
    [SerializeField] Text isHolding;
    [SerializeField] Text isRecording;

    [SerializeField] ThirdPersonController thirdPersonController;
    [SerializeField] CloneManager cloneManager;

    private void Update()
    {
        isHolding.text = "Is Holding = " + thirdPersonController.isHolding;
        if (cloneManager.isRecording == true)
        {
            isRecording.text = "Is Recording = " + cloneManager.currentColor.ToString();
        }
        else
        {
            isRecording.text = "Is Recording = False";
        }
    }
}
