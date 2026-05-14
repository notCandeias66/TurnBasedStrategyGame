using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    // Fields to store player input, character controller, and animator
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    // Hashes for optimization
    int isWalkingHash;
    int isRunningHash;

    // Input storage
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 appliedMovement;
    Vector3 cameraRelativeMovement;
    bool isMovementPressed;
    bool isRunPressed;

    // Constants
    float rotationFactorPerFrame = 15.0f;
    float runMultiplier = 3.0f;
    int zero = 0;

    // Gravity
    float gravity = -9.8f;
    float groundedGravity = -.05f;

    // Jump
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 2.0f;
    float maxJumpTime = 1.0f;
    bool isJumping = false;
    int isJumpingHash;
    bool isJumpAnimating = false;

    public AudioSource audioRun;
    public AudioSource audioWalk;
    public AudioSource audioJump;

    void Awake()
    {
        // Initialize player input, character controller and animator
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Convert animation state names to hashes for performance optimization
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        // Bind input actions to corresponding methods
        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        setupJumpVariables();
    }

    // Sets up jump-related variables
    void setupJumpVariables()
    {
        // Calculate time to reach the apex of the jump
        float timeToApex = maxJumpTime / 2;
        // Set gravity based on maximum jump height and time
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        // Calculate initial jump velocity
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    // Handles jump input
    void onJump(InputAction.CallbackContext context)
    {
        // Read the jump button state
        isJumpPressed = context.ReadValueAsButton();
    }

    // Handles run input
    void onRun(InputAction.CallbackContext context)
    {
        // Read the run button state
        isRunPressed = context.ReadValueAsButton();
    }

    // Handles movement input
    void onMovementInput(InputAction.CallbackContext context)
    {
        // Read the movement input as a Vector2
        currentMovementInput = context.ReadValue<Vector2>();
        // Update movement vectors based on input
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        // Check if movement input is pressed
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    // Handles jump mechanics
    void handleJump()
    {
        // Get the current animation states
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // Check if the character should jump
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            // Set jumping animation state
            animator.SetBool(isJumpingHash, true);

            // Stop walking or running audio if playing
            if (isWalking) audioWalk.Stop();
            if (isRunning) audioRun.Stop();

            // Play jump audio
            audioJump.Play();

            // Update jump states and velocities
            isJumpAnimating = true;
            isJumping = true;
            currentMovement.y = initialJumpVelocity;
            appliedMovement.y = initialJumpVelocity;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            // Reset jumping state if the character is grounded
            isJumping = false;
        }
    }

    // Handles character rotation
    void handleRotation()
    {
        // Determine the position to look at based on movement direction
        Vector3 positionToLookAt;
        positionToLookAt.x = cameraRelativeMovement.x;
        positionToLookAt.y = zero;
        positionToLookAt.z = cameraRelativeMovement.z;

        // Get the current rotation
        Quaternion currentRotation = transform.rotation;

        // Rotate the character smoothly towards the movement direction
        if (isMovementPressed && positionToLookAt.magnitude >= 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    // Handles animation states
    void handleAnimation()
    {
        // Get the current animation states
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // Handle walking animation state and audio
        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
            audioWalk.Play();
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
            audioWalk.Stop();
        }

        // Handle running animation state and audio
        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
            audioWalk.Stop();
            audioRun.Play();
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
            audioRun.Stop();
            if (isWalking) audioWalk.Play();
        }
    }

    // Handles gravity effects on the character
    void handleGravity()
    {
        // Get the current animation states
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        // Determine if the character is falling
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;
        if (characterController.isGrounded)
        {
            // Handle grounded state
            if (isJumpAnimating)
            {
                animator.SetBool(isJumpingHash, false);
                if (isWalking) audioWalk.Play();
                if (isRunning) audioWalk.Stop();
                audioRun.Play();
                isJumpAnimating = false;
            }
            currentMovement.y = groundedGravity;
            appliedMovement.y = groundedGravity;
        }
        else if (isFalling)
        {
            // Handle falling state
            float previousYVelocity = currentMovement.y;
            currentMovement.y += gravity * fallMultiplier * Time.deltaTime;
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * .5f, -20.0f);
        }
        else
        {
            // Handle airborne state
            float previousYVelocity = currentMovement.y;
            currentMovement.y += gravity * Time.deltaTime;
            appliedMovement.y = (previousYVelocity + currentMovement.y) * .5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle character rotation and animations
        handleRotation();
        handleAnimation();

        // Apply movement based on input states
        if (isRunPressed)
        {
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        }
        else
        {
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }

        // Convert movement to camera-relative space and move the character
        cameraRelativeMovement = ConvertToCameraSpace(appliedMovement);
        characterController.Move(cameraRelativeMovement * Time.deltaTime);

        // Handle gravity and jump mechanics
        handleGravity();
        handleJump();
    }

    // Converts a movement vector to be relative to the camera's orientation
    Vector3 ConvertToCameraSpace (Vector3 vectorToRotate)
    {
        float currentYValue = vectorToRotate.y;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraForwardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }

    // Enable player input controls when the script is enabled
    void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    // Disable player input controls when the script is disabled
    void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
