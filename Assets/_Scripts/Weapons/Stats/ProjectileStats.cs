using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Weapons/ProjectileStats")]
public class ProjectileStats : ScriptableObject
{
    public float MoveSpeed;
    public float AirAcceleration;
    public float Gravity;
    public float LifeTime;
    public float Damage;
    public float KnockbackForce;
}
