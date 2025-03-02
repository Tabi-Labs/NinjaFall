using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashStateNet : PlayerStateNet
{
    public PlayerDashStateNet(PlayerNet player, PlayerStateMachineNet stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.InitiateDash();        
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        //WALL SLIDE
        if (_player.ShouldWallSlide())
        {
            _player.StateMachine.ChangeState(_player.WallSlideState);
        }

        if (!_player.IsDashing && !_player.IsGrounded && !_isExitingState)
        {
            _player.StateMachine.ChangeState(_player.InAirState);
        }

        else if (!_player.IsDashing && _player.IsGrounded && !_isExitingState)
        {
            _player.StateMachine.ChangeState(_player.IdleState);
        }

        else if (_player.InputManager.JumpWasPressed)
        {
            if (_player.CanJump())
            {
                _player.SpawnJumpParticles(_player.JumpParticles);

                _player.StateMachine.ChangeState(_player.JumpState);
            }
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

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        _player.DashPhysics();
    }
}
