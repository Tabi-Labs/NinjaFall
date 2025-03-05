using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Weapons/ProjectileStats")]
public class ProjectileStats : ScriptableObject
{
    [Header("MOVEMENT PARAMETERS")]
    public float MoveSpeed;
    public float AirAcceleration;
    public float RedirectionAcceleration = 10f;
    public float MaxFallSpeed = -10f;
    public float Gravity;
    public float GravityIgnoreTime = 0.4f;
    [Header("DAMAGE PARAMETERS")]
    public float Damage;
    [Header("INTERACTIONS")]
    public float OwnerInvulnerabilityTime = 0.5f;
}
