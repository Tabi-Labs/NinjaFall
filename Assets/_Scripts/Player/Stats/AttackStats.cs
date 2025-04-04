using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackStats", menuName = "Player/AttackStats")]
public class AttackStats : ScriptableObject
{
    public float AttackDamage;
    public float MeleeAttackRange;
    [SerializeField, Range(0,5)] public int MaxShurikens;
    public float RangedAttackRange;
    public float AttackHeight;
    public float AttackRate;
    public float KnockbackForce;
    [Range(0,5)] public float KnockbackTime = 0.15f;
    public float AttackCooldown;
    [Header("PARRY")]
    [Range(0,1)] public float ParryTime = 0.12f;
    [Range(0,1)] public float ParryWindow = 0.1f;
    [Header(" DEBUGGING ")]
    public bool DebugAttackArea;
}
