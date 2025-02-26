using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackStats", menuName = "Player/AttackStats")]
public class AttackStats : ScriptableObject
{
    public float AttackDamage;
    public float MeleeAttackRange;
    public float RangedAttackRange;
    public float AttackHeight;
    public float AttackRate;
    public float KnockbackForce;
    public float AttackCooldown;
    [Header(" DEBUGGING ")]
    public bool DebugAttackArea;
}
