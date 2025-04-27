using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBuff : StatusEffect
{
    public override void ApplyEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if(_player != null )
        {
            _player.SetSpeedBuff(true);
            _player.SpeedParticles.Play();
        }
    }

    public override void RemoveEffect(GameObject player)
    {
        Player _player = player.GetComponent<Player>();

        if (_player != null)
        {
            _player.SetSpeedBuff(false);
            _player.SpeedParticles.Stop();
        }
        Debug.Log("Eliminando efecto NUEVO");
        Destroy(gameObject);
    }
}
