using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityShurikenDebuff : StatusEffect

{
    public override void ApplyEffect(GameObject obj)
    {
        RangedAttack rangedAttack = obj.GetComponent<RangedAttack>();

        Debug.Log("Se intenta aplicar gravedad");

        if (rangedAttack != null)
        {
            Debug.Log("Se aplica gravedad");
            rangedAttack.ApplyGravity(15.0f);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            Debug.Log("Aplicando efecto");
            player.ApplyEffect(this);
            Destroy(gameObject);
        }
    }

}
