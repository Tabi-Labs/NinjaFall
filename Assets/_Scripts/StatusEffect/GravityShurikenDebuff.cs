using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityShurikenDebuff : StatusEffect

{
    public override void ApplyEffect(GameObject player)
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerList in players)
        {
            Debug.Log("Jugador enemigo: " + playerList);
            Debug.Log("Juagdor actual" + player);

            var damageableComponent = playerList.GetComponent<IDamageable>();

            if (damageableComponent != null && playerList != player)
            {
                RangedAttack rangedAttack = playerList.GetComponent<RangedAttack>();

                if (rangedAttack != null)
                {
                    Debug.Log("Se aplica gravedad");
                    rangedAttack.ApplyGravity(90.0f);
                }
            }
        }

        
    }

    public override void RemoveEffect(GameObject player)
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerList in players)
        {
            Debug.Log("Jugador enemigo: " + playerList);

            Debug.Log("Juagdor actual" + player);

            var damageableComponent = playerList.GetComponent<IDamageable>();

            if (damageableComponent != null && playerList != player)
            {
                RangedAttack rangedAttack = playerList.GetComponent<RangedAttack>();

                if (rangedAttack != null)
                {
                    Debug.Log("Se aplica gravedad");
                    rangedAttack.ApplyGravity(1.0f);
                }
            }
        }

        Debug.Log("Eliminando efecto NUEVO");
        
    }

}
