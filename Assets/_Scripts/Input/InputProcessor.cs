using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInput;


[CreateAssetMenu(menuName = "Input/Processor")]
public class InputProcessor : ScriptableObject, IPlayerActions
{
    public PlayerInput playerActions;
    public event Action JumpEvent;
    public event Action JumpCanceled;
    private Vector2 moveInput;
    public bool isRunning {get; private set;}
    public bool hasJumpPerformed {get; private set;}
    public Vector2 MoveInput => moveInput;

    public void Enable()
    {
        if(playerActions != null) 
        {
            playerActions.Enable();
            return;
        }
        
        playerActions = new PlayerInput();
        playerActions.Player.SetCallbacks(this);
        playerActions.Enable();

    }
    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
         switch(context.phase)
        {
            case InputActionPhase.Performed:
                JumpEvent?.Invoke();
                break;
            case InputActionPhase.Canceled:
                JumpCanceled?.Invoke();
                break;

        }
    }

    public void OnMovement(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
       moveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        switch(context.phase)
        {
            case InputActionPhase.Performed:
                isRunning = true;
                break;
            case InputActionPhase.Canceled:
                isRunning = false;
                break;

        }
    }

    public void OnDive(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnSwimUp(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnSpin(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnPickAndDrop(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnAirDive(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnStomp(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnReleaseLedge(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnGlide(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnGrindBrake(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}
