using UnityEngine;

public class CheckpointSetter : MonoBehaviour
{
    [SerializeField]
    private Transform checkpoint;
    private bool doOnce = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController player) && player.IsPlayerControlled && doOnce == false)
        {
            player.SetCheckpoint(checkpoint);
            player.CleanMyShit();
            doOnce = true;
        }
    }
}
