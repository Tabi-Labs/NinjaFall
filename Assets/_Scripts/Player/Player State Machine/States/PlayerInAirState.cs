using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInAirState : PlayerState
{
    public PlayerInAirState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();


        //JUMP/WallJump
        if (_player.InputManager.JumpWasPressed)
        {
            if (_player.CanJump())
            {
                _player.SpawnJumpParticles(_player.JumpParticles);

                _player.StateMachine.ChangeState(_player.JumpState);

                return;
            }

            if (_player.CanAirJump())
            {
                _player.SpawnJumpParticles(_player.SecondJumpParticles);

                _player.StateMachine.ChangeState(_player.JumpState);

                return;
            }

            if (_player.CanWallJumpDueToPostBufferTimer())
            {
                _player.UseWallJumpMoveStats = true;
                _player.StateMachine.ChangeState(_player.WallJumpState);

                return;
            }
        }

        else if (_player.JumpBufferedOrCoyoteTimed())
        {
            _player.SpawnJumpParticles(_player.JumpParticles);
            _player.StateMachine.ChangeState(_player.JumpState);

            return;
        }

        
        //LAND
        if (_player.HasLanded())
        {
            _player.SpawnJumpParticles(_player.LandParticles);
            _player.StateMachine.ChangeState(_player.IdleState);

            return;
        }

        //WALL SLIDE
        if (_player.ShouldWallSlide())
        {
            _player.StateMachine.ChangeState(_player.WallSlideState);

            return;
        }

        //DASH
        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);

            return;
        }

        //ATTACK
        if (_player.InputManager.MeleeAttackWasPressed)
        {
            _player.ResetJumpValues();
            _player.ResetWallJumpValues();
            _player.StopWallSliding();
            _player.StateMachine.ChangeState(_player.MeleeAttackState);

            return;
        }

        if (_player.InputManager.RangeAttackWasPressed)
        {
            _player.ResetJumpValues();
            _player.ResetWallJumpValues();
            _player.StopWallSliding();
            _player.StateMachine.ChangeState(_player.RangeAttackState);

            return;
        }


    }
    

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

       /*  _player.JumpPhysics();
        _player.WallJumpPhysics();
        _player.DashPhysics(); */


        //movement
        if (_player.UseWallJumpMoveStats)
        {
            _player.Move(_moveStats.WallJumpMoveAcceleration, _moveStats.WallJumpMoveDeceleration, _player.InputManager.Movement);
        }

        else
        {
            _player.Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, _player.InputManager.Movement);     
        }
    }
}
