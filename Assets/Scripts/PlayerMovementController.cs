using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{


    public static PlayerMovementController Instance;
    [Header("Perspective")]
    [SerializeField] private Transform firstPersonCamera;
    [SerializeField] private Transform thirdPersonCamera;
    [SerializeField] private bool isFirstPerson = true;
    private Vector3 thirdPersonOriginalLocalPos;
    private Vector3 firstPersonOriginalLocalPos;
    private Camera firstPersonCam;
    private Camera thirdPersonCam;

    [SerializeField] public Transform cameraTransform;

    [Header("Toggles")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;
    [SerializeField] private bool canJump = false;
    [SerializeField] private bool canCrouch = false;
    [SerializeField] private bool canRun = false;
    [SerializeField] private bool canHeadbob = false;
    [SerializeField] private bool canChangePerspective = true;
    
    [field:Header("Movement")]
    [field: SerializeField] public float WalkSpeed { private set; get; } = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1f;
    [field: SerializeField] public float Gravity { private set; get; } = -9.81f;
    [field:SerializeField] public float SmoothTime { private set; get; } = 0.1f;
    [field: SerializeField] public float JumpHeight { private set; get; } = 1.5f;   

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private LayerMask blockingLayers;

    [field: Header("Mouse Look")]
    [field:SerializeField] public float MouseSensitivity { private set; get; } = 0.5f;
    [SerializeField] private float lookLimit = 80f;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.05f;
    [SerializeField] private float bobSpeedMultiplier = 1.0f;
    [Header("Input Recording/Clones")]
    [SerializeField] private bool isPlayerControlled = true;
    private PlayerInputFrame currentInput;
    private PlayerInputFrame previousInput;
    [SerializeField] private CloneInputRecorder inputRecorder;

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
    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        previousInput = currentInput = new PlayerInputFrame();

        if (firstPersonCamera != null)
            firstPersonCam = firstPersonCamera.GetComponentInChildren<Camera>();
        if (thirdPersonCamera != null)
            thirdPersonCam = thirdPersonCamera.GetComponentInChildren<Camera>();

    }

    private void Start()
    {
        if (!GameManager.isGameRunning)
            Debug.LogWarning("No Game Manager!");

        controller = GetComponent<CharacterController>();
        if (isPlayerControlled)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        thirdPersonOriginalLocalPos = thirdPersonCamera.localPosition;
        firstPersonOriginalLocalPos = firstPersonCamera.localPosition;

        SetPerspective(isFirstPerson);

        originalCamLocalPos = cameraTransform.localPosition;

        if (isFirstPerson)
        {
            targetCameraHeight = standingHeight - 0.5f;

            currentCameraHeight = targetCameraHeight;
            cameraTransform.localPosition = new Vector3(firstPersonOriginalLocalPos.x, currentCameraHeight, firstPersonOriginalLocalPos.z);
        }
        else
        {
            cameraTransform.localPosition = thirdPersonOriginalLocalPos;
        }

            currentTargetHeight = standingHeight;
        controller.height = standingHeight;
        isCrouching = false;



    }


    private void Update()
    {
        if (!GameManager.isGameRunning)
            return;

        HandleEdgeInputs();
        HandleMouseLook();
        HandleMovement();

        if (canHeadbob && canMove)
            HandleHeadBob();

        if (canCrouch)
            HandleCrouchTransition();

        if (isPlayerControlled && inputRecorder != null)
        {
            inputRecorder.Record(currentInput);
        }

        previousInput = currentInput;
    }

    // ================= INPUT =================

    public void SetInput(PlayerInputFrame input)
    {
        currentInput = input;
    }

    private void HandleEdgeInputs()
    {
        if (canJump && currentInput.jump && !previousInput.jump)
            Jump();

        if (canCrouch && currentInput.crouch && !previousInput.crouch)
            ToggleCrouch();

        if (currentInput.changePerspective && !previousInput.changePerspective)
            ChangePerspective();
    }
    //===========MOVEMENT===================
    void HandleMovement()
    {
        if (!canMove)
        {
            return;
        }

        float targetSpeed = isCrouching ? crouchSpeed : (currentInput.run && canRun ? runSpeed : WalkSpeed);

        Vector3 input = new Vector3(currentInput.move.x, 0, currentInput.move.y).normalized;
        Vector3 direction = transform.TransformDirection(input);
        currentDirection = Vector3.SmoothDamp(currentDirection, direction * targetSpeed, ref smoothVelocity, SmoothTime);

        if (controller.isGrounded)
        { 
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
            velocity.y += Gravity * Time.deltaTime;
        controller.Move((currentDirection + velocity) * Time.deltaTime);
    }
    private void Jump()
    {
        if (!canJump)
            return;
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }
    //===========lOOK===================
    void HandleMouseLook()
    {
        if (!canLook)
            return;

        float mouseX = currentInput.look.x * MouseSensitivity;
        float mouseY = currentInput.look.y * MouseSensitivity;

        // Accumulate & clamp pitch for BOTH modes
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookLimit, lookLimit);

        // Apply pitch to camera pivot
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Yaw always rotates the player
        transform.Rotate(Vector3.up * mouseX);
    }


    //=====================CROUCH==========================
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
        // Smoothly update CharacterController height and center
        controller.height = Mathf.Lerp(controller.height, currentTargetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0f, controller.height / 2f, 0f);

        // Calculate target heights for pivots
        float firstPersonTargetY = isCrouching ? crouchingHeight - 0.5f : standingHeight - 0.5f;
        float thirdPersonTargetY = isCrouching ? crouchingHeight : standingHeight; // adjust offset if needed

        // Update first-person pivot
        if (firstPersonCamera != null)
        {
            Vector3 fpPos = firstPersonCamera.localPosition;
            firstPersonCamera.localPosition = new Vector3(fpPos.x, Mathf.Lerp(fpPos.y, firstPersonTargetY, Time.deltaTime * crouchTransitionSpeed), fpPos.z);
        }

        // Update third-person pivot
        if (thirdPersonCamera != null)
        {
            Vector3 tpPos = thirdPersonCamera.localPosition;
            thirdPersonCamera.localPosition = new Vector3(tpPos.x, Mathf.Lerp(tpPos.y, thirdPersonTargetY, Time.deltaTime * crouchTransitionSpeed), tpPos.z);
        }

        // Update the currently active camera height (used for headbob, etc)
        currentCameraHeight = isFirstPerson ? firstPersonCamera.localPosition.y : thirdPersonCamera.localPosition.y;
    }

    //======================HEADBOB===================
    void HandleHeadBob()
    {
        //currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * crouchTransitionSpeed);
        if (!isFirstPerson)
            return;
        bool isMoving = controller.velocity.magnitude > 0.1f && controller.isGrounded;

        if (canHeadbob && isMoving)
        {
            float speed = currentInput.move.magnitude * bobSpeedMultiplier;
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
    //=======================PERSPECTIVE=============================
    private void SetPerspective(bool firstPerson)
    {
        isFirstPerson = firstPerson;

        if (firstPersonCam != null)
            firstPersonCam.gameObject.SetActive(firstPerson);

        if (thirdPersonCam != null)
            thirdPersonCam.gameObject.SetActive(!firstPerson);

        cameraTransform = firstPerson ? firstPersonCamera : thirdPersonCamera;

        cameraPitch = 0f;
    }
    public void ChangePerspective()
    {
        if (!canChangePerspective)
            return;
        SetPerspective(!isFirstPerson);
    }

    //====================PUBLIC=====================
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

    //=========================SYSTEM===========================
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
