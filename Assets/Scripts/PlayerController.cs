using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float jumpDamping = 0.99f;
    
    [Header("Sprint & Stamina")]
    [SerializeField] private float maxStamina = 200f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;
    [SerializeField] private float staminaSpeedMultiplier = 1f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float crouchCameraHeight = 0.8f;
    [SerializeField] private float normalCameraHeight = 1.6f;
    
    [Header("Debug - Player State")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isJumping;
    [SerializeField] private float currentStamina;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 12f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private Camera playerCamera;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = 1;
    
    [Header("Physics")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float terminalVelocity = -30f;
    
    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool runInput;
    
    // Movement
    private Vector3 velocity;
    private Vector3 moveDirection;
    private bool isRunning;
    
    // Stamina
    private float timeSinceLastSprint;
    
    // Camera
    private float xRotation = 0f;
    private float yRotation = 0f;
    
    // Components
    private CharacterController controller;
    private Transform playerBody;
    private InventorySystem inventorySystem;
    
    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction scrollAction;
    private InputAction slot1Action;
    private InputAction slot2Action;
    private InputAction slot3Action;
    private InputAction slot4Action;
    private InputAction useItemAction;
    
    // Input values
    private Vector2 currentLookInput;
    
    private void Awake()
    {
        // Get components
        controller = GetComponent<CharacterController>();
        playerBody = transform;
        inventorySystem = GetComponent<InventorySystem>();
        
        // Initialize camera if not assigned
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (cameraContainer == null)
        {
            cameraContainer = new GameObject("CameraContainer").transform;
            cameraContainer.SetParent(transform);
            cameraContainer.localPosition = new Vector3(0, 1.6f, 0);
        }
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize stamina
        currentStamina = maxStamina;
        
        // Initialize CharacterController settings
        InitializeCharacterController();
    }
    
    private void Start()
    {
        // Setup input actions
        SetupInputActions();
        
        // Position camera at start
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(cameraContainer);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }
    
    private void SetupInputActions()
    {
        // Create input actions with simpler binding approach
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
            
        // Use separate actions for mouse X and Y to avoid composite issues
        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        runAction = new InputAction("Run", InputActionType.Button, "<Keyboard>/leftShift");
        crouchAction = new InputAction("Crouch", InputActionType.Button, "<Keyboard>/leftCtrl");
        scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll/y");
        slot1Action = new InputAction("Slot1", InputActionType.Button, "<Keyboard>/1");
        slot2Action = new InputAction("Slot2", InputActionType.Button, "<Keyboard>/2");
        slot3Action = new InputAction("Slot3", InputActionType.Button, "<Keyboard>/3");
        slot4Action = new InputAction("Slot4", InputActionType.Button, "<Keyboard>/4");
        useItemAction = new InputAction("UseItem", InputActionType.Button, "<Mouse>/leftButton");
        
        // Enable actions
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        runAction.Enable();
        crouchAction.Enable();
        scrollAction.Enable();
        slot1Action.Enable();
        slot2Action.Enable();
        slot3Action.Enable();
        slot4Action.Enable();
        useItemAction.Enable();
    }
    
    private void Update()
    {
        HandleInput();
        HandleMovement();
        HandleCamera();
        HandleGravity();
        UpdateDebugState();
    }
    
    private void HandleInput()
    {
        // Get input values
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        jumpInput = jumpAction.WasPressedThisFrame();
        runInput = runAction.IsPressed();
        isCrouching = crouchAction.IsPressed();
        
        // Handle inventory input
        HandleInventoryInput();
        
        // Handle cursor lock toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    private void HandleInventoryInput()
    {
        if (inventorySystem == null) return;
        
        // Handle scroll wheel
        float scrollInput = scrollAction.ReadValue<float>();
        if (scrollInput > 0)
        {
            inventorySystem.PreviousSlot();
        }
        else if (scrollInput < 0)
        {
            inventorySystem.NextSlot();
        }
        
        // Handle number keys
        if (slot1Action.WasPressedThisFrame())
            inventorySystem.SelectSlot(0);
        if (slot2Action.WasPressedThisFrame())
            inventorySystem.SelectSlot(1);
        if (slot3Action.WasPressedThisFrame())
            inventorySystem.SelectSlot(2);
        if (slot4Action.WasPressedThisFrame())
            inventorySystem.SelectSlot(3);
        
        // Handle item use
        if (useItemAction.WasPressedThisFrame())
        {
            inventorySystem.UseItem(inventorySystem.SelectedSlot);
        }
    }
    
    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // Handle sprinting and stamina
        HandleSprint();
        
        // Determine movement state - allow sprinting while jumping
        isRunning = isSprinting && moveInput.magnitude > 0.1f && !isCrouching;
        
        // Calculate target speed based on state
        float targetSpeed;
        if (isCrouching)
            targetSpeed = crouchSpeed;
        else if (isRunning)
            targetSpeed = runSpeed;
        else
            targetSpeed = walkSpeed;
            
        targetSpeed *= moveInput.magnitude;
        
        // Apply air control
        float controlMultiplier = isGrounded ? 1f : airControl;
        
        // Calculate movement
        Vector3 targetVelocity = moveDirection * targetSpeed;
        
        // Apply acceleration/deceleration
        if (moveInput.magnitude > 0.1f)
        {
            velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, acceleration * controlMultiplier * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, acceleration * controlMultiplier * Time.deltaTime);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, deceleration * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0, deceleration * Time.deltaTime);
        }
        
        // Handle jumping
        if (jumpInput && isGrounded)
        {
            velocity.y = jumpForce;
        }
        
        // Apply jump damping when not grounded (reduces floatiness)
        if (!isGrounded && velocity.y > 0)
        {
            velocity.y *= jumpDamping;
        }
        
        // Handle crouch height
        HandleCrouch();
        
        // Apply movement
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void HandleSprint()
    {
        // Check if player wants to sprint and has stamina
        bool wantsToSprint = runInput && moveInput.magnitude > 0.1f && !isCrouching;
        bool canSprint = isGrounded || (isSprinting && currentStamina > 0); // Allow continuing sprint in air if already sprinting
        
        if (wantsToSprint && canSprint && currentStamina > 0)
        {
            isSprinting = true;
            // Drain stamina with speed multiplier
            currentStamina -= staminaDrainRate * staminaSpeedMultiplier * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            timeSinceLastSprint = 0f;
        }
        else
        {
            // Only stop sprinting if we're grounded or out of stamina
            if (isGrounded || currentStamina <= 0)
            {
                isSprinting = false;
            }
            
            timeSinceLastSprint += Time.deltaTime;
            
            // Regenerate stamina when not sprinting with speed multiplier
            if (timeSinceLastSprint >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * staminaSpeedMultiplier * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }
    
    private void InitializeCharacterController()
    {
        // Set initial CharacterController settings
        controller.height = normalHeight;
        controller.center = new Vector3(0, normalHeight * 0.5f, 0);
        controller.radius = 0.5f; // Standard character radius
    }
    
    private void HandleCrouch()
    {
        // Smoothly transition between crouch and normal height
        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        float currentHeight = controller.height;
        
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            
            // Set center to match the current lerped height, not the target
            controller.center = new Vector3(0, controller.height * 0.5f, 0);
        }
        else
        {
            // When transition is complete, ensure center is correct
            controller.center = new Vector3(0, controller.height * 0.5f, 0);
        }
        
        // Adjust camera container height for proper crouch view
        float targetCameraHeight = isCrouching ? crouchCameraHeight : normalCameraHeight;
        Vector3 targetCameraPosition = new Vector3(0, targetCameraHeight, 0);
        cameraContainer.localPosition = Vector3.Lerp(cameraContainer.localPosition, targetCameraPosition, crouchTransitionSpeed * Time.deltaTime);
    }
    
    private void UpdateDebugState()
    {
        // Update debug variables for inspector
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        isWalking = moveInput.magnitude > 0.1f && isGrounded && !isRunning && !isCrouching;
        isJumping = !isGrounded && velocity.y > 0;
    }
    
    private void HandleCamera()
    {
        // Calculate rotation directly from input (no smoothing for better responsiveness)
        yRotation += lookInput.x * mouseSensitivity * Time.deltaTime;
        xRotation -= lookInput.y * mouseSensitivity * Time.deltaTime;
        
        // Clamp vertical rotation
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        // Apply rotations
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraContainer.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    private void HandleGravity()
    {
        // Apply gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        // Clamp terminal velocity
        velocity.y = Mathf.Max(velocity.y, terminalVelocity);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw ground check sphere
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    private void OnDestroy()
    {
        // Disable input actions
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        runAction?.Disable();
        crouchAction?.Disable();
        scrollAction?.Disable();
        slot1Action?.Disable();
        slot2Action?.Disable();
        slot3Action?.Disable();
        slot4Action?.Disable();
        useItemAction?.Disable();
    }
    
    // Public methods for external access
    public bool IsGrounded => isGrounded;
    public bool IsRunning => isRunning;
    public bool IsCrouching => isCrouching;
    public bool IsSprinting => isSprinting;
    public float CurrentStamina => currentStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    public Vector3 Velocity => velocity;
    public float Speed => new Vector3(velocity.x, 0, velocity.z).magnitude;
    
    // Method to set camera sensitivity
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    // Method to set movement speeds
    public void SetMovementSpeeds(float walk, float run)
    {
        walkSpeed = walk;
        runSpeed = run;
    }
    
    // Method to add external force (for things like explosions, knockback, etc.)
    public void AddForce(Vector3 force)
    {
        velocity += force;
    }
    
    // Method to set velocity directly
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
}
