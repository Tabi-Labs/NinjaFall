using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicBuff : StatusEffect
{
    public override void ApplyEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetToxicBuff(true);
            _player.SetInmuneToxic(true);
        }
    }

    public override void RemoveEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetToxicBuff(false);
            _player.SetInmuneToxic(false);
        }

        Debug.Log("Eliminando efecto NUEVO");
    }
}
