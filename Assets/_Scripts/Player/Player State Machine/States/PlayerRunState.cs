using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();
        _player.Anim.Play("p_Walk");
        _player.Anim.SetBool(Player.IS_WALKING, true);
        _player.Anim.SetBool(Player.IS_RUNNING, true);
    }

    public override void StateExit()
    {
        base.StateExit();

        if (_player.SpeedParticles.isPlaying)
        {
            _player.SpeedParticles.Stop();
        }
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        //transitions
        if (Mathf.Abs(_player.InputManager.Movement.x) < _moveStats.MoveThreshold)
        {
            _player.Anim.SetBool(Player.IS_WALKING, false);
            _player.Anim.SetBool(Player.IS_RUNNING, false);

            _player.StateMachine.ChangeState(_player.IdleState);
        }

        else if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && !_player.InputManager.RunIsHeld)
        {
            _player.Anim.SetBool(Player.IS_RUNNING, false);

            _player.StateMachine.ChangeState(_player.WalkState);
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

        //FX
        HandleSpeedParticles();
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        //this gets called here for acceleration/movement
        _player.Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, _player.InputManager.Movement);
    }

    private void HandleSpeedParticles()
    {
        if (Mathf.Abs(_player.HorizontalVelocity) >= _player.MoveStats.MaxRunSpeed - 2f)
        {
            if (!_player.SpeedParticles.isPlaying)
            {
                _player.SpeedParticles.Play();
            }
        }

        else
        {
            if (_player.SpeedParticles.isPlaying)
            {
                _player.SpeedParticles.Stop();
            }
        }
    }
}
