using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDamageable : Damageable
{
    [SerializeField] private AttackStats _stats;
    private Player _player;
    private float _parryTimer;

    #region ----- MONOBEHAVIOUR CALLBACKS -----

    protected void Awake()
    {
        base.Awake();
        _player = GetComponent<Player>();      
    }

    void Start()
    {
       base.LoadSpots();
       _player.Input().MeleeAttackEvent += Parry;
    }

    private void Update()
    {
        HandleParryTimer();
    }

    #endregion
    protected override void OnDamageTaken()
    {
        base.OnDamageTaken();
        // VFXManager.PlayVFX("VFX_Blood", VFXType.Animation, transform.position, Quaternion.identity, transform);
        if (_player != null)
        {
            if(NetworkManager)
                _player.DeathRPC();
            else
                _player.Death();
        }
    }

    public override bool CanParry()
    {
        return true;
    }

    public override bool IsParrying()
    {
        return _isParrying;
    }

    private void Parry()
    {
        _isParrying = true;
        _parryTimer = _stats.ParryTime;
    }

    private void HandleParryTimer()
    {
        if(_isParrying)
        {
            _parryTimer -= Time.deltaTime;
            if(_parryTimer <= 0f)
            {
                _isParrying = false;
            }
        }
    }

  
}
