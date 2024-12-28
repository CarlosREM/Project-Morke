using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{
    public Action OnPauseTriggered;
    
    private Rewired.Player _input;
    public Rigidbody2D rb { get; private set; }
    public CharacterHealth health { get; private set; }

    [Header("Player Parameters")] 
    [SerializeField] private float moveSpeed;
    [SerializeField, Range(0, 1)] private float moveSpeedCrouchMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private float airMaxVelocity;
    [SerializeField] private float airMinVelocity;
    
    public bool IsJumping { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsLookingUp { get; private set; }
    public bool IsLookingBack { get; private set; }
    
    [Header("Ground check")]
    [SerializeField] private LayerMask groundCheckLayer;
    [SerializeField, Tooltip("Changing Z does nothing")] private Bounds groundCheckBounds;
    public bool IsGrounded { get; private set; }

    [Header("Camera Focus")] 
    [SerializeField] private Transform camTargetPlayer;
    public Transform CamTargetPlayer => camTargetPlayer;
    [SerializeField] private Transform camTargetCursor;
    public Transform CamTargetCursor => camTargetCursor;

    
    [Header("Additional Components")]
    [SerializeField] private Collider2D normalCollider;
    [SerializeField] private Collider2D crouchCollider;
    [SerializeField] private PlayerFlashlight flashlight;
    [SerializeField] private Transform flashlightTargetPivot;
    [SerializeField] private Collider2D interactionCollider;
    [SerializeField] private Transform flashlightInputTest;
    public float VelocityY { get; private set; }

    public bool IsFacingRight { get; private set; } = true;

    private Rewired.ControllerType _lastCursorInput;
    
    private 
        
    #region Initialization
    
    void Awake()
    {
        _input = ReInput.players.SystemPlayer;
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<CharacterHealth>();
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
        _input.AddInputEventDelegate(InputRecharge, UpdateLoopType.Update, InputActionEventType.Update, "GP_Reload");
        _input.AddInputEventDelegate(InputPause, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Pause");

        health.OnDeath += OnDeath;
        
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
        _input.RemoveInputEventDelegate(InputRecharge, UpdateLoopType.Update, InputActionEventType.Update, "GP_Reload");
        _input.RemoveInputEventDelegate(InputPause, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Pause");
        
        health.OnDeath -= OnDeath;
        
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

        VelocityY = Mathf.Clamp(rb.linearVelocityY, airMinVelocity, airMaxVelocity);
        rb.linearVelocityY = VelocityY;
    }

    private bool GroundCheck()
    {
        Vector2 groundCheckOrigin = rb.position + (Vector2) groundCheckBounds.center;
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
        // can't move while recharging
        if (flashlight.IsRecharging)
            return;
        
        float value = inputData.GetAxis();

        float crouchMultiplier = (IsCrouching) ? moveSpeedCrouchMultiplier : 1; 
        rb.linearVelocityX = value * moveSpeed * crouchMultiplier;
        MoveInput = value;

        if (value != 0)
        {
            IsFacingRight = value > 0;
        }
    }
    
    private void InputCrouch(InputActionEventData inputData)
    {
        // can't crouch while recharging
        if (flashlight.IsRecharging)
            return;
        
        bool previousCrouch = IsCrouching;
        if (crouchInputToggle)
        {
            if (inputData.GetButtonDown())
                IsCrouching = !IsCrouching;
        }
        else
        {
            IsCrouching = inputData.GetButton();
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
        // can't jump while recharging
        if (flashlight.IsRecharging)
            return;
        
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
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            IsJumping = true;
            _jumpInputCacheCurrent = 0;
        }

        if (IsJumping && !inputData.GetButton())
        {
            rb.linearVelocityY = 0;
        }
        if (IsJumping && rb.linearVelocityY <= 0)
        {
            IsJumping = false;
        }
    }

    private void InputFlashlight(InputActionEventData inputData)
    {
        if (flashlightInputToggle)
        {
            if (inputData.GetButtonDown())
                flashlight.ToggleFlashlight();
        }
        else
        {
            if (inputData.GetButtonDown())
                flashlight.TurnOn();
            else if (inputData.GetButtonUp())
                flashlight.TurnOff();
        }
    }

    private void InputFlashlightMove()
    {
        // first up, check on last device that gave input to flashlight movement
        // to handle it appropriately
        List<Rewired.InputActionSourceData> flashlightInputList = new (_input.GetCurrentInputSources("GP_FlashlightX"));
        flashlightInputList.AddRange(_input.GetCurrentInputSources("GP_FlashlightY"));

        if (flashlightInputList.Any())
        {
            _lastCursorInput = flashlightInputList[0].controllerType;
            //Debug.Log($"Last Cursor input: {_lastCursorInput}");
        }

        float rotValue = 0;
        switch (_lastCursorInput)
        {
            case ControllerType.Mouse:
            {
                // if mouse, get mouse world position and aim flashlight towards it
                var cursorPos = Camera.main.ScreenToWorldPoint(ReInput.controllers.Mouse.screenPosition);
                cursorPos.z = 0;

                flashlightInputTest.position = cursorPos;
                var testPos = flashlightInputTest.localPosition;
                testPos.x = Mathf.Clamp(testPos.x, -2, 2);
                testPos.y = Mathf.Clamp(testPos.y, -2, 2);
                flashlightInputTest.localPosition = testPos;

                
                var vectorTowardsMouse = cursorPos - transform.position;
                
                // clamps vector Y so it can't be aimed down
                // we want the flashlight to always be leveled with the character
                vectorTowardsMouse.y = Mathf.Max(vectorTowardsMouse.y, 0);
            
                rotValue = Mathf.Atan2(vectorTowardsMouse.y, vectorTowardsMouse.x) * Mathf.Rad2Deg;
                //flashlightCursor.position = cursorPos;
                break;
            }

            case ControllerType.Keyboard:
            case ControllerType.Joystick:
            {
                float valueX = _input.GetAxis("GP_FlashlightX"),
                    valueY = _input.GetAxis("GP_FlashlightY");

                flashlightInputTest.localPosition = Vector3.right * (2*valueX) + Vector3.up * (2*valueY);
                
                // points cursor either left or right, depending on the look direction & axis input
                if (IsFacingRight)
                    rotValue = (valueX >= 0) ? 0 : 180;
                else
                    rotValue = (valueX <= 0) ? 180 : 0;

                valueY = Mathf.Max(valueY, 0); // clamp Y from having negative values
                var absX = Mathf.Abs(valueX);
                
                if (absX > 0.5)
                {
                    valueY = Mathf.Min(0.5f, valueY);
                }
                else if (valueY > 0.5f && absX > 0)
                {
                    valueY = Mathf.Min(valueY, valueY - absX);
                }

                rotValue += (rotValue > 0) ? valueY * -90 : valueY * 90;

                Debug.Log($"Pivot rotation {rotValue} (X:{valueX} Y:{valueY})");
                break;
            }
        }
        flashlightTargetPivot.rotation = Quaternion.Euler(0, 0, rotValue);

        IsLookingUp =  rotValue is > 45 and < 135;
        IsLookingBack = (IsFacingRight) ? rotValue > 90 : rotValue < 90;
        flashlight.SetRotation( (IsFacingRight) ? rotValue : 180 - rotValue);
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

    private void InputRecharge(InputActionEventData obj)
    {
        if (obj.GetButtonDown())
            flashlight.SetRechargeStatus(true);
        
        else if (obj.GetButtonUp())
            flashlight.SetRechargeStatus(false);
    }

    private void InputPause(InputActionEventData obj)
    {
        OnPauseTriggered?.Invoke();
    }
    
    #endregion
}
