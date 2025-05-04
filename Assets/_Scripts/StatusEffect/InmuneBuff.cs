using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InmuneBuff : StatusEffect
{


    public override void ApplyEffect(GameObject player)
    {
        IDamageable damageable = player.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.SetInmune(true);
        }
    }

    public override void RemoveEffect(GameObject player)
    {
        IDamageable damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.SetInmune(false);
        }
    }
}
