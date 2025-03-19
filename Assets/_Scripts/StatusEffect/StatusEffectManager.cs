using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private Dictionary<StatusEffect, Coroutine> effectCoroutines = new Dictionary<StatusEffect, Coroutine>();

    public void ApplyStatusEffect(StatusEffect effect, GameObject obj)
    {
        Debug.Log("Clase Effect Manager aplicando efecto");

        if (effectCoroutines.ContainsKey(effect))
        {
            StopCoroutine(effectCoroutines[effect]);
            effectCoroutines.Remove(effect);
        }

        effect.ApplyEffect(obj);
        Coroutine coroutine = StartCoroutine(RemoveEffectAfterDuration(effect, obj));
        effectCoroutines.Add(effect, coroutine);
    }

    private IEnumerator RemoveEffectAfterDuration(StatusEffect effect, GameObject obj)
    {
        yield return new WaitForSeconds(effect.Duration);
        Debug.Log("Clase Effect Manager eliminando efecto");
        effect.RemoveEffect(obj);
        effectCoroutines.Remove(effect);
    }
}
