using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController Instance;

    [SerializeField] public Transform cameraTransform;

    [Header("Toggles")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;
    [SerializeField] private bool canJump = false;

    [field: Header("Movement")]
    [field: SerializeField] public float topWalkSpeed { private set; get; } = 2f;
    [field: SerializeField] public float SmoothTime { private set; get; } = 0.1f;

    [Header("Acceleration & Turning")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float turnSpeed = 360f; // Degrees per second
    [SerializeField] private Transform characterMesh; // Reference to the child mesh transform
    [SerializeField] private float meshTiltAmount = 15f; // Max tilt angle in degrees
    [SerializeField] private float meshTiltSpeed = 5f; // How fast mesh tilts

    private float currentSpeed = 0f;
    private float meshTiltVelocity = 0f;
    private float currentMeshTilt = 0f;

    [field: Header("Jump")]
    [field: SerializeField] public float JumpGravity { private set; get; } = -9.81f;
    [field: SerializeField] public float Gravity { private set; get; } = -9.81f;
    [field: SerializeField] public float JumpHeight { private set; get; } = 1.5f;
    [field: SerializeField] public float JumpTime { private set; get; } = 1.5f;
    private float currentGravity;
    private bool isJumping;
    private float jumpTimer;

    [Header("Input Recording/Clones")]
    [SerializeField] private bool isPlayerControlled = true;
    private PlayerInputFrame currentInput;
    private PlayerInputFrame previousInput;
    [SerializeField] private CloneInputRecorder inputRecorder;

    private float currentTargetHeight;

    private CharacterController controller;
    private Vector3 velocity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        previousInput = currentInput = new PlayerInputFrame();
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

        currentGravity = Gravity;
        isJumping = false;
        jumpTimer = 0f;

        // Find character mesh if not assigned
        if (characterMesh == null)
        {
            // Look for a child transform that might be the character mesh
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Mesh") || child.name.Contains("Character") || child.name.Contains("Model"))
                {
                    characterMesh = child;
                    break;
                }
            }

            // If still not found, use the first child
            if (characterMesh == null && transform.childCount > 0)
            {
                characterMesh = transform.GetChild(0);
            }
        }
    }

    private void Update()
    {
        if (!GameManager.isGameRunning)
            return;

        HandleEdgeInputs();
        HandleMovement(currentInput.cameraForward);
        HandleGravity();
        HandleMeshTilt();

            //if (isPlayerControlled && inputRecorder != null)
            //{
            //    inputRecorder.Record(currentInput);
            //}

        previousInput = currentInput;
    }


    public void SetInput(PlayerInputFrame input)
    {
        currentInput = input;
    }

    private void HandleEdgeInputs()
    {
        if (canJump && currentInput.jump && !previousInput.jump)
            Jump();
    }

    void HandleMovement(float cameraForward)
    {
        if (!canMove)
        {
            // Decelerate when can't move
            if (currentSpeed > 0f)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);
            }
            return;
        }

        Vector3 input = new Vector3(currentInput.move.x, 0, currentInput.move.y).normalized;
        Vector3 direction = new Vector3(input.x, 0f, input.z).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraForward;

            // Smooth rotation using turn speed
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);
            float maxRotation = turnSpeed * Time.deltaTime;
            float rotationAmount = Mathf.Clamp(angleDifference, -maxRotation, maxRotation);
            float angle = transform.eulerAngles.y + rotationAmount;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Accelerate
            currentSpeed = Mathf.Min(topWalkSpeed, currentSpeed + acceleration * Time.deltaTime);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            // Decelerate when no input
            if (currentSpeed > 0f)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);

                // Apply residual momentum in current direction
                if (currentSpeed > 0f)
                {
                    Vector3 moveDir = transform.forward;
                    controller.Move(moveDir * currentSpeed * Time.deltaTime);
                }
            }
        }
    }

    private void HandleMeshTilt()
    {
        if (characterMesh == null) return;

        // Calculate tilt based on turning
        Vector3 input = new Vector3(currentInput.move.x, 0, currentInput.move.y).normalized;

        if (input.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);

            // Normalize angle difference to -1 to 1 range
            float normalizedDifference = Mathf.Clamp(angleDifference / 180f, -1f, 1f);

            // Target tilt is opposite of turn direction for visual weight
            float targetTilt = -normalizedDifference * meshTiltAmount;
            currentMeshTilt = Mathf.SmoothDamp(currentMeshTilt, targetTilt, ref meshTiltVelocity, 1f / meshTiltSpeed);
        }
        else
        {
            // Return to neutral when not moving
            currentMeshTilt = Mathf.SmoothDamp(currentMeshTilt, 0f, ref meshTiltVelocity, 0.2f);
        }

        // Apply tilt to character mesh (Z-axis rotation)
        characterMesh.localRotation = Quaternion.Euler(0f, 0f, currentMeshTilt);
    }

    private void HandleGravity()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= JumpTime)
            {
                isJumping = false;
                currentGravity = Gravity;
                jumpTimer = 0f;
            }
        }
        velocity.y += currentGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        Debug.Log(controller.isGrounded);
        if (!canJump)
            return;

        if (controller.isGrounded)
        {
            float requiredVelocity = (JumpHeight - (0.5f * JumpGravity * JumpTime * JumpTime)) / JumpTime;
            velocity.y = requiredVelocity;

            currentGravity = JumpGravity;
            isJumping = true;
            jumpTimer = 0f;
        }
    }

    public void ChangeCanMove(bool value)
    {
        canMove = value;
        if (!value)
        {
            currentSpeed = 0f;
        }
    }

    //=========================SYSTEM===========================
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}