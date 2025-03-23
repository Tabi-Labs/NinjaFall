using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
    public string EffectName;
    public float Duration;

    public abstract void ApplyEffect(GameObject player);
    public abstract void RemoveEffect(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null && other.CompareTag("Player"))
        {
            Debug.Log("Aplicando efecto");
            player.ApplyEffect(this);
            Destroy(gameObject);
        }
    }

}
