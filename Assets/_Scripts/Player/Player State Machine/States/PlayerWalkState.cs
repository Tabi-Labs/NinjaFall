using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();
        //_player.DisableSwordCollider();

        _player.Anim.Play("p_Walk");
        //_player.Anim.SetBool(Player.IS_WALKING, true);
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (_player.CheckIsDead())
        {
            _player.StateMachine.ChangeState(_player.DeathState);
            return;
        }

        if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && _player.InputManager.RunIsHeld)
        {
            _player.StateMachine.ChangeState(_player.RunState);
            return;
        }


        else if (Mathf.Abs(_player.InputManager.Movement.x) < _moveStats.MoveThreshold)
        {
            //_player.Anim.SetBool(Player.IS_WALKING, false);

            _player.StateMachine.ChangeState(_player.IdleState);

            return;
        }

        else if (_player.InputManager.JumpWasPressed)
        {
            if (_player.CanJump())
            {
                if (_player.IsToxicBuffActive())
                {
                    _player.SpawnJumpParticles(_player.ToxicParticles);
                }
                else
                {
                    _player.SpawnJumpParticles(_player.JumpParticles);
                }

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

        //this gets called here for acceleration/movement
        _player.Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, _player.InputManager.Movement);
    }
}
