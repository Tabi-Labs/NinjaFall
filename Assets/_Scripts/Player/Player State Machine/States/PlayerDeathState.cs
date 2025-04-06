using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        _player.Movement.Stop();

        AudioManager.PlaySound("FX_Death", volume: 0.5f);
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
        if (_player.Anim.GetCurrentAnimatorStateInfo(0).IsName("p_Death") &&
            _player.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Debug.Log("Player Death Animation Finished");
            _player.DeletePlayer();
            return;
        }
    }
}
