using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAttack : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] InputActionReference _meleeAttackAction;
    [Header("STATS")]
    [SerializeField] AttackStats _stats;
    [Header("CHECKS")]
    [Tooltip("If set to True it will search for its IDamageable Component to exclude it from self-harming"), SerializeField] 
    bool _isDamageable;
    private IDamageable _selfDamageable;
    private Color _debugColor = Color.red;

    #region ----- UNITY CALLBACKS -------

    void Awake()
    {
        if(_isDamageable) _selfDamageable = GetComponentInParent<IDamageable>();   
    }
    void OnEnable()
    {
        _meleeAttackAction.action.Enable();
        _meleeAttackAction.action.performed += Melee;
    }

    void OnDisable()
    {
        //_meleeAttackAction.action.performed -= Melee;
        //_meleeAttackAction.action.Disable();
    }

    void Update()
    {
        if(_stats.DebugAttackArea)
            DebugAttack(_debugColor);
    }
    #endregion

    #region  ---- ATTACKS ------

    void Melee(InputAction.CallbackContext context)
    {
        Debug.Log($"MELEE PERFORMED BY {gameObject.name}");
        var boxCenter = transform.position + transform.right * _stats.MeleeAttackRange / 2f;
        var boxSize = new Vector2(_stats.MeleeAttackRange, _stats.AttackHeight);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        if(colliders.Length == 0) 
        {
            _debugColor = Color.red;
            return;
        }

        foreach(Collider2D collider in colliders)
        { 
            var damageableComponent = collider.GetComponentInParent<IDamageable>();
            if(damageableComponent != null)
            {
                if(_isDamageable && damageableComponent == _selfDamageable)
                {
                    _debugColor = Color.yellow;
                    continue;
                }
                damageableComponent.TakeDamage(_stats.AttackDamage);
                _debugColor = Color.green;
            }
        }
    }

    void OnMeleeAttackClash()
    {
        //TODO implement clash
    }
 
    #endregion

    #region ---- DEBUG ------
    void DebugAttack(Color debugColor)
    {
        var boxUpperLeftCorner = transform.position + transform.up * _stats.AttackHeight/2f ;
        var boxUpperRightCorner = transform.position + transform.up * _stats.AttackHeight/2f + transform.right * _stats.MeleeAttackRange;
        var boxLowerLeftCorner = transform.position - transform.up * _stats.AttackHeight/2f;
        var boxLowerRightCorner = transform.position + transform.right * _stats.MeleeAttackRange - transform.up * _stats.AttackHeight/2f;
        Debug.DrawLine(boxUpperLeftCorner, boxUpperRightCorner, debugColor);
        Debug.DrawLine(boxLowerLeftCorner, boxLowerRightCorner, debugColor);
        Debug.DrawLine(boxLowerLeftCorner, boxUpperLeftCorner, debugColor);
        Debug.DrawLine(boxLowerRightCorner, boxUpperRightCorner, debugColor);
    }
    #endregion
    
}
