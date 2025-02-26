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

    private Color _debugColor = Color.red;

    #region ----- UNITY CALLBACKS -------

    void OnEnable()
    {
        _meleeAttackAction.action.Enable();
        _meleeAttackAction.action.performed += Melee;
    }

    void OnDisable()
    {
        _meleeAttackAction.action.performed -= Melee;
        _meleeAttackAction.action.Disable();
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
            if(collider.transform.root == transform.root) 
            {
                _debugColor = Color.yellow;
                continue;
            }
            if(collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_stats.AttackDamage);
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
