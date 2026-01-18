using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class CloneController : MonoBehaviour
{
    private PlayerMovementController movementController;
    public List<PlayerInputFrame> recordedFrames;
    private int frameIndex;

    /// <summary>
    /// Initializes the clone with a recording
    /// </summary>
    public void Initialize(List<PlayerInputFrame> frames)
    {
        recordedFrames = frames;
        frameIndex = 0;

        movementController = GetComponent<PlayerMovementController>();
    }

    private void FixedUpdate()
    {
        if (recordedFrames == null || frameIndex >= recordedFrames.Count)
            return;

        // Feed the current frame to the movement controller
        movementController.SetInput(recordedFrames[frameIndex]);

        // Move to the next frame for next FixedUpdate
        frameIndex++;
    }

    /// <summary>
    /// Returns true if the clone has finished replaying the input
    /// </summary>
    public bool IsFinished()
    {
        return recordedFrames != null && frameIndex >= recordedFrames.Count;
    }
}
