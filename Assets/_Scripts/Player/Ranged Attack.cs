using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedAttack : NetworkBehaviour
{
    private Player _player;
    private IDamageable _selfDamageable;
    [Header("STATS")]
    [SerializeField] AttackStats _attackStats;
    [Header("PROJECTILE")]
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _projectileSpawnPoint;

    private bool wallBuff = false;
    private float gravityDebuff = 1.0f;
    private float gravityIgnoreTimer = 0.4f;
    public int ShurikensCount { get; private set; }
    public int ShurikensThrown{get;private set;}
    public GameEvent shurikenAmmoEvent;
    private Color _debugColor = Color.red;

    Vector2 aimDirection = default;

    #region ----- UNITY CALLBACKS -------

    void Awake()
    {
        _player = GetComponent<Player>();
        _selfDamageable = GetComponent<IDamageable>();
    }

    void Start()
    {
        _player.Input().RangedAttackEvent += OnRangedAttack;
    }

    void Update()
    {
        if (_attackStats.DebugAttackArea)
            DebugAttack(_debugColor);

        if (_player.Input().IsAiming)
        {
            AimShuriken();
        }
    }

    #endregion

    #region  ---- ATTACKS ------

    void AimShuriken()
    {
        aimDirection = _player.Input().AimMovement;

        if (aimDirection.SqrMagnitude() > 3f)
        {
            aimDirection = Camera.main.ScreenToWorldPoint(aimDirection) - _projectileSpawnPoint.position;
            aimDirection.Normalize();
        }
    }

    void OnRangedAttack()
    {
        // Validaci�n com�n para todos los modos
        if (_player.IsWallSliding && _player.IsWallSlideFalling) return;
        if (ShurikensThrown >= _attackStats.MaxShurikens) 
        {
            NoAmmoFX();
            return;
        }

        // Usar la direcci�n de apuntado o la direcci�n por defecto
        Vector3 direction = aimDirection == default ? transform.right : aimDirection;

        if (NetworkManager && NetworkManager.IsListening)
        {
            // Estamos en modo red
            if (!IsOwner) return; // Solo el due�o puede disparar

            // Solicitar al servidor que instancie el proyectil
            SpawnProjectileServerRpc(direction);
        }
        else
        {
            // Estamos en modo local
            SpawnProjectileLocally(direction);
        }

        // Resetear la direcci�n de apuntado
        aimDirection = default;
    }

    public void ApplyGravity(float newGravity)
    {
        gravityDebuff = newGravity;

        if (newGravity > 1.0f)
        {
            gravityIgnoreTimer = 0.0f;
        }
        else
        {
            gravityIgnoreTimer = 0.4f;
        }
    }

    public void ApplyWallBuff(bool buff)
    {
        wallBuff = buff;
    }

    [ServerRpc]
    void SpawnProjectileServerRpc(Vector3 direction)
    {
        // Instanciar el proyectil en el servidor
        var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBehaviour>().Init(direction, _selfDamageable, true, wallBuff, gravityIgnoreTimer, gravityDebuff);

        // Hacer spawn del objeto en la red
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        // Actualizar el contador de shurikens en el cliente que dispar�
        UpdateShurikenCountClientRpc(NetworkManager.LocalClientId);
    }

    [ClientRpc]
    void UpdateShurikenCountClientRpc(ulong clientId)
    {
        if (IsOwner && OwnerClientId == clientId)
        {
            aimDirection = default;
            ShurikensThrown++;
            if(ShurikensThrown > _attackStats.MaxShurikens) ShurikensThrown = _attackStats.MaxShurikens;

            if (shurikenAmmoEvent != null)
            {
                shurikenAmmoEvent.Raise(_player, ShurikensThrown);
            }

            ShurikenFX();
        }
    }

    void SpawnProjectileLocally(Vector3 direction)
    {
        // Instanciar el proyectil localmente (sin red)
        var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBehaviour>().Init(direction, _selfDamageable, true, wallBuff, gravityIgnoreTimer, gravityDebuff);

        // Actualizar contador local
        ShurikensThrown++;
        if (ShurikensThrown > _attackStats.MaxShurikens) ShurikensThrown = _attackStats.MaxShurikens;

        if (shurikenAmmoEvent != null)
        {
            shurikenAmmoEvent.Raise(_player, ShurikensThrown);
        }

        ShurikenFX();
    }

    void DebugAttack(Color color)
    {
        Debug.DrawRay(_projectileSpawnPoint.position, transform.right * _attackStats.RangedAttackRange, color);
    }

    public bool OnShurikenPickedUp()
    {   
        if(ShurikensThrown <= 0) return false;
        ShurikensThrown--;
        if (ShurikensThrown < 0) ShurikensThrown = 0;
        shurikenAmmoEvent.Raise(_player, ShurikensThrown);
        return true;    
    }

    // Destroy the shuriken if it is PICKED UP
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Shuriken"))
        {
            var shuriken = collision.GetComponent<ProjectileBehaviour>();
            if (shuriken.IsMoving) return;

            // En modo red, solo el servidor puede destruir objetos
            if (NetworkManager && NetworkManager.IsListening)
            {
                if (IsServer)
                {
                    Destroy(shuriken.gameObject);
                    OnShurikenPickupServerRpc(OwnerClientId);
                }
                else if (IsOwner)
                {
                    RequestPickupShurikenServerRpc(shuriken.NetworkObject);
                }
            }
            else
            {
                // En modo local, destruimos directamente
            if(OnShurikenPickedUp())
                Destroy(shuriken.gameObject);
            }
        }
    }

    [ServerRpc]
    void RequestPickupShurikenServerRpc(NetworkObjectReference shurikenRef)
    {
        if (shurikenRef.TryGet(out NetworkObject shurikenObject))
        {
            Destroy(shurikenObject.gameObject);
            OnShurikenPickupServerRpc(OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void OnShurikenPickupServerRpc(ulong clientId)
    {
        OnShurikenPickupClientRpc(clientId);
    }

    [ClientRpc]
    void OnShurikenPickupClientRpc(ulong clientId)
    {
        if (IsOwner && OwnerClientId == clientId)
        {
            OnShurikenPickedUp();
        }
    }
    #endregion

    #region ------- FX -------

    void NoAmmoFX()
    {
        AudioManager.PlaySound("FX_NoShurikens");
        VFXManager.PlayVFX("VFX_OutOfAmmo", VFXType.ParticleSystem, _projectileSpawnPoint.position, Quaternion.LookRotation(transform.up, transform.right), _projectileSpawnPoint.transform);
    }

    void ShurikenFX()
    {
        AudioManager.PlaySound("FX_ShurikenThrow");
    }
    #endregion
}
