using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    public static PlayerMovementController Instance;
    [SerializeField] public Transform cameraTransform; //camera height should be = standing height - 0.5
    [Header("Toggles")]
    [SerializeField] private bool canJump;
    [SerializeField] private bool canCrouch;
    [SerializeField] private bool canRun;
    [SerializeField] private bool canHeadbob;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float jumpHeight = 1.5f;   

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private LayerMask blockingLayers;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float lookLimit = 80f;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.05f;
    [SerializeField] private float bobSpeedMultiplier = 1.0f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool runInput;
    private float currentTargetHeight;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentDirection;
    private Vector3 smoothVelocity;
    private float cameraPitch = 0f;

    private float bobTimer = 0f;
    private Vector3 originalCamLocalPos;
    private float targetCameraHeight;
    private float currentCameraHeight;
    private bool isCrouching = false;

    private PlayerInputActions inputActions;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();   
        inputActions.Player.Crouch.performed += _ => ToggleCrouch();
        inputActions.Player.Jump.performed += _ => Jump();
    }

    private void Start()
    {
        if (!GameManager.isGameRunning)
            Debug.LogWarning("No Game Manager!");

        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        originalCamLocalPos = cameraTransform.localPosition;

        isCrouching = false;
        currentTargetHeight = standingHeight;
        controller.height = standingHeight;

        targetCameraHeight = standingHeight - 0.5f;
        currentCameraHeight = targetCameraHeight;
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, currentCameraHeight, cameraTransform.localPosition.z
);

    }


    private void Update()
    {
        if (GameManager.isGameRunning)
        {

            moveInput = canMove ? inputActions.Player.Move.ReadValue<Vector2>() : Vector2.zero;
            lookInput = canLook ? inputActions.Player.Look.ReadValue<Vector2>() : Vector2.zero;
            runInput = canRun && inputActions.Player.Run.IsPressed();

            HandleMouseLook();
            HandleMovement();
            if (canHeadbob && canMove)
                HandleHeadBob();
            if (canCrouch)
                HandleCrouchTransition();
        }
    }

    void HandleMouseLook()
    {
        if (!canLook)
            return;
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookLimit, lookLimit);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (!canMove)
        {
            velocity.y = 0f;
            return;
        }

        float targetSpeed = isCrouching ? crouchSpeed : (runInput ? runSpeed : walkSpeed);

        Vector3 input = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 direction = transform.TransformDirection(input);
        currentDirection = Vector3.SmoothDamp(currentDirection, direction * targetSpeed, ref smoothVelocity, smoothTime);

        if (controller.isGrounded)
        { 
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
            velocity.y += gravity * Time.deltaTime;
        controller.Move((currentDirection + velocity) * Time.deltaTime);
    }
    private void Jump()
    {
        if (!canJump)
            return;
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ToggleCrouch()
    {
        if (canCrouch && canMove)
        {
            if (isCrouching)
            {
                float spaceCheckHeight = standingHeight - crouchingHeight;
                Vector3 bottom = transform.position + Vector3.up * crouchingHeight;
                Vector3 top = bottom + Vector3.up * spaceCheckHeight;

                Collider[] hits = Physics.OverlapCapsule(bottom, top, controller.radius - 0.05f, blockingLayers);
                if (hits.Length == 0)
                {
                    isCrouching = false;
                    currentTargetHeight = standingHeight;
                    targetCameraHeight = standingHeight - 0.5f;
                }
            }
            else
            {
                isCrouching = true;
                currentTargetHeight = crouchingHeight;
                targetCameraHeight = crouchingHeight - 0.5f;
            }
        }
    }
    void HandleCrouchTransition()
    {
        controller.height = Mathf.Lerp(controller.height, currentTargetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0f, controller.height / 2f, 0f);
        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * crouchTransitionSpeed);
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, currentCameraHeight, cameraTransform.localPosition.z
        );
    }

    void HandleHeadBob()
    {
        //currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * crouchTransitionSpeed);

        bool isMoving = controller.velocity.magnitude > 0.1f && controller.isGrounded;

        if (canHeadbob && isMoving)
        {
            float speed = moveInput.magnitude * bobSpeedMultiplier;
            bobTimer += Time.deltaTime * speed;

            float bobX = Mathf.Cos(bobTimer * bobFrequency) * bobHorizontalAmplitude;
            float bobY = Mathf.Sin(bobTimer * bobFrequency * 2f) * bobVerticalAmplitude;

            cameraTransform.localPosition = new Vector3(originalCamLocalPos.x + bobX, Mathf.Lerp(cameraTransform.localPosition.y, currentCameraHeight + bobY, Time.deltaTime * 10f), originalCamLocalPos.z);
        }
        else
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(originalCamLocalPos.x, currentCameraHeight, originalCamLocalPos.z), Time.deltaTime * 5f);
            bobTimer = 0f;
        }
    }

    public void ChangeCanLook(bool value)
    {
        canLook = value;
    }
    public void ChangeCanMove(bool value)
    {
        canMove = value;
        if (!value)
        {
            currentDirection = Vector3.zero;
            smoothVelocity = Vector3.zero;
        }
    }
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
