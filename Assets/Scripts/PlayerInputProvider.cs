using System;
using UnityEngine;

[Serializable]
public struct PlayerInputFrame
{
    public Vector2 move;
    public Vector2 look;
    public bool interact;
    public bool jump;
    public float cameraForward;
    public bool run;
    public bool crouch;
    public bool changePerspective;
}
public class PlayerInputProvider : MonoBehaviour
{
    private PlayerInputActions actions;
    private ThirdPersonController controller;

    private bool jumpQueued;
    private bool crouchQueued;
    private bool changePerspectiveQueued;
    private bool interactQueued;

    void Awake()
    {
        actions = new PlayerInputActions();
        actions.Player.Enable();
        controller = GetComponent<ThirdPersonController>();

        actions.Player.Jump.performed += _ => jumpQueued = true;
        actions.Player.Crouch.performed += _ => crouchQueued = true;
        actions.Player.ChangePerspective.performed += _ => changePerspectiveQueued = true;
        actions.Player.Interact.performed += _ => interactQueued = true;
    }

    void FixedUpdate()
    {
        PlayerInputFrame frame = new PlayerInputFrame
        {
            move = actions.Player.Move.ReadValue<Vector2>(),
            look = actions.Player.Look.ReadValue<Vector2>(),
            run = actions.Player.Run.IsPressed(),
            jump = jumpQueued,
            crouch = crouchQueued,
            changePerspective = changePerspectiveQueued,
            interact = interactQueued,
            cameraForward = controller.cameraTransform.eulerAngles.y
        };
        if(controller)
            controller.SetInput(frame);

        jumpQueued = false;
        crouchQueued = false;
        changePerspectiveQueued = false;
        interactQueued = false;

        CloneInputRecorder recorder = GetComponent<CloneInputRecorder>();
        if (recorder != null)
        {
            recorder.Record(frame);
        }
    }
}
