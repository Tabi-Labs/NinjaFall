using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class MeleeAttack : MonoBehaviour
{
    private Player _player;
    [Header("STATS")]
    [SerializeField] AttackStats _stats;
    [Header("CHECKS")]
    [Tooltip("If set to True it will search for its IDamageable Component to exclude it from self-harming"), SerializeField] 
    bool _isDamageable;
    private IDamageable _selfDamageable;
    private Color _debugColor = Color.red;

    private Vector3 hitPoint;


    #region ----- UNITY CALLBACKS -------

    void Awake()
    {
        if(_isDamageable) _selfDamageable = GetComponentInParent<IDamageable>();   
        _player = GetComponent<Player>();
    }
    void Start()
    {
        _player.Input().MeleeAttackEvent += Melee;
        //_meleeAttackAction.action.performed += Melee;
    }

    void OnDisable()
    {
        _player.Input().MeleeAttackEvent -= Melee;
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

    void Melee()
    {
        hitPoint = transform.position + transform.right * _stats.MeleeAttackRange / 2f;
        var boxSize = new Vector2(_stats.MeleeAttackRange, _stats.AttackHeight);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(hitPoint, boxSize, 0f);

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
                DG.Tweening.Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(_stats.ParryWindow);
                sequence.AppendCallback(() =>
                {
                if(!damageableComponent.IsParrying())
                {
                    damageableComponent.TakeDamage(_stats.AttackDamage);
                    _debugColor = Color.green;
                }
                else
                {
                    OnMeleeAttackClash(collider.transform);
                }
                });
            
            }

            if (collider.CompareTag("Shuriken"))
            {
                ProjectileBehaviour projectileBehaviour = collider.GetComponentInParent<ProjectileBehaviour>();

                if (projectileBehaviour != null)
                {
                    projectileBehaviour.ReflectShuriken(_player.GetMovement());
                }
            }
        }
    }

    void OnMeleeAttackClash(Transform clashTransform)
    {
        var knockbackDir = -(clashTransform.position - transform.position).normalized;
        knockbackDir *= _stats.KnockbackForce;
        _player.Movement.Impulse(knockbackDir, _stats.KnockbackTime);

        ClashEffects();
    }
    
    void ClashEffects()
    {
        if(AudioManager.Instance) AudioManager.PlaySound("FX_SwordClash");
        VFXManager.PlayVFX("VFX_Impact",VFXType.Animation, hitPoint, Quaternion.identity);
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
