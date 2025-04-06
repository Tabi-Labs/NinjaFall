using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private Dictionary<StatusEffect, Coroutine> effectCoroutines = new Dictionary<StatusEffect, Coroutine>();


    // Singleton Pattern
    // --------------------------------------------------------------------------------
    public static StatusEffectManager instance { get; private set; }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ApplyStatusEffect(StatusEffect effect, GameObject player)
    {
        Debug.Log("Clase Effect Manager aplicando efecto");

        if (effectCoroutines.ContainsKey(effect))
        {
            StopCoroutine(effectCoroutines[effect]);
            effectCoroutines.Remove(effect);
            activeEffects.Remove(effect);
        }

        effect.ApplyEffect(player);
        activeEffects.Add(effect);
        Coroutine coroutine = StartCoroutine(RemoveEffectAfterDuration(effect, player));
        effectCoroutines.Add(effect, coroutine);
    }
    

    private IEnumerator RemoveEffectAfterDuration(StatusEffect effect, GameObject player)
    {
        yield return new WaitForSeconds(effect.Duration);
        Debug.Log("Clase Effect Manager eliminando efecto");
        effect.RemoveEffect(player);
        effectCoroutines.Remove(effect);
        activeEffects.Remove(effect);
    }
}
