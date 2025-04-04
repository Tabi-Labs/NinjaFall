using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        //_player.DisableSwordCollider();

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

        _player.Movement.Move(_moveStats.WallJumpMoveAcceleration, _player.GetMovement(), _moveStats.WallJumpMoveDeceleration);
        
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);

            return;
        }

        if (_player.InputManager.MeleeAttackWasPressed)
        {
            _player.StateMachine.ChangeState(_player.MeleeAttackState);

            return;
        }

        if (_player.InputManager.RangeAttackWasPressed)
        {
            _player.StateMachine.ChangeState(_player.RangeAttackState);

            return;
        }


    }
}
