using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRangeAttackStateNet : PlayerStateNet
{
    public PlayerRangeAttackStateNet(PlayerNet player, PlayerStateMachineNet stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();
        //_player.DisableSwordCollider();
        _player.Anim.Play("p_RangeAttack_1");
        _player.RangeAttackWasPressed();
    }

    public override void StateExit()
    {
        base.StateExit();    
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        // Si se presiona Dash durante el ataque, cambiar inmediatamente al estado Dash
        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.StateMachine.ChangeState(_player.DashState);
            return; 
        }

        // Transici�n a Idle si la animaci�n de ataque ha terminado
        if (_player.Anim.GetCurrentAnimatorStateInfo(0).IsName("p_RangeAttack_1") &&
            _player.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {

            // Transici�n a Walk si el jugador comienza a moverse
            if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && !_player.InputManager.RunIsHeld)
            {
                _player.StateMachine.ChangeState(_player.WalkState);
                return;
            }

            // Transici�n a Run si el jugador comienza a correr
            if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && _player.InputManager.RunIsHeld)
            {
                _player.StateMachine.ChangeState(_player.RunState);
                return;
            }

            // Si despues ataca a melee
            if (_player.InputManager.MeleeAttackWasPressed)
            {
                _player.StateMachine.ChangeState(_player.MeleeAttackState);
                return;
            }

            // Si no se cumple ninguna de las anteriores condiciones volvemos a Idle
            _player.StateMachine.ChangeState(_player.IdleState);
            return;
        }

        // Transici�n a Jump si el jugador presiona salto
        if (_player.InputManager.JumpWasPressed && _player.CanJump())
        {
            _player.StateMachine.ChangeState(_player.JumpState);
            return;
        }


    }
}
