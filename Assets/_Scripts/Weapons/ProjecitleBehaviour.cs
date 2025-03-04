using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField] private ProjectileStats _stats;
    private Movement _movement;
    private Vector2 _direction;
    private Animator _animator;
    private bool _isMoving = true;

    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _movement = GetComponent<Movement>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if(!_isMoving) return;
        _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isMoving)
        {
            CheckForDamageHit(collision.transform);
            OnObstacleHit(collision.ClosestPoint(transform.position));
        }
        else 
        {
            CheckForPlayerPickup(collision.transform);
        }
       
    }

    /* private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForDamageHit(collision.transform);
        OnObstacleHit(collision.GetContact(0).point);
    }    */
    #endregion

    #region ---- PROJECTILE BEHAVIOUR ----

    public void Init(Vector2 direction)
    {
        _direction = direction;
    }

    private void OnObstacleHit(Vector3 hitPoint)
    {
        _isMoving = false;
        _movement.Stop();
        transform.position = hitPoint;
        _animator.enabled = false;  
    }

    private void CheckForDamageHit(Transform collider)
    {
       
        var damageableComponent = collider.GetComponentInParent<IDamageable>();
        if(damageableComponent != null)
        {
            damageableComponent.TakeDamage(_stats.Damage);
            Destroy(gameObject);
      
        }
    }

    private void CheckForPlayerPickup(Transform collider)
    {
        /* var playerPickupComponent = collider.GetComponentInParent<IPlayerPickup>();
        if(playerPickupComponent != null)
        {
            playerPickupComponent.Pickup();
            Destroy(gameObject);
        } */
        if(collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
