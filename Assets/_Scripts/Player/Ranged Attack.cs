using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedAttack : NetworkBehaviour
{
    private Player _player;
    private IDamageable _selfDamageable;
    [Header("STATS")]
    [SerializeField] ProjectileStats _projectileStats;
    [SerializeField] AttackStats _attackStats;
    [Header("PROJECTILE")]
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _projectileSpawnPoint;

    private Color _debugColor = Color.red;

    #region ----- UNITY CALLBACKS -------

    void Awake()
    {
        _player = GetComponent<Player>();
        _selfDamageable = GetComponent<IDamageable>();
    }

    void Start()
    {
        _player.Input().RangedAttackEvent += OnRangedAttack;
    }

    void OnDisable()
    {
        _player.Input().RangedAttackEvent -= OnRangedAttack;
    }

    void Update()
    {
        if(_attackStats.DebugAttackArea)
            DebugAttack(_debugColor);
    }

    #endregion

    #region  ---- ATTACKS ------

    void OnRangedAttack()
    {
        if (NetworkManager)
        {
            if (!IsOwner)
            {
                return;
            }
            RequestSpawnProjectileRPC();
        }
        else
        {
            var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileBehaviour>().Init(transform.right, _selfDamageable, true);
        }

        //projectile.GetComponent<Projectile>().Init(_stats);
    }

    public void ApplyGravity(float newGravity)
    {
        Debug.Log("Clase Ranged Attack");
        _projectileStats.Gravity = newGravity;
    }

    [Rpc(SendTo.Server)]
    void RequestSpawnProjectileRPC()
    {
        var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBehaviour>().Init(transform.right, _selfDamageable, true);
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
    void DebugAttack(Color color)
    {
        Debug.DrawRay(_projectileSpawnPoint.position, transform.right * _attackStats.RangedAttackRange, color);
    }

    #endregion
}
