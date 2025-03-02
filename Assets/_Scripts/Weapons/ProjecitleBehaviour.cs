using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField] private ProjectileStats _stats;
    private Movement _movement;
    private Vector2 _direction;

    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _movement = GetComponent<Movement>();
    }

    private void FixedUpdate()
    {
        _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damageableComponent = collision.GetComponentInParent<IDamageable>();
        if(damageableComponent != null)
        {
            damageableComponent.TakeDamage(_stats.Damage);
        
      
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damageableComponent = collision.transform.GetComponentInParent<IDamageable>();
         if(damageableComponent != null)
        {
            damageableComponent.TakeDamage(_stats.Damage);
            Destroy(gameObject);
        }
    }
    #endregion

    #region ---- PROJECTILE BEHAVIOUR ----

    public void Init(Vector2 direction)
    {
        _direction = direction;
    }
    #endregion
}
