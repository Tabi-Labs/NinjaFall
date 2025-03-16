using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomInputManager : MonoBehaviour
{
    public PlayerInput PlayerInput;

    #region --- INPUT EVENTS ----
    public event Action MeleeAttackEvent;
    public event Action RangedAttackEvent;
    #endregion
    public Vector2 Movement;
    public bool JumpWasPressed;
    public  bool JumpIsHeld;
    public  bool JumpWasReleased;
    public  bool RunIsHeld;
    public  bool DashWasPressed;
    public  bool TestWasPressed;
    public bool MeleeAttackWasPressed;
    public bool RangeAttackWasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _testAction;
    private InputAction _meleeAttackAction;
    private InputAction _rangeAttackAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _meleeAttackAction = PlayerInput.actions["MeleeAttack"];
        _rangeAttackAction = PlayerInput.actions["RangeAttack"];
        _testAction = PlayerInput.actions["Test"];

        _rangeAttackAction.performed += OnRangeAttack;
        _meleeAttackAction.performed += OnMeleeAttack;
    }

    void OnDisable()
    {
         _rangeAttackAction.performed -= OnRangeAttack;
        _meleeAttackAction.performed -= OnMeleeAttack;
    }
    private void OnEnable()
    {
       /*   if(_inputActions == null)
        {
            _inputActions = new Controls();
            _inputActions.Player.AddCallbacks(this);
        }
        _inputActions.Player.SetCallbacks(this);
        _inputActions.Player.Enable(); */
    }
    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();

        MeleeAttackWasPressed = _meleeAttackAction.WasPressedThisFrame();

        RangeAttackWasPressed = _rangeAttackAction.WasPressedThisFrame();

        DashWasPressed = _dashAction.WasPressedThisFrame();

        TestWasPressed = _testAction.WasPressedThisFrame();
    }

    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        MeleeAttackEvent?.Invoke();
    }

    public void OnRangeAttack(InputAction.CallbackContext context)
    {
        RangedAttackEvent?.Invoke();
    }
}
