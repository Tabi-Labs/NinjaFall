using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomInputManager : MonoBehaviour
{
    private PlayerInput PlayerInput;

    #region --- INPUT EVENTS ----
    public event Action MeleeAttackEvent;
    public event Action RangedAttackEvent;
    #endregion

    private Vector2 _movement;
    public Vector2 Movement => _movement;
    public bool JumpWasPressed {get; private set; }
    public  bool JumpIsHeld{get; private set; }
    public  bool JumpWasReleased{get; private set; }
    public  bool RunIsHeld{get; private set; }
    public  bool DashWasPressed{get; private set; }
    public  bool TestWasPressed{get; private set; }
    public bool MeleeAttackWasPressed{get; private set; }
    public bool RangeAttackWasPressed{get; private set; }

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

    public void Enable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _runAction.Enable();
        _dashAction.Enable();
        _meleeAttackAction.Enable();
        _rangeAttackAction.Enable();
        _testAction.Enable();

        _rangeAttackAction.performed += OnRangeAttack;
        _meleeAttackAction.performed += OnMeleeAttack;
    }
    public void Disable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _runAction.Disable();
        _dashAction.Disable();
        _meleeAttackAction.Disable();
        _rangeAttackAction.Disable();
        _testAction.Disable();

        _rangeAttackAction.performed -= OnRangeAttack;
        _meleeAttackAction.performed -= OnMeleeAttack;
    }
    void OnDisable()
    {
          Disable();
    }
    private void OnEnable()
    {
        Enable();
    }
    private void Update()
    {
        _movement = _moveAction.ReadValue<Vector2>();

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
