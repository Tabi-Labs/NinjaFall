using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedAttack : MonoBehaviour
{
    private Player _player;
    [Header("STATS")]
    [SerializeField] AttackStats _stats;
    [Header("PROJECTILE")]
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _projectileSpawnPoint;

    private Color _debugColor = Color.red;

    #region ----- UNITY CALLBACKS -------

    void Awake()
    {
        _player = GetComponent<Player>();
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
        if(_stats.DebugAttackArea)
            DebugAttack(_debugColor);
    }

    #endregion

    #region  ---- ATTACKS ------

    void OnRangedAttack()
    {
        var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBehaviour>().Init(transform.right);
        //projectile.GetComponent<Projectile>().Init(_stats);
    }

    void DebugAttack(Color color)
    {
        Debug.DrawRay(_projectileSpawnPoint.position, transform.right * _stats.RangedAttackRange, color);
    }

    #endregion
}
