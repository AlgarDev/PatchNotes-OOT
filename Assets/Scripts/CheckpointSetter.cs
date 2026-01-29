using UnityEngine;
using UnityEngine.Events;

public class CheckpointSetter : MonoBehaviour
{
    [SerializeField] private Transform checkpoint;
    [SerializeField] private UnityEvent checkpointReached;
    private bool doOnce = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController player) && player.IsPlayerControlled && doOnce == false)
        {
            player.SetCheckpoint(checkpoint);
            player.CleanMyShit();
            checkpointReached.Invoke();
            doOnce = true;
        }
    }
}
