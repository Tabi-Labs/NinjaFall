using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private Dictionary<string, Coroutine> effectCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, StatusEffect> activeEffects = new Dictionary<string, StatusEffect>();


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
        }
    }

    public void ApplyStatusEffect(StatusEffect effect, GameObject player)
    {
        Debug.Log("Clase Effect Manager aplicando efecto");

        if (!effectCoroutines.ContainsKey(effect.EffectName))
        {

            effect.ApplyEffect(player);
            effect.StartVisualEffect(player);
            Coroutine coroutine = StartCoroutine(RemoveEffectAfterDuration(effect, player));
            effectCoroutines.Add(effect.EffectName, coroutine);
            activeEffects.Add(effect.EffectName, effect);
            Debug.Log("Manager: " + effectCoroutines);
        }

        
    }
    

    private IEnumerator RemoveEffectAfterDuration(StatusEffect effect, GameObject player)
    {
        yield return new WaitForSeconds(effect.Duration);
        Debug.Log("Clase Effect Manager eliminando efecto");
        RemoveEffect(effect, player);
    }

    private void RemoveEffect(StatusEffect effect, GameObject player)
    {

        effect.StopVisualEffect();
        effectCoroutines.Remove(effect.EffectName);
        activeEffects.Remove(effect.EffectName);
        effect.RemoveEffect(player);

    }
}
