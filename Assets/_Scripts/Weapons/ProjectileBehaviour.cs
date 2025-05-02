using DG.Tweening;
using System;
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
    private Transform[] pathPoints;

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
        if (!_isMoving || isFollowingEdge) return;

        _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, _direction);

        if (_isAffectedByGravity)
            _movement.ApplyGravity(_stats.Gravity * _gravityDebuff, _stats.MaxFallSpeed);

        _movement.VerticalMove(_stats.MoveSpeed, _stats.AirAcceleration, _direction);


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (_isMoving)
        {
            Debug.Log("Shuriken moviendose");
            var damageableComponent = collision.GetComponentInParent<IDamageable>();




            if ( collision.CompareTag("Shuriken"))
            {
                Debug.Log("Impacto en shuriken");
                _direction = new Vector2(0.0f, -1.0f);
                return;
            } 
            
            if (!collision.isTrigger)
            {
                Debug.Log("Impacto en un objeto que no tiene trigger activado");

                CheckForDamageHit(damageableComponent);

                OnObstacleHit(collision.ClosestPoint(transform.position), collision);

                return;
            }
            
            if (collision.isTrigger)
            {
                Debug.Log("Impacto en un objeto que tiene trigger activado");

                if (damageableComponent != null )
                {
                    Debug.Log("Intenta auto aim");
                    AutoAim(collision.transform, damageableComponent);
                }

                else if (pathPoints == null || pathPoints.Length == 0)
                {
                    pathPoints = GetPathPointsFromCollider(collision);

                    if(pathPoints.Length > 0)
                    {
                        OnObstacleHit(collision.ClosestPoint(transform.position), collision);
                    }
                   
                }
                return;
            }        
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

    public void ReflectShuriken(Vector2 newDirection)
    {
        if(newDirection == Vector2.zero)
        {
            newDirection.x = -_direction.x;
            newDirection.y = 0.0f;
        }

        Debug.Log("Nueva direccion shuriken" + newDirection);
        _direction = newDirection;
    }


    private void OnObstacleHit(Vector3 hitPoint, Collider2D collision)
    {
        Debug.Log("Shuriken wall buff on hit: " + _shurikenWallBuff);
        AudioManager.PlaySound("FX_ShurikenHit");
        if (_shurikenWallBuff)
        {
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



    private void FollowPath()
    {
        isFollowingEdge = true;
        _movement.Stop();
        _animator.enabled = true;

        Vector3[] path = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
            path[i] = pathPoints[i].position;

        transform.DOPath(path, _stats.MoveSpeed, PathType.CatmullRom, PathMode.TopDown2D)
                 .SetEase(Ease.Linear)
                 .OnComplete(() => StopShuriken());
    }

    private Transform[] GetPathPointsFromCollider(Collider2D collision)
    {
        int layer = LayerMask.NameToLayer("Path Point");
        List<Transform> pathPoints = new List<Transform>();

        foreach (Transform child in collision.transform)
        {
            Debug.Log("Child layer: " + child.gameObject.layer);
            Debug.Log("Layer: " + layer);
            if (child.gameObject.layer == layer)
            {
                pathPoints.Add(child);
            }
        }

        // Si no hay puntos en el camino, retornamos un array vacío
        if (pathPoints.Count == 0) return new Transform[0];

        // Empezamos por el punto más cercano al shuriken
        List<Transform> orderedPathPoints = new List<Transform>();
        Transform currentPoint = FindNearestPoint(this.transform, pathPoints);
        orderedPathPoints.Add(currentPoint);

        // Ordenar los puntos por proximidad, buscando el más cercano al último punto agregado
        while (pathPoints.Count > 0)
        {
            // Encontramos el punto más cercano al último punto agregado
            Transform nearestPoint = FindNearestPoint(currentPoint, pathPoints);
            orderedPathPoints.Add(nearestPoint);
            pathPoints.Remove(nearestPoint);
            currentPoint = nearestPoint;
        }

        return orderedPathPoints.ToArray();
    }

    private Transform FindNearestPoint(Transform fromPoint, List<Transform> points)
    {
        Transform nearest = points[0];
        float shortestDistance = Vector2.Distance(fromPoint.position, nearest.position);

        // Recorremos todos los puntos para encontrar el más cercano
        foreach (Transform point in points)
        {
            float distance = Vector2.Distance(fromPoint.position, point.position);
            if (distance < shortestDistance)
            {
                nearest = point;
                shortestDistance = distance;
            }
        }

        return nearest;
    }

    private void StartFollowingEdge(Collider2D obstacle)
    {
        if (pathPoints != null && pathPoints.Length > 0)
        {
            if (isFollowingEdge)
            {
                return;
            }
            
            FollowPath();
        }
        else
        {
            Debug.LogWarning("No se asignó un path al shuriken.");
            transform.position = obstacle.ClosestPoint(transform.position);
            StopShuriken();
        }
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
