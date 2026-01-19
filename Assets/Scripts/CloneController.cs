using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ThirdPersonController))]
public class CloneController : MonoBehaviour
{
    private ThirdPersonController movementController;
    private List<PlayerInputFrame> recordedFrames;
    private int frameIndex;

    private bool hasFinished;
    private bool isPlaying; // NEW

    [SerializeField]
    private GameObject corpseObject;

    [Header("Events")]
    public UnityEvent onFinishedPlayback;

    private void Awake()
    {
        movementController = GetComponent<ThirdPersonController>();
    }

    public void PassFrames(List<PlayerInputFrame> frames)
    {
        recordedFrames = frames;
        frameIndex = 0;
        hasFinished = false;
        isPlaying = false; // don't start immediately
    }

    // Explicitly call this when you want playback to start
    public void StartPlayback()
    {
        if (recordedFrames == null || recordedFrames.Count == 0) return;
        isPlaying = true;
    }

    private void FixedUpdate()
    {
        if (!isPlaying || recordedFrames == null || hasFinished) return;

        if (frameIndex < recordedFrames.Count)
        {
            movementController.SetInput(recordedFrames[frameIndex]);
            frameIndex++;
            return;
        }

        // Finished playback
        hasFinished = true;
        OnFinished();
    }

    private void OnFinished()
    {
        var corpse = Instantiate(corpseObject, transform.position + Vector3.up, Quaternion.identity);
        corpse.GetComponent<Rigidbody>().velocity = Vector3.zero;
        corpse.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        corpse.GetComponent<Rigidbody>().isKinematic = true;
        movementController.SetInput(default);
        Debug.Log($"{name} finished playback");
        Destroy(gameObject);
    }

    public bool IsFinished() => hasFinished;
}
