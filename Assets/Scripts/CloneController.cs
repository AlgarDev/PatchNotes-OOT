using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(CharacterController))]
public class CloneController : MonoBehaviour
{
    [SerializeField] private PlayerMovementController playerPrefabController; // For reference if needed
    [SerializeField] private CharacterController controller;

    private CloneInputRecorder inputRecorder;
    private int frameIndex = 0;

    // Movement variables (copy from your player)
    private Vector3 velocity;
    private Vector3 currentDirection;
    private Vector3 smoothVelocity;

    public void Initialize(CloneInputRecorder recorder)
    {
        inputRecorder = recorder;
        frameIndex = 0;

        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (inputRecorder == null || frameIndex >= inputRecorder.Frames.Count)
            return;

        //// Grab the next frame
        //playerPrefabController.SetInput(recorder.Frames[frameIndex]);

        //HandleMovement(inputFrame);
        //HandleLook(inputFrame.look);

        frameIndex++;
    }

    private void HandleMovement(PlayerInputFrame frame)
    {
        float speed = playerPrefabController.WalkSpeed;

        Vector3 input = new Vector3(frame.move.x, 0, frame.move.y).normalized;
        Vector3 direction = transform.TransformDirection(input);
        currentDirection = Vector3.SmoothDamp(currentDirection, direction * speed, ref smoothVelocity, playerPrefabController.SmoothTime);

        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += playerPrefabController.Gravity * Time.deltaTime;
        }

        if (frame.jump && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(playerPrefabController.JumpHeight * -2f * playerPrefabController.Gravity);
        }

        controller.Move((currentDirection + velocity) * Time.deltaTime);
    }

    private void HandleLook(Vector2 look)
    {
        // Optional: add rotation if needed for visual facing direction
        float mouseX = look.x * playerPrefabController.MouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }
}

