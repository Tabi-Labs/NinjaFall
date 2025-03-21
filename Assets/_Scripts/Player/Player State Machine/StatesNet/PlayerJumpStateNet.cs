using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpStateNet : PlayerStateNet
{
    public PlayerJumpStateNet(PlayerNet player, PlayerStateMachineNet stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.Anim.Play("p_Jump");
        _player.InitiateJump();

        _player.StateMachine.ChangeState(_player.InAirState);
    }

    public override void StateExit()
    {
        base.StateExit();    
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        //even though we exit this state immediately, we need to account for movement in case a physics update is called before the transition is made for the sake of smooth movement

        _player.Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, _player.InputManager.Movement);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);
        }
        if (_player.InputManager.MeleeAttackWasPressed)
        {
            _player.StateMachine.ChangeState(_player.MeleeAttackState);
        }

        if (_player.InputManager.RangeAttackWasPressed)
        {
            _player.StateMachine.ChangeState(_player.RangeAttackState);
        }
    }
}
