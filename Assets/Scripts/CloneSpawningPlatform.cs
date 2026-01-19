using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CloneSpawningPlatform : MonoBehaviour
{
    [SerializeField]
    private UnityEvent spawnClone;
    [SerializeField]
    private GameObject cloneObject;
    [SerializeField]
    private int numberOfButtonPushers;
    private bool isPlayerInside;
    private bool isRecording = false;
    private GameObject currentClone;
    private List<PlayerInputFrame> currentRecording;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<ThirdPersonController>().IsPlayerControlled)
        {
            isPlayerInside = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<ThirdPersonController>().IsPlayerControlled)
        {
            isPlayerInside = false;
        }
    }
    private bool cloneQueuedLastFrame = false;

    private void Update()
    {
        bool clonePressed = PlayerInputProvider.Instance.CloneQueued;

        // Edge detection
        bool clonePressedEdge = clonePressed && !cloneQueuedLastFrame;

        if (clonePressedEdge)
        {
            if (isPlayerInside && !isRecording)
            {
                // Spawn clone and start recording
                SpawnClone();
                CloneInputRecorder.Instance.StartRecording();
                isRecording = true;
            }
            else if (isRecording)
            {
                // Stop recording anywhere
                CloneInputRecorder.Instance.StopRecording();
                isRecording = false;

                // Save the recording for this clone
                currentRecording = new List<PlayerInputFrame>(CloneInputRecorder.Instance.Frames);

                // Give the frames to the clone (but don't start playback yet)
                var cloneController = currentClone.GetComponent<CloneController>();
                cloneController.PassFrames(currentRecording);
            }
        }

        cloneQueuedLastFrame = clonePressed;
    }


    private void SpawnClone()
    {
        Debug.Log("Clone spawned");
        spawnClone?.Invoke();
        currentClone = Instantiate(cloneObject, CloneInputRecorder.Instance.gameObject.transform.position, CloneInputRecorder.Instance.gameObject.transform.rotation);
        // your spawn logic here
    }
}
