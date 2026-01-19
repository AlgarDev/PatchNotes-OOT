using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct PlayerInputFrame
{
    public Vector2 move;
    public Vector2 look;
    public bool interact;
    public bool jump;
    public float cameraForward;
    //public bool run;
    //public bool crouch;
    //public bool changePerspective;
}
public class PlayerInputProvider : MonoBehaviour
{
    static public PlayerInputProvider Instance;
    private PlayerInputActions actions;
    private ThirdPersonController controller;

    private bool jumpQueued;
    private bool interactQueued;
    public bool CloneQueued { get; private set; }
    //private bool crouchQueued;
    //private bool changePerspectiveQueued;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    
    actions = new PlayerInputActions();
        actions.Player.Enable();
        controller = GetComponent<ThirdPersonController>();

        actions.Player.Jump.performed += _ => jumpQueued = true;
        actions.Player.Interact.performed += _ => interactQueued = true;
        actions.Player.Clone.performed += _ => CloneQueued = true;
        //actions.Player.Crouch.performed += _ => crouchQueued = true;
        //actions.Player.ChangePerspective.performed += _ => changePerspectiveQueued = true;
    }

    void FixedUpdate()
    {
        PlayerInputFrame frame = new PlayerInputFrame
        {
            move = actions.Player.Move.ReadValue<Vector2>(),
            look = actions.Player.Look.ReadValue<Vector2>(),
            jump = jumpQueued,
            interact = interactQueued,
            cameraForward = controller.cameraTransform.eulerAngles.y
            //run = actions.Player.Run.IsPressed(),
            //crouch = crouchQueued,
            //changePerspective = changePerspectiveQueued,
        };
        if(controller)
            controller.SetInput(frame);

        jumpQueued = false;
        interactQueued = false;
        CloneQueued = false;
        //crouchQueued = false;
        //changePerspectiveQueued = false;

        CloneInputRecorder recorder = GetComponent<CloneInputRecorder>();
        if (recorder != null)
        {
            recorder.Record(frame);
        }
    }
}
