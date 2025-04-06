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
    private bool _shurikenWallBuff = false;
    private float _gravityDebuff = 1.0f;

    private IDamageable _owner;
    private bool _canDamageOwner;
    private bool _shouldDamageOwner;
    private bool _runInvulnerabilityTimer = true;
    private float _invulnerabilityTimer;
    private float _gravityIgnoreTimer;
    private bool _isAffectedByGravity = false;

    private Collider2D currentObstacle;
    private bool isFollowingEdge = false;
    private Vector2 tangentDirection;
    private float timerFollowingEdge = 5.0f;

    public bool IsMoving => _isMoving;
    #region ---- UNITY CALLBACKS ----
    private void Awake()
    {
        _movement = GetComponent<Movement>();
        _animator = GetComponent<Animator>();

        _gravityIgnoreTimer = _stats.GravityIgnoreTime;
        _invulnerabilityTimer = _stats.OwnerInvulnerabilityTime;
    }

    private void Update()
    {
        OwnerInvulnerabilityTimer();   
        GravityIgnoreTimer();
    }
    private void FixedUpdate()
    {

        if (isFollowingEdge && currentObstacle != null)
        {
            Debug.Log("Obstaculo: " + currentObstacle.name);
            FollowEdge();

            if (timerFollowingEdge <= 0.0f)
            {
                StopShuriken();
            }
        }
        else
        {
            if (!_isMoving) return;
            _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
            if (_isAffectedByGravity)
                _movement.ApplyGravity(_stats.Gravity * _gravityDebuff, _stats.MaxFallSpeed);
            _movement.VerticalMove(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isMoving)
        {
            var damageableComponent = collision.GetComponentInParent<IDamageable>();
            if(!collision.isTrigger)
            {
                CheckForDamageHit(damageableComponent);
                if(damageableComponent != null && damageableComponent == _owner) return;
                OnObstacleHit(collision.ClosestPoint(transform.position), collision);
            }
            else if(damageableComponent != null)
                AutoAim(collision.transform, damageableComponent);
        }

       
    }

    #endregion

    #region ---- PROJECTILE BEHAVIOUR ----

    public void Init(Vector2 direction, IDamageable owner, bool canDamageOwner, bool wallBuff, float _gravityTimer, float gravity)
    {
        _direction = direction;
        _owner = owner;
        _canDamageOwner = canDamageOwner;
        _shurikenWallBuff = wallBuff;
        _gravityIgnoreTimer = _gravityTimer;
        _gravityDebuff = gravity;
    }


    private void OnObstacleHit(Vector3 hitPoint, Collider2D collision)
    {
        Debug.Log("Shuriken wall buff on hit: " + _shurikenWallBuff);
        AudioManager.PlaySound("FX_ShurikenHit");
        if (_shurikenWallBuff)
        {
            isFollowingEdge = true;
            StartFollowingEdge(collision);

        } else
        {
            transform.position = hitPoint;
            StopShuriken();
        }
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


    private void AutoAim(Transform target, IDamageable damageableComponent)
    {
        if(damageableComponent != null && damageableComponent == _owner) return;
        var directionToTarget = (target.position - transform.position).normalized;
        if(Vector2.Dot(_direction, directionToTarget) > 0)
            _direction = Vector2.Lerp(_direction, directionToTarget, _stats.RedirectionAcceleration * Time.deltaTime);
    }

    private void StartFollowingEdge(Collider2D obstacle)
    {
        currentObstacle = obstacle;
        isFollowingEdge = true;

        // Calcular direcci�n tangencial inicial
        Vector2 closestPoint = obstacle.ClosestPoint(transform.position);
        Vector2 surfaceNormal = ((Vector2)transform.position - closestPoint).normalized;
        tangentDirection = Vector2.Perpendicular(surfaceNormal);

        // Determinar direcci�n basada en la velocidad inicial
        float direction = Vector2.Dot(_direction.normalized, tangentDirection) > 0 ? 1 : -1;
        tangentDirection *= direction;

    }

    private void FollowEdge()
    {
        // Obtener punto m�s cercano en el obst�culo
        Vector2 closestPoint = currentObstacle.ClosestPoint(transform.position);

        // Calcular nueva normal y tangente
        Vector2 surfaceNormal = ((Vector2)transform.position - closestPoint).normalized;
        Vector2 desiredTangent = Vector2.Perpendicular(surfaceNormal);

        // Mantener la direcci�n original de rotaci�n
        float rotationDirection = Vector2.Dot(tangentDirection, desiredTangent) > 0 ? 1 : -1;
        desiredTangent *= rotationDirection;

        // Aplicar fuerza centr�peta
        Vector2 steerForce = desiredTangent * _stats.MoveSpeed - _direction;
        _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, steerForce);

        timerFollowingEdge -= Time.deltaTime;

    }

    private void StopShuriken()
    {
        _isMoving = false;
        isFollowingEdge = false;
        _movement.Stop();
        _animator.enabled = false;
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

    private void GravityIgnoreTimer()
    {
        _gravityIgnoreTimer -= Time.deltaTime;
        if(_gravityIgnoreTimer <= 0)
        {
            _isAffectedByGravity = true;
        }
    }
    #endregion
}
