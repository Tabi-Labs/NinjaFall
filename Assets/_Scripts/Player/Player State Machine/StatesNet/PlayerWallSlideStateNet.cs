using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideStateNet : PlayerStateNet
{
    private Vector2 _hitPosition;

    public PlayerWallSlideStateNet(PlayerNet player, PlayerStateMachineNet stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.Anim.Play("p_WallSlide");
        if (_player.WallSlideParticles.isPlaying)
        {
            _player.WallSlideParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        _player.WallSlideParticles.gameObject.SetActive(true);
        _player.WallSlideParticles.Play();

        _player.ResetJumpValues();
        _player.ResetWallJumpValues();
        _player.ResetDashValues();

        if (_player.MoveStats.ResetDashOnWallSlide)
        {
            _player.ResetDashes();
        }

        _player.IsWallSlideFalling = false;
        _player.IsWallSliding = true;

        _player.Anim.SetBool(Player.IS_WALL_SLIDING, true);

        if (_player.MoveStats.ResetJumpsOnWallSlide)
        {
            _player.ReplenishJumps();
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

    public override void StateExit()
    {
        base.StateExit();

        _player.WallSlideParticles.Stop();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        //EXITS wall slide due to moving away from wall
        if (_player.ShouldStopWallSliding())
        {
            _player.IsWallSlideFalling = true;
            _player.StopWallSliding();

            _player.StateMachine.ChangeState(_player.InAirState);
        }

        //EXITS wall slide due to landing on ground
        else if (_player.HasLanded())
        {
            _player.StateMachine.ChangeState(_player.IdleState);
        }

        //WALLJUMP
        else if (_player.InputManager.JumpWasPressed && _player.CanWallJumpDueToPostBufferTimer())
        {
            _player.UseWallJumpMoveStats = true;
            _player.StateMachine.ChangeState(_player.WallJumpState);
        }

        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        _player.ChangeVerticalVelocity(Mathf.Lerp(_player.VerticalVelocity, -_player.MoveStats.WallSlideSpeed, _player.MoveStats.WallSlideDecelerationSpeed * Time.fixedDeltaTime));

        if (_player.UseWallJumpMoveStats)
        {
            _player.Move(_moveStats.WallJumpMoveAcceleration, _moveStats.WallJumpMoveDeceleration, _player.InputManager.Movement);
        }

        //movement
        MoveCheck();        
    }

    //we are doing this so the player avoids occasionally getting 'stuck' at just the right angle on certain floors. Taking away movement ensures he just slides down past that point
    private void MoveCheck()
    {
        if (_player.WallHit.collider != null)
        {
            _hitPosition = _player.WallHit.collider.ClosestPoint(_player.transform.position);

            if (_player.InputManager.Movement.x > 0 && _hitPosition.x > _player.transform.position.x)
            {
                _player.Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, Vector2.zero);
            }

            else if (_player.InputManager.Movement.x < 0 && _hitPosition.x < _player.transform.position.x)
            {
                _player.Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, Vector2.zero);
            }
            else
            {
                _player.Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, _player.InputManager.Movement);
            }
        }
    }
}
