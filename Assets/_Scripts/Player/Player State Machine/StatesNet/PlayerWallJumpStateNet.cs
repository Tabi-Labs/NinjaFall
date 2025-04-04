using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpStateNet : PlayerStateNet
{
    public PlayerWallJumpStateNet(PlayerNet player, PlayerStateMachineNet stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.InitiateWallJump();
        _player.StateMachine.ChangeState(_player.InAirState);
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        _player.WallJumpPhysics();

        //movement

        _player.Move(_moveStats.WallJumpMoveAcceleration, _moveStats.WallJumpMoveDeceleration, _player.InputManager.Movement);
        
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
