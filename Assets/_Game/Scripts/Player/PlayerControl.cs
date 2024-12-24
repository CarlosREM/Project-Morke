using System;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{
    
    private Rewired.Player _input;
    private Rigidbody2D _rb;
    private CharacterHealth _health;

    [Header("Player Movement")] 
    [SerializeField] private float moveSpeed;
    [SerializeField, Range(0, 1)] private float moveSpeedCrouchMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private float airMaxVelocity;
    [SerializeField] private float airMinVelocity;
    
    public bool IsJumping { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsFlashlightOn { get; private set; }
    
    [Header("Ground check")]
    [SerializeField] private LayerMask groundCheckLayer;
    [SerializeField, Tooltip("Changing Z does nothing")] private Bounds groundCheckBounds;
    public bool IsGrounded { get; private set; }

    [Header("Additional Components")]
    [SerializeField] private Collider2D normalCollider;
    [SerializeField] private Collider2D crouchCollider;
    [SerializeField] private Transform flashlightPivot;
    [SerializeField] private GameObject flashlightObject;
    [SerializeField] private Collider2D interactionCollider;
    public float VelocityY { get; private set; }

    public bool IsFacingRight { get; private set; } = true;
    
    #region Initialization
    
    void Awake()
    {
        _input = ReInput.players.SystemPlayer;
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        Assert.IsNotNull(_input, "Player input is not initialized");
        
        //set input callbacks
        _input.AddInputEventDelegate(InputMove, UpdateLoopType.Update, InputActionEventType.Update, "GP_Move");
        _input.AddInputEventDelegate(InputCrouch, UpdateLoopType.Update, InputActionEventType.Update, "GP_Crouch");
        _input.AddInputEventDelegate(InputJump, UpdateLoopType.Update, InputActionEventType.Update, "GP_Jump");
        _input.AddInputEventDelegate(InputFlashlight, UpdateLoopType.Update, InputActionEventType.Update, "GP_Flashlight");
        _input.AddInputEventDelegate(InputInteract, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Interact");

        _health.OnDeath += OnDeath;
        
        Debug.Log("[PlayerInput] <color=green>Ready</color>");
    }

    private void OnDisable()
    {
        Assert.IsNotNull(_input, "Player input is not initialized");

        // remove input callbacks
        _input.RemoveInputEventDelegate(InputMove, UpdateLoopType.Update, InputActionEventType.Update, "GP_Move");
        _input.RemoveInputEventDelegate(InputCrouch, UpdateLoopType.Update, InputActionEventType.Update, "GP_Crouch");
        _input.RemoveInputEventDelegate(InputJump, UpdateLoopType.Update, InputActionEventType.Update, "GP_Jump");
        _input.RemoveInputEventDelegate(InputFlashlight, UpdateLoopType.Update, InputActionEventType.Update, "GP_Flashlight");
        _input.RemoveInputEventDelegate(InputInteract, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Interact");
        
        _health.OnDeath -= OnDeath;
        
        Debug.Log("[PlayerInput] <color=red>Disabled</color>");
    }

    private void OnDeath()
    {
        this.enabled = false;
    }
    
    #endregion
    
    #region Update Loop

    private void OnDrawGizmos()
    {
        // ground check
        Vector2 groundCheckOrigin = transform.position + groundCheckBounds.center;

        Gizmos.color = (IsGrounded) ? Color.cyan : Color.red;
        Gizmos.DrawWireCube(groundCheckOrigin, groundCheckBounds.size);
    }

    private void Update()
    {
        InputFlashlightMove();
    }
    
    private void FixedUpdate()
    {
        IsGrounded = GroundCheck();

        if (!IsGrounded && IsCrouching)
        {
            IsCrouching = false;
        }

        VelocityY = Mathf.Clamp(_rb.linearVelocityY, airMinVelocity, airMaxVelocity);
        _rb.linearVelocityY = VelocityY;
    }

    private bool GroundCheck()
    {
        Vector2 groundCheckOrigin = _rb.position + (Vector2) groundCheckBounds.center;
        return Physics2D.BoxCast(groundCheckOrigin, groundCheckBounds.size, 0, Vector2.down, 0, groundCheckLayer);
    }

    #endregion
    
    #region Input Events
    
    [Header("Input parameters")]
    [SerializeField] private float jumpInputCacheDuration;
    [SerializeField] private bool crouchInputToggle;
    [SerializeField] private bool flashlightInputToggle;

    public float MoveInput { get; private set; }

    private void InputMove(InputActionEventData inputData)
    {
        float value = inputData.GetAxis();

        float crouchMultiplier = (IsCrouching) ? moveSpeedCrouchMultiplier : 1; 
        _rb.linearVelocityX = value * moveSpeed * crouchMultiplier;
        MoveInput = value;

        if (value != 0)
        {
            IsFacingRight = value > 0;
        }
    }
    
    private void InputCrouch(InputActionEventData inputData)
    {
        bool isPressedNow = inputData.GetButtonDown(),
             isPressed = inputData.GetButton();

        bool previousCrouch = IsCrouching;
        if (crouchInputToggle)
        {
            if (isPressedNow)
                IsCrouching = !IsCrouching;
        }
        else
        {
            IsCrouching = isPressed;
        }

        if (IsCrouching != previousCrouch)
        {
            normalCollider.enabled = !IsCrouching;
            crouchCollider.enabled = IsCrouching;
        }
    }


    private float _jumpInputCacheCurrent;
    private void InputJump(InputActionEventData inputData)
    {
        bool isPressed = inputData.GetButton();

        if (inputData.GetButtonDown() && !IsCrouching)
        {
            _jumpInputCacheCurrent = jumpInputCacheDuration;
        }
        else if (_jumpInputCacheCurrent > 0)
        {
            _jumpInputCacheCurrent -= Time.deltaTime;
        }

        if (IsGrounded && _jumpInputCacheCurrent > 0)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            IsJumping = true;
            _jumpInputCacheCurrent = 0;
        }

        if (IsJumping && !isPressed)
        {
            _rb.linearVelocityY = 0;
        }
        if (IsJumping && _rb.linearVelocityY <= 0)
        {
            IsJumping = false;
        }
    }

    private void InputFlashlight(InputActionEventData inputData)
    {
        bool isPressedNow = inputData.GetButtonDown(),
             isPressed = inputData.GetButton();

        if (flashlightInputToggle)
        {
            if (isPressedNow)
                IsFlashlightOn = !IsFlashlightOn;
        }
        else
        {
            IsFlashlightOn = isPressed;
        }

        if (IsFlashlightOn != flashlightObject.activeSelf)
        {
            flashlightObject.SetActive(IsFlashlightOn);
        }
    }

    private void InputFlashlightMove()
    {
        Vector3 vectorTowardsMouse, cursorWorldPoint;

        if (_input.controllers.GetLastActiveController() == null)
            return;
        
        if (_input.controllers.GetLastActiveController().type == ControllerType.Joystick)
        {
            // joystick input
            float valueX = _input.GetAxis("GP_FlashlightX"),
                  valueY = _input.GetAxis("GP_FlashlightY");

            valueX = (valueX == 0) ? ((IsFacingRight) ? 1 : -1) : valueX;
            cursorWorldPoint = transform.position + Vector3.right * (5 * valueX) + Vector3.up * (5 * valueY);
        }
        else
        {
            // if mouse, get mouse world position and aim flashlight towards it
            cursorWorldPoint = Camera.main.ScreenToWorldPoint(ReInput.controllers.Mouse.screenPosition);

        }
        
        vectorTowardsMouse = cursorWorldPoint - transform.position;
        // clamps vector Y so it can't be aimed down
        // we want the flashlight to always be leveled with the character
        vectorTowardsMouse.y = Mathf.Max(vectorTowardsMouse.y, 0);
            
        float rotZ = Mathf.Atan2(vectorTowardsMouse.y, vectorTowardsMouse.x) * Mathf.Rad2Deg;

        flashlightPivot.rotation = (IsFacingRight) 
                                    ? Quaternion.Euler(0, 0, rotZ) 
                                    : Quaternion.Euler(0, 180, 180 - rotZ);
    }

    private void InputInteract(InputActionEventData inputData)
    {
        List<RaycastHit2D> interactCastHits = new();
        interactionCollider.Cast(transform.right, interactCastHits, 0);

        foreach (var hit in interactCastHits)
        {
            if (hit.collider.TryGetComponent<Interactable>(out var interactableComp))
            {
                interactableComp.Interact(this.gameObject);
                break;
            }
        }
    }

    #endregion
}
