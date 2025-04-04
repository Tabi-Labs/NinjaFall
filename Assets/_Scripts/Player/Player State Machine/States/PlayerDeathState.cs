using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();
       
        _player.Anim.Play("p_Death");
        _player.OnPlayerDeath.Raise(_player, null);

        _player.Input().Disable();
        _player.Movement.Stop();
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
        // Transici�n a Idle si la animaci�n de ataque ha terminado
        if (_player.Anim.GetCurrentAnimatorStateInfo(0).IsName("p_Death") &&
            _player.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {

            // TODO Destruir objeto player
            _player.DeletePlayer();
            return;
        }


    }
}
