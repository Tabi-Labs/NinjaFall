using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        //_player.DisableSwordCollider();
        _player.SetIsAttacking(true);
        _player.Anim.Play("p_MeleeAttack_1");
        _player.InitiateDash();        
    }

    public override void StateExit()
    {
        _player.SetIsAttacking(false);
        _player.Anim.Play("p_Idle");
        base.StateExit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        //WALL SLIDE
        if (_player.ShouldWallSlide())
        {
            _player.StateMachine.ChangeState(_player.WallSlideState);
            return;
        }

          if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);
            return;
        }


        if (!_player.IsDashing && !_player.IsGrounded && !_isExitingState)
        {
            _player.StateMachine.ChangeState(_player.InAirState);
            return;
        }

        else if (!_player.IsDashing && _player.IsGrounded && !_isExitingState)
        {
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
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        _player.DashPhysics();
    }
}
