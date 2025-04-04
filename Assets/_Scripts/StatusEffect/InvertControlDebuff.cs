using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertControlDebuff : StatusEffect
{
    public override void ApplyEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetInvertControlDebuff(true);
        }
    }

    public override void RemoveEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetInvertControlDebuff(false);
        }
    }

}
