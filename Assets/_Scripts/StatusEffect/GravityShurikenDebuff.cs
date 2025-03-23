using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityShurikenDebuff : StatusEffect

{
    public override void ApplyEffect(GameObject player)
    {
        RangedAttack rangedAttack = player.GetComponent<RangedAttack>();

        Debug.Log("Se intenta aplicar gravedad");

        if (rangedAttack != null)
        {
            Debug.Log("Se aplica gravedad");
            rangedAttack.ApplyGravity(90.0f);
        }
    }

    public override void RemoveEffect(GameObject obj)
    {
        RangedAttack rangedAttack = obj.GetComponent<RangedAttack>();

        Debug.Log("Se intenta eliminar debuff gravedad");

        if (rangedAttack != null)
        {
            Debug.Log("Se elimmina debuff gravedad");
            rangedAttack.ApplyGravity(9.8f);
        }
    }

}
