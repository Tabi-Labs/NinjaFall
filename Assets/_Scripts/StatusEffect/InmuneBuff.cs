using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InmuneBuff : StatusEffect
{
    private GameObject playerInstance;

    private void Update()
    {

        if (playerInstance != null)
        {
            IDamageable damageable = playerInstance.GetComponent<IDamageable>();
            Debug.Log("Intentando eliminar efecto visual: " + damageable);
            if (damageable != null && !damageable.IsInmune())
            {
                Debug.Log("Eliminado efecto visual " + damageable.IsInmune());
                StopVisualEffect();
                SetActive(false);
            }

        }
    }

    public override void ApplyEffect(GameObject player)
    {
        IDamageable damageable = player.GetComponent<IDamageable>();
        playerInstance = player;

        Debug.Log("Instancia del jugador en el buff: " + playerInstance);
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
        Debug.Log("Eliminando efecto NUEVO");
        
    }
}
