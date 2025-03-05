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

    private IDamageable _owner;
    private bool _canDamageOwner;
    private bool _shouldDamageOwner;
    private bool _runInvulnerabilityTimer = true;
    private float _invulnerabilityTimer;
   

    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _movement = GetComponent<Movement>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        OwnerInvulnerabilityTimer();   
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
            var damageableComponent = collision.GetComponentInParent<IDamageable>();
            CheckForDamageHit(damageableComponent);
            if(damageableComponent != null && damageableComponent == _owner) return;
            OnObstacleHit(collision.ClosestPoint(transform.position));
        }
        else 
        {
            CheckForPlayerPickup(collision.transform);
        }
       
    }

    #endregion

    #region ---- PROJECTILE BEHAVIOUR ----

    public void Init(Vector2 direction, IDamageable owner, bool canDamageOwner)
    {
        _direction = direction;
        _owner = owner;
        _canDamageOwner = canDamageOwner;
        _invulnerabilityTimer = _stats.OwnerInvulnerabilityTime;
    }

    private void OnObstacleHit(Vector3 hitPoint)
    {
        _isMoving = false;
        _movement.Stop();
        transform.position = hitPoint;
        _animator.enabled = false;  
    }

    private void CheckForDamageHit(IDamageable damageableComponent)
    {
        if(damageableComponent != null)
        {
            if(_owner != null && damageableComponent == _owner && !_shouldDamageOwner) return;
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
            Debug.Log("Reco");
            Destroy(gameObject);
        }
    }
    #endregion

    #region ---- TIMERS ----

    private void OwnerInvulnerabilityTimer()
    {
        if(!_runInvulnerabilityTimer) return;
        if(!_canDamageOwner) return;

        _invulnerabilityTimer -= Time.deltaTime;

        if(_invulnerabilityTimer <= 0)
        {
            _runInvulnerabilityTimer = false;
            _shouldDamageOwner = true;
        }
            
    }
    #endregion
}
