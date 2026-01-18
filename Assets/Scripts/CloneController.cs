using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
public class CloneController : MonoBehaviour
{
    private CloneInputRecorder inputRecorder;
    private PlayerMovementController movementController;

    private int frameIndex;

    public void Initialize(CloneInputRecorder recorder)
    {
        inputRecorder = recorder;
        frameIndex = 0;

        movementController = GetComponent<PlayerMovementController>();

        // Clones should move and rotate, but not control a player camera
        movementController.ChangeCanMove(true);
        movementController.ChangeCanLook(false);
    }

    private void Update()
    {
        if (inputRecorder == null)
            return;

        if (frameIndex >= inputRecorder.Frames.Count)
            return;

        movementController.SetInput(inputRecorder.Frames[frameIndex]);
        frameIndex++;
    }
}
