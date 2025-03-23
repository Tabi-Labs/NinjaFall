using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SpeedBuff : StatusEffect
{
    public override void ApplyEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if(_player != null )
        {
            _player.SetSpeedBuff(true);
        }
    }

    public override void RemoveEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetSpeedBuff(false);
        }
    }
}
