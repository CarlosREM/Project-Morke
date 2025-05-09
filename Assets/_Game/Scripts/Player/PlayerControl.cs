using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Rewired;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    [SerializeField, Range(1, 179)] private float lookUpAngle = 90;
    
    private bool JumpIntent
    { 
        get => _jumpInputBufferCurrent > 0; 
        set => _jumpInputBufferCurrent = (value) ? jumpInputBufferDuration : 0; 
    }
    private bool _jumpCancel;

    public bool IsJumping { get; private set; }

    public bool IsCrouching { get; private set; }
    public bool IsLookingUp { get; private set; }
    public bool IsLookingBack { get; private set; }

    [Header("Ground check")] 
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private ContactFilter2D groundCheckFilter;
    public bool IsGrounded { get; private set; }
    
    [SerializeField] private float slopeMaxIncline = 47.5f;
    public bool IsOnSlope { get; private set; }
    private Vector2 _slopeNormalPerp;

    [SerializeField] private PhysicsMaterial2D noFrictionPhysMat;
    [SerializeField] private PhysicsMaterial2D yesFrictionPhysMat;

    [Header("Camera Focus")] 
    [SerializeField] private Transform camTargetPlayer;
    public Transform CamTargetPlayer => camTargetPlayer;
    [SerializeField] private Transform camTargetCursor;
    public Transform CamTargetCursor => camTargetCursor;

    
    [Header("Additional Components")]
    [SerializeField] private Collider2D normalCollider;
    [SerializeField] private Collider2D crouchCollider;
    public PlayerFlashlight flashlight;
    [SerializeField] private Transform flashlightTargetPivot;
    [SerializeField] private Collider2D interactionCollider;
    [SerializeField] private Transform flashlightInputTest;

    public Vector2 VelocityVector => rb.linearVelocity;

    public bool IsFacingRight { get; private set; } = true;

    private Rewired.ControllerType _lastCursorInput;
    
    #region Initialization
    
    private void Awake()
    {
        _input = GameInputManager.MainPlayer;
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        Assert.IsNotNull(_input, "Player input is not initialized");
        
        //set input callbacks
        _input.AddInputEventDelegate(InputMove, UpdateLoopType.Update, InputActionEventType.Update, "GP_Move");
        //_input.AddInputEventDelegate(InputCrouch, UpdateLoopType.Update, InputActionEventType.Update, "GP_Crouch");
        _input.AddInputEventDelegate(InputJump, UpdateLoopType.Update, InputActionEventType.Update, "GP_Jump");
        _input.AddInputEventDelegate(InputFlashlight, UpdateLoopType.Update, InputActionEventType.Update, "GP_Flashlight");
        _input.AddInputEventDelegate(InputInteract, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Interact");
        _input.AddInputEventDelegate(InputRecharge, UpdateLoopType.Update, InputActionEventType.Update, "GP_Reload");
        _input.AddInputEventDelegate(InputPause, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Pause");

        health.OnDeath += OnDeath;
        
        camTargetCursor.gameObject.SetActive(true);
        flashlight.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        Debug.Log("[PlayerInput] <color=green>Ready</color>");
    }

    private void OnDisable()
    {
        Assert.IsNotNull(_input, "Player input is not initialized");

        // remove input callbacks
        _input.RemoveInputEventDelegate(InputMove, UpdateLoopType.Update, InputActionEventType.Update, "GP_Move");
        //_input.RemoveInputEventDelegate(InputCrouch, UpdateLoopType.Update, InputActionEventType.Update, "GP_Crouch");
        _input.RemoveInputEventDelegate(InputJump, UpdateLoopType.Update, InputActionEventType.Update, "GP_Jump");
        _input.RemoveInputEventDelegate(InputFlashlight, UpdateLoopType.Update, InputActionEventType.Update, "GP_Flashlight");
        _input.RemoveInputEventDelegate(InputInteract, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Interact");
        _input.RemoveInputEventDelegate(InputRecharge, UpdateLoopType.Update, InputActionEventType.Update, "GP_Reload");
        _input.RemoveInputEventDelegate(InputPause, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "GP_Pause");
        
        health.OnDeath -= OnDeath;
        
        camTargetCursor.gameObject.SetActive(false);
        MoveInput = 0;
        JumpIntent = false;
        rb.sharedMaterial = yesFrictionPhysMat;
        
        Debug.Log("[PlayerInput] <color=red>Disabled</color>");
    }

    private void OnDeath()
    {
        IsLookingUp = false;
        IsLookingBack = false;
        flashlight.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
    }
    
    #endregion
    
    #region Update Loop

    private void OnDrawGizmos()
    {
        // ground check
        Vector2 groundCheckOrigin = ((Vector2) transform.position) + groundCheckOffset;

        Gizmos.color = (IsGrounded) ? Color.cyan : Color.red;
        Gizmos.DrawWireCube(groundCheckOrigin, groundCheckSize);
        
        // draw look up angle
        Vector3[] lookUpPoints =
        {
            Quaternion.Euler(0, 0, lookUpAngle/2) * (Vector3.up * 6) + flashlightTargetPivot.position,
            flashlightTargetPivot.position,
            Quaternion.Euler(0, 0, -lookUpAngle/2) * (Vector3.up * 6) + flashlightTargetPivot.position,
        };
        Gizmos.color = Color.white;
        Gizmos.DrawLineStrip(lookUpPoints, false);
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

        ProcessMovement();
    }

    private List<RaycastHit2D> _groundCheckResults = new();
    private bool GroundCheck()
    {
        if (IsJumping)
            return false;
        
        Vector2 groundCheckOrigin = rb.position + groundCheckOffset;
        var hitCount = Physics2D.BoxCast(groundCheckOrigin, groundCheckSize, 0f, Vector2.down, groundCheckFilter,
            _groundCheckResults, 0);
        
        if (hitCount > 0)
        {
            float centerDistance = Mathf.Infinity;
            RaycastHit2D hit = _groundCheckResults[0]; // defaults to first result

            for (int i = 0; i < hitCount && hitCount > 1; ++i)
            {
                var distance = Vector2.Distance(groundCheckOrigin, _groundCheckResults[i].point);
                if (distance > centerDistance)
                    continue;
                
                centerDistance = distance;
                hit = _groundCheckResults[i];
            }
            
            // Note: for some forsaken reason the dude walks backwards on slopes if the Perp Normal is not inverted
            _slopeNormalPerp = -Vector2.Perpendicular(hit.normal).normalized;
            var surfaceAngle = Vector2.Angle(hit.normal, Vector2.up);
            
            #if UNITY_EDITOR
            Debug.DrawRay(hit.point, hit.normal/2, Color.green); // up ray
            Debug.DrawRay(hit.point, _slopeNormalPerp/2, Color.red); // right ray
            #endif
            
            if (surfaceAngle >= slopeMaxIncline)
            {
                IsOnSlope = false;
                return false;
            }

            IsOnSlope = surfaceAngle > 0;
        }
        else
            IsOnSlope = false;
        
        return hitCount > 0;
    }

    private void ProcessMovement()
    {
        // if charging just ignore all movement input
        if (flashlight.IsRecharging)
            goto LimitVelocityY;
        
        // ground movement
        if (IsGrounded)
        {
            rb.sharedMaterial = (MoveInput == 0) ? yesFrictionPhysMat : noFrictionPhysMat;

            if (MoveInput != 0)
            {
                if (IsOnSlope)
                {
                    // move according to slope surface (both X and Y, yes)
                    rb.linearVelocity = MoveInput * moveSpeed * _slopeNormalPerp;
                }
                else
                    rb.linearVelocityX = MoveInput * moveSpeed;

                // limit move speed when crouching
                if (IsCrouching)
                    rb.linearVelocity *= moveSpeedCrouchMultiplier;
            }

            // jump
            if (IsGrounded && JumpIntent)
            {
                rb.linearVelocityY = jumpForce;
                JumpIntent = false;
                IsJumping = true;
                IsGrounded = false;
            }
        }
        // air movement
        else
        {
            rb.sharedMaterial = noFrictionPhysMat; // this is to avoid sticking to surfaces
            
            rb.linearVelocityX = MoveInput * moveSpeed;
            
            // TODO: when receiving any impulse from other forces, cancel out jump
            if (IsJumping && _jumpCancel)
            {
                rb.linearVelocityY = 0;
                _jumpCancel = false;
            }
            
            if (IsJumping && rb.linearVelocityY <= 0)
                IsJumping = false;
        }
        
        LimitVelocityY:
        // limit Y velocity
        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, airMinVelocity, airMaxVelocity);
    }
    
    #endregion
    
    #region Input Events
    
    [Header("Input parameters")]
    [SerializeField] private float jumpInputBufferDuration;
    [SerializeField] private bool crouchInputToggle;
    [SerializeField] private bool flashlightInputToggle;

    public float MoveInput { get; private set; }

    private void InputMove(InputActionEventData inputData)
    {
        MoveInput =  inputData.GetAxis();
        
        if (MoveInput != 0)
        {
            bool previousDir = IsFacingRight; 
            IsFacingRight = MoveInput > 0;
            if (previousDir != IsFacingRight) // flip flashlight rotation when character rotates
                flashlight.FlipRotation(IsFacingRight);
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


    private float _jumpInputBufferCurrent;
    private void InputJump(InputActionEventData inputData)
    {
        // can't jump while recharging
        if (flashlight.IsRecharging)
            return;
        
        if (inputData.GetButtonDown() && !IsCrouching)
        {
            JumpIntent = true;
        }
        else if (JumpIntent)
        {
            _jumpInputBufferCurrent -= Time.deltaTime;
        }

        if (IsJumping && !inputData.GetButton())
        {
            _jumpCancel = true;
        }
    }

    private void InputFlashlight(InputActionEventData inputData)
    {
        if (flashlight.IsRecharging)
            return;
        
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
        if (!flashlight.isActiveAndEnabled)
            return;
        
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

                //Debug.Log($"Pivot rotation {rotValue} (X:{valueX} Y:{valueY})");
                break;
            }
        }
        flashlightTargetPivot.rotation = Quaternion.Euler(0, 0, rotValue);

        float minLookUpAngle = 90 - (lookUpAngle / 2),
              maxLookUpAngle = 90 + (lookUpAngle / 2);
        IsLookingUp =  rotValue > minLookUpAngle && rotValue < maxLookUpAngle;
        IsLookingBack = (IsFacingRight) ? rotValue > 90 : rotValue < 90;
        flashlight.SetRotation( (IsFacingRight) ? rotValue : 180 - rotValue);
    }

    private void InputInteract(InputActionEventData inputData)
    {
        List<RaycastHit2D> interactCastHits = new();
        interactionCollider.Cast(transform.right, interactCastHits, 0);

        foreach (var hit in interactCastHits)
        {
            if (hit.collider.TryGetComponent<InteractableObject>(out var interactableComp))
            {
                interactableComp.Interact(this.gameObject);
                break;
            }
        }
    }

    private bool btnPressedRecharge => _rechargeInputBuffer > 0;
    private float _rechargeInputBuffer = 0;
    private void InputRecharge(InputActionEventData obj)
    {
        if (obj.GetButtonDown())
            _rechargeInputBuffer = 0.1f;
        
        else if (!obj.GetButton())
            _rechargeInputBuffer -= (_rechargeInputBuffer > 0) ? Time.deltaTime : 0;
        
        // can't recharge in the air
        if (!IsGrounded)
            return;
        
        if (btnPressedRecharge && !flashlight.IsRecharging)
        {
            flashlight.SetRechargeStatus(true);
            rb.sharedMaterial = yesFrictionPhysMat;
        }
        else if (!btnPressedRecharge && flashlight.IsRecharging)
            flashlight.SetRechargeStatus(false);
    }

    private void InputPause(InputActionEventData obj)
    {
        OnPauseTriggered?.Invoke();
    }
    
    #endregion
}
