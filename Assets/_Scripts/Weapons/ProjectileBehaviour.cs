using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        // En modo red, solo el servidor controla el movimiento
        // El ClientNetworkTransform se encargará de sincronizar la posición
        if (!NetworkManager || IsServer)
        {
            if (isFollowingEdge && currentObstacle != null)
            {
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // En modo red, solo el servidor procesa colisiones para evitar duplicados
        if (!NetworkManager || IsServer)
        {
            if (_isMoving)
            {
                var damageableComponent = collision.GetComponentInParent<IDamageable>();

                if (collision.CompareTag("Shuriken"))
                {
                    _direction = new Vector2(0.0f, -1.0f);

                    // Notificar a los clientes del cambio de dirección
                    if (NetworkManager && IsServer)
                    {
                        ReflectShurikenClientRpc(_direction);
                    }
                }
                else if (!collision.isTrigger)
                {
                    CheckForDamageHit(damageableComponent);
                    if (damageableComponent != null && damageableComponent == _owner) return;
                    OnObstacleHit(collision.ClosestPoint(transform.position), collision);
                }
                else if (damageableComponent != null)
                    AutoAim(collision.transform, damageableComponent);
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
        AudioManager.PlaySound("FX_ShurikenHit");
        if (_shurikenWallBuff)
        {
            isFollowingEdge = true;
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

    private void CheckForDamageHit(IDamageable damageableComponent)
    {
        if (damageableComponent != null)
        {
            if (_owner != null && damageableComponent == _owner && !_shouldDamageOwner) return;
            damageableComponent.TakeDamage(_stats.Damage);
            DestroyProjectile();
        }
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

    private void StartFollowingEdge(Collider2D obstacle)
    {
        currentObstacle = obstacle;
        isFollowingEdge = true;

        // Calcular dirección tangencial inicial
        Vector2 closestPoint = obstacle.ClosestPoint(transform.position);
        Vector2 surfaceNormal = ((Vector2)transform.position - closestPoint).normalized;
        tangentDirection = Vector2.Perpendicular(surfaceNormal);

        // Determinar dirección basada en la velocidad inicial
        float direction = Vector2.Dot(_direction.normalized, tangentDirection) > 0 ? 1 : -1;
        tangentDirection *= direction;
    }

    private void FollowEdge()
    {
        // Obtener punto más cercano en el obstáculo
        Vector2 closestPoint = currentObstacle.ClosestPoint(transform.position);

        // Calcular nueva normal y tangente
        Vector2 surfaceNormal = ((Vector2)transform.position - closestPoint).normalized;
        Vector2 desiredTangent = Vector2.Perpendicular(surfaceNormal);

        // Mantener la dirección original de rotación
        float rotationDirection = Vector2.Dot(tangentDirection, desiredTangent) > 0 ? 1 : -1;
        desiredTangent *= rotationDirection;

        // Aplicar fuerza centrípeta
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
