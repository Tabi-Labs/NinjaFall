using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShurikenWallBuff : StatusEffect
{
    public override void ApplyEffect(GameObject player)
    {
        RangedAttack rangedAttack = player.GetComponent<RangedAttack>();


        if (rangedAttack != null)
        {
            Debug.Log("Shuriken wall buff activado");
            rangedAttack.ApplyWallBuff(true);
        }
    }

    public override void RemoveEffect(GameObject obj)
    {
        RangedAttack rangedAttack = obj.GetComponent<RangedAttack>();


        if (rangedAttack != null)
        {
            Debug.Log("Shuriken wall buff desactivado");
            rangedAttack.ApplyWallBuff(false);
        }
    }

}
