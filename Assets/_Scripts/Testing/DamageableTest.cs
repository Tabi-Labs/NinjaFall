using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableTest : MonoBehaviour, IDamageable
{
    public void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} took {damage} damage");
    }
}
