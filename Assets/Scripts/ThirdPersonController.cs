using Cinemachine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController Instance;

    [SerializeField] public Transform cameraTransform;
    [SerializeField] public CinemachineFreeLook cinemachineFreeLook;

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
    [field: SerializeField]
    public Transform throwPivot { private set; get; }
    [SerializeField]
    private float interactRadius;
    [SerializeField]
    private float interactRange;
    [SerializeField]
    GameObject heldObject;
    [field: SerializeField]
    public bool isHolding { private set; get; }
    [field: SerializeField]
    public bool isReadyToThrow { private set; get; }

    [Header("Interact Hold Logic")]
    [SerializeField] private float throwHoldTime = 1f;

    private float interactHoldTimer;
    private bool interactConsumed;
    [SerializeField]
    private Collider[] interactableColliders;

    [Header("Animation")]
    [SerializeField]
    private Animator animationController;
    [SerializeField] float minTimeToLongIdle = 1, maxTimeToLongIdle = 3;
    Coroutine longIdleCoroutine;

    [field: Header("Input Recording/Clones")]
    [field: SerializeField] public bool IsPlayerControlled { private set; get; } = true;
    private PlayerInputFrame currentInput;
    private PlayerInputFrame previousInput;

    private CloneSpawningPlatform activeSpawner;
    public CharacterController controller;
    private Vector3 velocity;

    [field: Header("Hourglass")]
    [field: SerializeField] public InteractableBox Hourglass { get; private set; }
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float distanceToWall = 3;
    public bool controllingHourglass;
    private Vector3 storedPlayerPosition;
    private Quaternion storedPlayerRotation;
    public bool isTooClose;

    private MovingPlatform currentPlatform;
    private Vector3 lastPlatformGhostPosition;

    private float verticalVelocity;

    [field: Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip grabSFX;
    [SerializeField] private AudioClip dropSFX;
    [SerializeField] private AudioClip throwSFX;
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private AudioClip backToCheckpointSFX;
    [SerializeField] private AudioClip dieAndStopRecordingSFX;
    [SerializeField] private AudioClip respawnSFX;

    private bool isWalking;

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

        longIdleCoroutine = null;
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

        animationController.SetBool("IsHolding", isHolding);
        animationController.SetBool("IsReadyToThrow", isReadyToThrow);

        previousInput = currentInput;

        if (controller.velocity.magnitude < 0.1)
        {
            //Debug.Log("Not moving");
            if (longIdleCoroutine == null)
            {
                longIdleCoroutine = StartCoroutine(LongIdle());
            }
        }
        else
        {
            //Debug.Log("moving");
            if (longIdleCoroutine != null)
            {
                StopCoroutine(longIdleCoroutine);
                longIdleCoroutine = null;
                animationController.SetBool("LongIdleA", false);
                animationController.SetBool("LongIdleB", false);
            }
        }
        if (Physics.Raycast(transform.position, transform.forward, distanceToWall, wallLayer))
        {
            isTooClose = true;
        }
        else
            isTooClose = false;

    }


    public void SetInput(PlayerInputFrame input)
    {
        currentInput = input;
    }

    private void HandleEdgeInputs()
    {
        if (currentInput.controllingHourglass && !previousInput.controllingHourglass)
        {
            //ToggleHourglassControl();
        }
        if (canJump && currentInput.jump && !previousInput.jump)
            Jump();
        if (canInteract)
            HandleInteractInput();
    }
    void HandleMovement(float cameraForward)
    {
        if (!canMove)
        {
            if (currentSpeed > 0f)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);
            }
            return;
        }

        Vector3 input = new Vector3(currentInput.move.x, 0, currentInput.move.y).normalized;
        Vector3 direction = new Vector3(input.x, 0f, input.z).normalized;

        Vector3 moveDirection = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraForward;

            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);
            float maxRotation = turnSpeed * Time.deltaTime;
            float rotationAmount = Mathf.Clamp(angleDifference, -maxRotation, maxRotation);
            float angle = transform.eulerAngles.y + rotationAmount;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            currentSpeed = Mathf.Min(topWalkSpeed, currentSpeed + acceleration * Time.deltaTime);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            if (currentSpeed > 0f)
            {
                currentSpeed = Mathf.Max(0f, currentSpeed - deceleration * Time.deltaTime);
                moveDirection = transform.forward;
            }
        }

        Vector3 finalMove = moveDirection.normalized * currentSpeed;
        finalMove.y = verticalVelocity;

        if (platformGhost != null)
        {
            Vector3 platformDelta = platformGhost.position - lastPlatformGhostPosition;
            finalMove += platformDelta / Time.deltaTime;
        }

        controller.Move(finalMove * Time.deltaTime);

        if (platformGhost != null)
        {
            lastPlatformGhostPosition = platformGhost.position;
        }

        bool shouldWalk = canMove && controller.isGrounded && currentSpeed > 0.1f && currentInput.move.magnitude > 0.1f;

        if (shouldWalk && !isWalking)
        {
            isWalking = true;
            StartWalkSFX();
        }
        else if (!shouldWalk && isWalking)
        {
            isWalking = false;
            StopWalkSFX();
        }

    }

    public void StopMovement()
    {
        velocity = Vector3.zero;
        currentSpeed = 0f;
        verticalVelocity = 0f;
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
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small downward bias to stay grounded
        }

        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= JumpTime)
            {
                isJumping = false;
                currentGravity = Gravity;
            }
        }
        verticalVelocity += currentGravity * Time.deltaTime;
    }
    private void Jump()
    {
        if (!canJump || !controller.isGrounded)
            return;

        verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * JumpGravity);
        currentGravity = JumpGravity;
        isJumping = true;
        jumpTimer = 0f;

        animationController.SetTrigger("Jump");

        StopWalkSFX();
        isWalking = false;

        audioSource.PlayOneShot(jumpSFX);
    }


    public void ChangeCanMove(bool value)
    {
        canMove = value;
        if (!value)
        {
            currentSpeed = 0f;
            isWalking = false;
            StopWalkSFX();
        }

    }
    private void HandleInteractInput()
    {
        if (isHolding && heldObject != null)
        {
            if (currentInput.interactPressed)
            {
                interactHoldTimer = 0f;
                interactConsumed = false;
            }

            // Button held
            if (currentInput.interactHeld)
            {

                if (!interactConsumed && isHolding && interactHoldTimer >= throwHoldTime)
                {
                    print("throw");
                    animationController.SetTrigger("Throw");
                    //ThrowHeldObject();
                    interactConsumed = true;
                }
                else
                {
                    if (interactHoldTimer >= throwHoldTime / 3)
                    {
                        isReadyToThrow = true;
                        heldObject.transform.SetParent(throwPivot);
                        heldObject.transform.localPosition = Vector3.zero;
                    }
                    print("holding");
                    interactHoldTimer += Time.deltaTime;

                }
            }
        }

        if (!currentInput.interactHeld && previousInput.interactHeld)
        {
            isReadyToThrow = false;
            if (isHolding && heldObject != null)
            {
                heldObject.transform.SetParent(grabPivot);
                heldObject.transform.localPosition = Vector3.zero;
            }
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
        if (isHolding && heldObject != null)
        {
            if (heldObject.TryGetComponent(out IGrabbable grabbed))
            {
                grabbed.Drop(this);
            }
            isHolding = false;
            heldObject = null;
            print("Drop hourglass");
            audioSource.PlayOneShot(dropSFX);

        }
        else
        {
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
                        print("Grabbed hourglass");
                        isHolding = true;
                        heldObject = item.gameObject;
                        animationController.SetTrigger("GrabNormal");
                        audioSource.PlayOneShot(grabSFX);

                        break;
                    }
                }
            }
        }



    }
    public void ThrowHeldObject()
    {
        if (heldObject == null)
            return;

        if (heldObject.TryGetComponent(out IGrabbable grabbed))
        {
            grabbed.Throw(this);
        }

        isHolding = false;
        heldObject = null;
        isReadyToThrow = false;
        audioSource.PlayOneShot(throwSFX);
    }

    public void ResetGrab()
    {
        //Debug.Log("Reset grab");
        isHolding = false;
        heldObject = null;
        isReadyToThrow = false;
    }

    private void AnimationHandler()
    {
        animationController.SetBool("isGrounded", controller.isGrounded);
        animationController.SetFloat("Movement", currentSpeed);

    }

    private IEnumerator LongIdle()
    {
        //Debug.Log("Started");
        float timeToWait = Random.Range(minTimeToLongIdle, maxTimeToLongIdle);

        //Debug.Log("timeToWait " + timeToWait);
        yield return new WaitForSeconds(timeToWait);

        int randomAnimation = Random.Range(0, 2);
        //Debug.Log("randomAnimation " + randomAnimation);
        if (randomAnimation == 0)
        {
            //Debug.Log("A");
            animationController.SetBool("LongIdleA", true);
        }
        else
        {
            //Debug.Log("B");
            animationController.SetBool("LongIdleB", true);
        }

    }

    public void ResetLongAnimation()
    {
        animationController.SetBool("LongIdleA", false);
        animationController.SetBool("LongIdleB", false);
        longIdleCoroutine = null;
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
    //=======================HOURGLASS============================
    public void ToggleHourglassControl()
    {
        controllingHourglass = !controllingHourglass;
        currentInput.controllingHourglass = controllingHourglass;

        if (controllingHourglass)
        {
            // store player state
            storedPlayerPosition = transform.position;
            storedPlayerRotation = transform.rotation;

            // swap positions
            controller.enabled = false;
            transform.position = Hourglass.transform.position;
            transform.rotation = Hourglass.transform.rotation;
            controller.enabled = true;
            Hourglass.transform.position = storedPlayerPosition;
            Hourglass.transform.rotation = storedPlayerRotation;

            // disable player movement, enable hourglass physics
            ChangeCanMove(false);
            Hourglass.EnableControl(true);
            if (IsPlayerControlled)
            {
                cinemachineFreeLook.Follow = Hourglass.transform;
                cinemachineFreeLook.LookAt = Hourglass.transform;
                if (activeSpawner != null)
                {
                    activeSpawner.PauseTimer(controllingHourglass);
                }
            }
        }
        else
        {
            // swap back
            Vector3 hgPos = Hourglass.transform.position;
            Quaternion hgRot = Hourglass.transform.rotation;

            Hourglass.transform.position = transform.position;
            Hourglass.transform.rotation = transform.rotation;

            controller.enabled = false;
            transform.position = hgPos;
            transform.rotation = hgRot;
            controller.enabled = true;

            ChangeCanMove(true);
            Hourglass.EnableControl(false);

            if (IsPlayerControlled)
            {
                cinemachineFreeLook.Follow = transform;
                cinemachineFreeLook.LookAt = transform;
                if (activeSpawner != null)
                {
                    activeSpawner.PauseTimer(controllingHourglass);
                }
            }
        }
    }

    public void SetHourglass(GameObject hourglass)
    {
        Hourglass = hourglass.GetComponent<InteractableBox>();
        //print(Hourglass.transform.position);
    }
    //=====================PLATFORMS=============================
    [SerializeField] private Transform platformGhost;

    public void SetPlatformGhost(Transform ghost)
    {
        platformGhost = ghost;
        lastPlatformGhostPosition = ghost.position;
    }


    public void ClearPlatformGhost(Transform ghost)
    {
        if (platformGhost == ghost)
            platformGhost = null;
    }

    //==========================CHECKPOINT=====================
    private Transform checkpoint;
    public void SetCheckpoint(Transform Tr)
    {
        checkpoint = Tr;
    }
    public void RespawnAtCheckpoint()
    {
        print("huiwqiehbiwq");
        if (activeSpawner != null)
        {
            activeSpawner.StopEarly();
            controller.enabled = false;
            if (checkpoint != null)
                transform.position = checkpoint.position;
            controller.enabled = true;
            GetComponent<PlayerColorManager>().ChangeColor(ColorRef.Green);
            GetComponent<PlayerColorManager>().DisableHourglassSand();
            audioSource.PlayOneShot(respawnSFX);
        }
        else
        {
            controller.enabled = false;
            if (checkpoint != null)
                transform.position = checkpoint.position;
            controller.enabled = true;
            audioSource.PlayOneShot(respawnSFX);
        }
    }
    //========================SOUNDS============================

    private void StartWalkSFX()
    {
        if (audioSource.isPlaying && audioSource.clip == walkSFX)
            return;

        audioSource.clip = walkSFX;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopWalkSFX()
    {
        if (audioSource.clip == walkSFX)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    //==========================SETTERS========================
    public void SetActiveSpawner(CloneSpawningPlatform spawner)
    {
        activeSpawner = spawner;
    }


    //=========================SYSTEM===========================
    public void CleanMyShit()
    {
        if (activeSpawner != null) activeSpawner.GoToHell();
        GetComponent<PlayerColorManager>().ChangeColor(ColorRef.Green);
        GetComponent<PlayerColorManager>().DisableHourglassSand();

    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}