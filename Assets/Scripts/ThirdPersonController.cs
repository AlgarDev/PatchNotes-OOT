using UnityEngine;
public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController Instance;

    [SerializeField] public Transform cameraTransform;

    [Header("Toggles")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;
    [SerializeField] private bool canJump = false;
    [SerializeField] private bool canInteract = false;

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

    [field: Header("Interact")]
    [SerializeField]
    private Transform interactorSource;
    [field: SerializeField]
    public Transform grabPivot { private set; get; }
    [SerializeField]
    private float interactRadius;
    [SerializeField]
    private float interactRange;
    [SerializeField]
    GameObject heldObject;
    [field: SerializeField]
    public bool isHolding { private set; get; }

    [Header("Interact Hold Logic")]
    [SerializeField] private float throwHoldTime = 1f;

    private float interactHoldTimer;
    private bool interactConsumed;
    [SerializeField]
    private Collider[] interactableColliders;

    [Header("Animation")]
    [SerializeField]
    private Animator animationController;

    [field: Header("Input Recording/Clones")]
    [field: SerializeField] public bool IsPlayerControlled { private set; get; } = true;
    private PlayerInputFrame currentInput;
    private PlayerInputFrame previousInput;

    public CharacterController controller;
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
        animationController = GetComponentInChildren<Animator>();

        if (IsPlayerControlled)
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

        HandleEdgeInputs(); //jump, interact
        HandleMovement(currentInput.cameraForward);
        HandleGravity();
        HandleMeshTilt(currentInput.cameraForward);
        AnimationHandler();

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
        if (canInteract)
            HandleInteractInput();
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

    private void HandleMeshTilt(float cameraForward)
    {
        if (characterMesh == null) return;

        // Calculate tilt based on turning
        Vector3 input = new Vector3(currentInput.move.x, 0, currentInput.move.y).normalized;

        if (input.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cameraForward;
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
            Debug.Log(controller.isGrounded);
            float requiredVelocity = (JumpHeight - (0.5f * JumpGravity * JumpTime * JumpTime)) / JumpTime;
            velocity.y = requiredVelocity;

            currentGravity = JumpGravity;
            isJumping = true;
            jumpTimer = 0f;

            animationController.SetTrigger("Jump");
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
    private void HandleInteractInput()
    {
        // Button pressed
        if (currentInput.interactPressed)
        {
            interactHoldTimer = 0f;
            interactConsumed = false;
        }

        // Button held
        if (currentInput.interactHeld)
        {
            interactHoldTimer += Time.deltaTime;

            if (!interactConsumed && isHolding && interactHoldTimer >= throwHoldTime)
            {
                print("throw"); 
                ThrowHeldObject();
                interactConsumed = true;
            }
        }

        if (!currentInput.interactHeld && previousInput.interactHeld)
        {
            if (!interactConsumed)
            {
                Interact();
            }

            interactHoldTimer = 0f;
            interactConsumed = false;
        }
    }

    private void Interact()
    {
        print("interacted");
        if (isHolding && heldObject != null)
        {
            if (heldObject.TryGetComponent(out IGrabbable grabbed))
            {
                grabbed.Drop(this);
            }
            isHolding = false;
            heldObject = null;
            animationController.SetTrigger("Drop"); //?
            return;
        }

        interactableColliders = Physics.OverlapBox(interactorSource.transform.position + interactorSource.transform.forward * interactRange,
new Vector3(interactRadius, interactRadius, interactRange), interactorSource.transform.rotation, ~0, QueryTriggerInteraction.Collide);

        foreach (var item in interactableColliders)
        {
            if (!isHolding && item.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                //print("interacted");
                interactObj.Interact(this);
                if (item.TryGetComponent(out IGrabbable grabbable))
                {
                    isHolding = true;
                    heldObject = item.gameObject;
                    animationController.SetTrigger("GrabNormal");
                    break;
                }
            }
        }

    }
    private void ThrowHeldObject()
    {
        if (heldObject == null)
            return;

        if (heldObject.TryGetComponent(out IGrabbable grabbed))
        {
            grabbed.Throw(this);
        }

        isHolding = false;
        heldObject = null;
    }


    //use animationController.SetTrigger("GrabToThrow") when getting ready to throw
    //use animationController.SetTrigger("Throw") when throwing

    private void AnimationHandler()
    {
        animationController.SetBool("isGrounded", controller.isGrounded);
        animationController.SetFloat("Movement", currentSpeed);

    }
    private void OnDrawGizmos()
    {
        if (interactorSource == null)
            return;

        Vector3 halfExtents = new Vector3(interactRadius, interactRadius, interactRange);

        Vector3 center =
            interactorSource.transform.position +
            interactorSource.transform.forward * interactRange;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(
            center,
            interactorSource.transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(interactorSource.transform.position, interactorSource.transform.forward * interactRange);

    }
    //=========================SYSTEM===========================
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}