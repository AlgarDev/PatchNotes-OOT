using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController player))
        {
            if (player.IsPlayerControlled)
            {
                print("Player fell");
                player.RespawnAtCheckpoint();
            }
            else
            {
                if (player.TryGetComponent(out CloneController clone))
                {
                    clone.activeSpawner.StopRecording();
                    print("Clone fell");
                }
            }
        }
    }
}
