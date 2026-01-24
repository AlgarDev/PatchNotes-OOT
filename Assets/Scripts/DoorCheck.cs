using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCheck : MonoBehaviour
{
    [SerializeField] GameObject blocker;

    public void ChangeState(bool state)
    {
        if (blocker != null)
        {
            blocker.SetActive(!state);
        }
    }
}
