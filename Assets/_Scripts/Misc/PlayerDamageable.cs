using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDamageable : Damageable
{
    private Player _player;

    protected override void Awake()
    {
        base.Awake();
        _player = GetComponent<Player>();
    }
    protected override void OnDamageTaken()
    {
        base.OnDamageTaken();
    
        if (_player != null) _player.Death();
    }
  
}
