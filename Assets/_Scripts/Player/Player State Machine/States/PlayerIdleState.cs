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

        _player.DisableSwordCollider();

        //_player.Anim.SetBool(Player.IS_WALKING, false);
        //_player.Anim.SetBool(Player.IS_RUNNING, false);
        _player.TrailRenderer.emitting = false;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (_player.CheckIsDead())
        {
            _player.StateMachine.ChangeState(_player.DeathState);
            return;
        }

        if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && !_player.InputManager.RunIsHeld)
        {
            _player.StateMachine.ChangeState(_player.WalkState);
        }

        else if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && _player.InputManager.RunIsHeld)
        {
            _player.StateMachine.ChangeState(_player.RunState);
        }

        else if (_player.InputManager.JumpWasPressed)
        {
            if (_player.CanJump())
            {
                _player.SpawnJumpParticles(_player.JumpParticles);

                _player.StateMachine.ChangeState(_player.JumpState);
            }
        }

        else if (_player.JumpBufferedOrCoyoteTimed())
        {
            _player.SpawnJumpParticles(_player.JumpParticles);

            _player.StateMachine.ChangeState(_player.JumpState);
        }

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

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        //this gets called here for deceleration
        _player.Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, _player.InputManager.Movement);
    }
}
