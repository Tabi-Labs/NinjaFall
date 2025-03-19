using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
    public string EffectName;
    public float Duration;

    public abstract void ApplyEffect(GameObject obj);
    public abstract void RemoveEffect(GameObject obj);


}
