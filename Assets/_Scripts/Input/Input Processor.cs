using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Processor")]
public class InputProcessor : ScriptableObject, Controls.IPlayerActions
{
    private Controls _inputActions;

    public Vector2 MoveInput;

    public void Enable()
    {
        if(_inputActions == null)
        {
            _inputActions = new Controls();
            _inputActions.Player.AddCallbacks(this);
        }
        _inputActions.Player.SetCallbacks(this);
        _inputActions.Player.Enable();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnRangeAttack(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    public void OnTest(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }
}
