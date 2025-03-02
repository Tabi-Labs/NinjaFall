using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDamageable : Damageable
{
    private Player _player;

    void Awake()
    {
        _player = GetComponent<Player>();
    }
    protected override void OnDamageTaken()
    {
        base.OnDamageTaken();
        _player.OnHit();
    }
}
