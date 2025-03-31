using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.Anim.Play("p_Idle");

        //_player.DisableSwordCollider();

        //_player.Anim.SetBool(Player.IS_WALKING, false);
        //_player.Anim.SetBool(Player.IS_RUNNING, false);
        _player.TrailRenderer.emitting = false;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();


        if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && !_player.InputManager.RunIsHeld)
        {
            _player.StateMachine.ChangeState(_player.WalkState);
            return;
        }

        else if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && _player.InputManager.RunIsHeld)
        {
            _player.StateMachine.ChangeState(_player.RunState);
            return;
        }

        else if (_player.InputManager.JumpWasPressed)
        {
            if (_player.CanJump())
            {
                _player.SpawnJumpParticles(_player.JumpParticles);

                _player.StateMachine.ChangeState(_player.JumpState);

                return;
            }
        }

        else if (_player.JumpBufferedOrCoyoteTimed())
        {
            _player.SpawnJumpParticles(_player.JumpParticles);

            _player.StateMachine.ChangeState(_player.JumpState);

            return;
        }

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

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        //this gets called here for deceleration
        _player.Movement.Move(_moveStats.GroundAcceleration, _player.InputManager.Movement, _moveStats.GroundDeceleration);
    }
}
