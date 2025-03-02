using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedAttack : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] InputActionReference _rangedAttackAction;
    [Header("STATS")]
    [SerializeField] AttackStats _stats;
    [Header("PROJECTILE")]
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _projectileSpawnPoint;

    private Color _debugColor = Color.red;

    #region ----- UNITY CALLBACKS -------
    void OnEnable()
    {
        _rangedAttackAction.action.Enable();
        _rangedAttackAction.action.performed += Ranged;
    }

    void OnDisable()
    {
        _rangedAttackAction.action.performed -= Ranged;
        _rangedAttackAction.action.Disable();
    }

    void Update()
    {
        if(_stats.DebugAttackArea)
            DebugAttack(_debugColor);
    }

    #endregion

    #region  ---- ATTACKS ------

    void Ranged(InputAction.CallbackContext context)
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
