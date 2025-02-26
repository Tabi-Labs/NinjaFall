using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttackState : PlayerState
{
    public PlayerMeleeAttackState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

        _player.SetIsAttacking(true);

        _player.EnableSwordCollider();
        
        _player.Anim.Play("p_MeleeAttack_1");
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

        if (_player.CheckIsDead())
        {
            _player.StateMachine.ChangeState(_player.DeathState);
            return;
        }

        // Si se presiona Dash durante el ataque, cambiar inmediatamente al estado Dash
        if (_player.InputManager.DashWasPressed && (_player.CanDash() || _player.CanAirDash()))
        {
            _player.SetIsAttacking(false);
            _player.DisableSwordCollider();
            _player.StateMachine.ChangeState(_player.DashState);
            return; 
        }

        // Transición a Idle si la animación de ataque ha terminado
        if (_player.Anim.GetCurrentAnimatorStateInfo(0).IsName("p_MeleeAttack_1") &&
            _player.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            _player.CheckForFalling();
            _player.SetIsAttacking(false);
            _player.DisableSwordCollider();

            // Transición a Walk si el jugador comienza a moverse
            if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && !_player.InputManager.RunIsHeld)
            {
                _player.StateMachine.ChangeState(_player.WalkState);
                return;
            }

            // Transición a Run si el jugador comienza a correr
            if (Mathf.Abs(_player.InputManager.Movement.x) > _moveStats.MoveThreshold && _player.InputManager.RunIsHeld)
            {
                _player.StateMachine.ChangeState(_player.RunState);
                return;
            }

            // Si se pulsa el boton de ataque a distancia
            if (_player.InputManager.RangeAttackWasPressed)
            {
                _player.StateMachine.ChangeState(_player.RangeAttackState);
                return;
            }

            // Si no se cumple ninguna de las anteriores condiciones volvemos a Idle
            _player.StateMachine.ChangeState(_player.IdleState);
            return;
        }

        // Transición a Jump si el jugador presiona salto
        if (_player.InputManager.JumpWasPressed && _player.CanJump())
        {
            _player.SetIsAttacking(false);
            _player.DisableSwordCollider();
            _player.StateMachine.ChangeState(_player.JumpState);
            return;
        }

    }
}
