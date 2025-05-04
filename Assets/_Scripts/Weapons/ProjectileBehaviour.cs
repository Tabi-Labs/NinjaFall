using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
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
    private bool _canDamage = true;
    private Vector3 _hitSurfaceNormal;
    private bool isFollowingEdge = false;
    private Transform[] pathPoints;


    private bool _collided = false;
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
        if(!_isMoving) return;
        OwnerInvulnerabilityTimer();   
        GravityIgnoreTimer();
        RotateShuriken();
    }

    private void FixedUpdate()
    {
        if (!NetworkManager || IsServer)
        {
            
            if (_isAffectedByGravity)
                _movement.ApplyGravity(_stats.Gravity * _gravityDebuff, _stats.MaxFallSpeed);

            if (!_isMoving || isFollowingEdge || _collided) return;
            _movement.Move(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
            _movement.VerticalMove(_stats.MoveSpeed, _stats.AirAcceleration, _direction);
        }
        


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // En modo red, solo el servidor procesa colisiones para evitar duplicados
        if (!NetworkManager || IsServer)
        {
            if( collision.CompareTag("Shuriken"))
            {
                var incomingShuriken = collision.GetComponent<ProjectileBehaviour>();
                if(_isMoving && incomingShuriken.IsMoving)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, _stats.MoveSpeed);
                    if(hit)
                        _hitSurfaceNormal = hit.normal;
                    _direction = Vector2.Reflect(_direction, _hitSurfaceNormal); // Reflect the shuriken
                    _movement.Impulse(_direction * _stats.MoveSpeed,  0.5f);
                }
            } 

            var damageableComponent = collision.GetComponentInParent<IDamageable>();
            
            
            if(!collision.isTrigger && !CheckForDamageHit(damageableComponent))
            {
                if (pathPoints == null || pathPoints.Length == 0)
                {
                    pathPoints = GetPathPointsFromCollider(collision);

                }
                
                OnObstacleHit(collision.ClosestPoint(transform.position), collision);
            }
                

            if (collision.isTrigger)
            {
                Debug.Log("Impacto en un objeto que tiene trigger activado");

                if (_shouldDamageOwner && damageableComponent != null )
                {
                    Debug.Log("Intenta auto aim");
                    AutoAim(collision.transform, damageableComponent);
                }

                if (pathPoints == null || pathPoints.Length == 0)
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
    // Método para inicializar el proyectil cuando se crea
    public void Init(Vector2 direction, IDamageable owner, bool canDamageOwner, bool wallBuff, float gravityTimer, float gravity)
    {
        _direction = direction;
        _owner = owner;
        _canDamageOwner = canDamageOwner;
        _shurikenWallBuff = wallBuff;
        _gravityIgnoreTimer = gravityTimer;
        _gravityDebuff = gravity;
        _canDamage = true;
        _collided = false;

        // Si estamos en red y somos el servidor, sincronizar con los clientes
        if (NetworkManager && IsServer)
        {
            InitClientRpc(direction, wallBuff, gravityTimer, gravity);
        }
    }

    [ClientRpc]
    private void InitClientRpc(Vector2 direction, bool wallBuff, float gravityTimer, float gravity)
    {
        // No actualizamos en el servidor, ya que ya tiene los valores correctos
        if (!IsServer)
        {
            _direction = direction;
            _shurikenWallBuff = wallBuff;
            _gravityIgnoreTimer = gravityTimer;
            _gravityDebuff = gravity;
        }
    }

    // Método para reflejar el shuriken cuando rebota
    public void ReflectShuriken(Vector2 newDirection)
    {
        if (newDirection == Vector2.zero)
        {
            newDirection.x = -_direction.x;
            newDirection.y = 0.0f;
        }

        // Solo el servidor cambia la dirección en red
        if (!NetworkManager || IsServer)
        {
            _direction = newDirection;

            // Si estamos en red y somos el servidor, sincronizar con los clientes
            if (NetworkManager)
            {
                ReflectShurikenClientRpc(newDirection);
            }
        }
        else if (NetworkManager)
        {
            // Si somos cliente, solicitamos al servidor cambiar la dirección
            ReflectShurikenServerRpc(newDirection);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReflectShurikenServerRpc(Vector2 newDirection)
    {
        _direction = newDirection;

        // Propagar el cambio a todos los clientes
        ReflectShurikenClientRpc(newDirection);
    }

    [ClientRpc]
    private void ReflectShurikenClientRpc(Vector2 newDirection)
    {
        // No actualizamos en el servidor, ya que ya tiene la dirección correcta
        if (!IsServer)
        {
            _direction = newDirection;
        }
    }

    private void OnObstacleHit(Vector3 hitPoint, Collider2D collision)
    {
        if(!_isMoving) return;
        AudioManager.PlaySound("FX_ShurikenHit");
        
        if (_shurikenWallBuff)
        {
            StartFollowingEdge(collision);

            // Sincronizar con los clientes que el shuriken está siguiendo un borde
            if (NetworkManager && IsServer)
            {
                StartFollowingEdgeClientRpc(hitPoint);
            }
        }
        else
        {
            transform.position = hitPoint;
            StopShuriken();

            // Sincronizar con los clientes que el shuriken se ha detenido
            if (NetworkManager && IsServer)
            {
                StopShurikenClientRpc(hitPoint);
            }
        }
    }
    private void RotateShuriken()
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    [ClientRpc]
    private void StartFollowingEdgeClientRpc(Vector3 hitPoint)
    {
        if (!IsServer)
        {
            // Marcar que está siguiendo el borde, pero el movimiento lo controla el servidor
            isFollowingEdge = true;
        }
    }

    [ClientRpc]
    private void StopShurikenClientRpc(Vector3 hitPoint)
    {
        if (!IsServer)
        {
           
            transform.position = hitPoint;
            StopShuriken();
        }
    }

    private bool CheckForDamageHit(IDamageable damageableComponent)
    {
        if (damageableComponent != null)
        {
            if(_owner != null && damageableComponent == _owner && !_shouldDamageOwner) return true;
            if(_canDamage && !_collided)
            {
                damageableComponent.TakeDamage(_stats.Damage);

                if (!isFollowingEdge)
                {
                    DisableDamage();
                    _collided = true;
                    _isAffectedByGravity = true;
                    _movement.StopX();
                    return true;
                }
                
            }
        }
        return false;
    }

    private void DestroyProjectile()
    {
        if (NetworkManager)
        {
            if (IsServer)
            {
                // Si somos servidor, despachamos directamente
                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                // Si somos cliente, solicitamos al servidor que despache
                RequestDespawnServerRpc();
            }
        }
        else
        {
            // Si no estamos en red, destruimos localmente
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    private void AutoAim(Transform target, IDamageable damageableComponent)
    {
        if (damageableComponent != null && damageableComponent == _owner) return;
        var directionToTarget = (target.position - transform.position).normalized;
        if (Vector2.Dot(_direction, directionToTarget) > 0)
        {
            _direction = Vector2.Lerp(_direction, directionToTarget, _stats.RedirectionAcceleration * Time.deltaTime);

            // Sincronizar la nueva dirección después del auto-aim
            if (NetworkManager && IsServer)
            {
                AutoAimClientRpc(_direction);
            }
        }
    }

    [ClientRpc]
    private void AutoAimClientRpc(Vector2 newDirection)
    {
        if (!IsServer)
        {
            _direction = newDirection;
        }
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



    private void DisableDamage() => _canDamage = false;
    private void StopShuriken()
    {
        _isMoving = false;
        _isAffectedByGravity = false;
        _collided = true;
        isFollowingEdge = false;
        _movement.Stop();
        _animator.enabled = false;
    }
    #endregion

    #region ---- TIMERS ----
    private void OwnerInvulnerabilityTimer()
    {
        if (!_runInvulnerabilityTimer) return;
        if (!_canDamageOwner) return;

        _invulnerabilityTimer -= Time.deltaTime;

        if (_invulnerabilityTimer <= 0)
        {
            _runInvulnerabilityTimer = false;
            _shouldDamageOwner = true;
        }
    }

    private void GravityIgnoreTimer()
    {
        _gravityIgnoreTimer -= Time.deltaTime;
        if (_gravityIgnoreTimer <= 0)
        {
            _isAffectedByGravity = true;
        }
    }
    #endregion
}
