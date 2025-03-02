using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachineNet
{
    public PlayerStateNet CurrentState { get; private set; }

    public void InitializeDefaultState(PlayerStateNet startState)
    {
        CurrentState = startState;
        CurrentState.StateEnter();
    }

    public void ChangeState(PlayerStateNet newState)
    {
        CurrentState.StateExit();
        CurrentState = newState;
        CurrentState.StateEnter();
    }
}
