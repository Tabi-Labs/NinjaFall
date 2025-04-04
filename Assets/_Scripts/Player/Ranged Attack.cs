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
    public int ShurikensCount{get;private set;}
    public GameEvent shurikenAmmoEvent;
    private Color _debugColor = Color.red;

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

    void OnDisable()
    {
        _player.Input().RangedAttackEvent -= OnRangedAttack;
    }

    void Update()
    {
        if(_attackStats.DebugAttackArea)
            DebugAttack(_debugColor);
    }

    #endregion

    #region  ---- ATTACKS ------

    void OnRangedAttack()
    {
        if (NetworkManager)
        {
            if (!IsOwner)
            {
                return;
            }
            RequestSpawnProjectileRPC();
        }
        else
        {
           
            
            if (ShurikensCount >= _attackStats.MaxShurikens) 
            {
                AudioManager.PlaySound("FX_NoShurikens");
                VFXManager.PlayVFX("VFX_OutOfAmmo", _projectileSpawnPoint.position, Quaternion.LookRotation(transform.up, transform.right), transform);
                return;
            }
            var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<ProjectileBehaviour>().Init(transform.right, _selfDamageable, true, wallBuff, gravityIgnoreTimer,gravityDebuff);
            AudioManager.PlaySound("FX_ShurikenThrow");
            ShurikensCount++;
            if(ShurikensCount > _attackStats.MaxShurikens) ShurikensCount = _attackStats.MaxShurikens;
            shurikenAmmoEvent.Raise(_player, ShurikensCount);
        }

        //projectile.GetComponent<Projectile>().Init(_stats);
    }

    public void ApplyGravity(float newGravity)
    {
        Debug.Log("Clase Ranged Attack");

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
        Debug.Log("Shuriken wall buff - Se intenta aplicar: " + buff);
        wallBuff = buff;

    }

    [Rpc(SendTo.Server)]
    void RequestSpawnProjectileRPC()
    {
        var projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<ProjectileBehaviour>().Init(transform.right, _selfDamageable, true, wallBuff, gravityIgnoreTimer, gravityDebuff);
        NetworkObject networkObject = projectile.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
    void DebugAttack(Color color)
    {
        Debug.DrawRay(_projectileSpawnPoint.position, transform.right * _attackStats.RangedAttackRange, color);
    }

    public void OnShurikenPickedUp()
    {   
        ShurikensCount--;
        if (ShurikensCount < 0) ShurikensCount = 0;
    
        shurikenAmmoEvent.Raise(_player, ShurikensCount);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Shuriken"))
        {
            var shuriken = collision.GetComponent<ProjectileBehaviour>();
            if(shuriken.IsMoving) return;
            Destroy(shuriken.gameObject);
            OnShurikenPickedUp();
        }
    }
    #endregion
}
