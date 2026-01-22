using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CloneSpawningPlatform : MonoBehaviour
{

    [Header("Clone")]
    [SerializeField] private UnityEvent spawnClone;
    [SerializeField] private GameObject cloneObject;

    [Header("Recording Timer")]
    [SerializeField] private float recordingDuration = 10f;

    public float timerRemaining;
    public bool timerPaused;

    private bool isPlayerInside;
    private bool isRecording = false;

    private GameObject currentClone;
    private List<PlayerInputFrame> currentRecording;

    private Vector3 recordingStartPosition;
    private Quaternion recordingStartRotation;
    private ThirdPersonController player;

    private bool cloneQueuedLastFrame;
    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled)
        {
            isPlayerInside = true;
            player = controller;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null && controller.IsPlayerControlled)
        {
            isPlayerInside = false;
        }
    }
    private void Update()
    {
        HandleInput();
        UpdateTimer();
    }

    private void HandleInput()
    {
        bool clonePressed = PlayerInputProvider.Instance.CloneQueued;
        bool clonePressedEdge = clonePressed && !cloneQueuedLastFrame;

        if (clonePressedEdge)
        {
            if (isPlayerInside && !isRecording)
            {
                StartRecording();
            }
            else if (isRecording)
            {
                StopRecording();
            }
        }

        cloneQueuedLastFrame = clonePressed;
    }

    private void UpdateTimer()
    {
        if (!isRecording || timerPaused)
            return;

        timerRemaining -= Time.deltaTime;

        if (timerRemaining <= 0f)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        // Start playback on existing clones
        foreach (var clone in FindObjectsOfType<CloneController>())
        {
            clone.StartPlayback();
        }

        SpawnClone();

        recordingStartPosition = player.transform.position;
        recordingStartRotation = player.transform.rotation;
        timerRemaining = recordingDuration;
        timerPaused = false;

        CloneInputRecorder.Instance.StartRecording();
        isRecording = true;
    }

    private void StopRecording()
    {
        if (!isRecording)
            return;

        CloneInputRecorder.Instance.StopRecording();
        isRecording = false;

        currentRecording = new List<PlayerInputFrame>(CloneInputRecorder.Instance.Frames);

        var cloneController = currentClone.GetComponent<CloneController>();
        cloneController.PassFrames(currentRecording);

        // Teleport player back
        player.controller.enabled = false;
        player.transform.position = recordingStartPosition;
        player.transform.rotation = recordingStartRotation;
        player.controller.enabled = true;
    }

    private void SpawnClone()
    {
        Debug.Log("Clone spawned");
        spawnClone?.Invoke();
        currentClone = Instantiate(cloneObject, CloneInputRecorder.Instance.gameObject.transform.position, CloneInputRecorder.Instance.gameObject.transform.rotation);
        // your spawn logic here
    }

    public void PauseTimer(bool pause)
    {
        timerPaused = pause;
    }
}
