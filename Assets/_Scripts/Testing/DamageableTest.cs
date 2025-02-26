using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableTest : MonoBehaviour, IDamageable
{
    public void TakeDamage(AttackStats attackStats)
    {
        Debug.Log($"{gameObject.name} took {attackStats.AttackDamage} damage");
    }
}
