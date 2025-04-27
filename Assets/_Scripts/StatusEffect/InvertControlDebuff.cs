using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertControlDebuff : StatusEffect
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
                Player enemyPlayer = playerList.GetComponentInParent<Player>();
                if (enemyPlayer != null)
                {
                    enemyPlayer.SetInvertControlDebuff(true);
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
                Player enemyPlayer = playerList.GetComponentInParent<Player>();
                if (enemyPlayer != null)
                {
                    enemyPlayer.SetInvertControlDebuff(false);
                }
            }
        }
        Debug.Log("Eliminando efecto NUEVO");
        Destroy(gameObject);
    }

}
