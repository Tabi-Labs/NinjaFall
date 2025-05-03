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
    [Header("EFFECTS")]
    [SerializeField] Transform _vfx;
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

    void Update()
    {
        if(_stats.DebugAttackArea)
            DebugAttack(_debugColor);
    }
    #endregion

    #region  ---- ATTACKS ------

    void Melee()
    {
        Vector2 mov = _player.GetMovement();

        //hitPoint = transform.position + transform.right * _stats.MeleeAttackRange / 2f;
        hitPoint = transform.position + (Vector3)mov.normalized * _stats.MeleeAttackRange / 2f;
        var boxSize = new Vector2(_stats.MeleeAttackRange, _stats.AttackHeight);
        float angle = Mathf.Atan2(mov.y, mov.x) * Mathf.Rad2Deg;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(hitPoint, boxSize, angle);

        if(mov.magnitude == 0)
        {
            _vfx.rotation = transform.rotation;
        }
        else
        {
            _vfx.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        //_vfx.rotation = Quaternion.identity;
        //_vfx.Rotate(0f, 0f, angle);

        Debug.DrawLine(transform.position, hitPoint, Color.blue, 1f);
        DrawDebugBox(hitPoint, boxSize, angle, Color.magenta, 1f);

        if(colliders.Length == 0 || _player.CheckIsDead()) 
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

    void DrawDebugBox(Vector2 center, Vector2 size, float angle, Color color, float duration)
    {
        // Create a rotation matrix
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Vector2[] corners = new Vector2[4];

        // Calculate local corners
        Vector2 halfSize = size / 2f;
        corners[0] = new Vector2(-halfSize.x, -halfSize.y);
        corners[1] = new Vector2(halfSize.x, -halfSize.y);
        corners[2] = new Vector2(halfSize.x, halfSize.y);
        corners[3] = new Vector2(-halfSize.x, halfSize.y);

        // Rotate and position the corners
        for (int i = 0; i < 4; i++)
        {
            corners[i] = rotation * corners[i];
            corners[i] += center;
        }

        // Draw the box
        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[1], corners[2], color, duration);
        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[3], corners[0], color, duration);
    }
    #endregion

}
