using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyDeathAnimation : MonoBehaviour
{
    private Animator _animator;
    private Damageable _damageable;

    void Awake()
    {
        _damageable = GetComponent<Damageable>();
        _animator = GetComponent<Animator>();

        _damageable.OnDamageTakenEvent += OnDamageTaken;   
    }

    private void OnDamageTaken()
    {
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        _animator.Play("p_Death");
        AudioManager.PlaySound("FX_Death");
        _damageable.IsEnabled = false;
        yield return new WaitForSeconds(1.5f);
        _animator.Play("p_Idle");
        _damageable.IsEnabled = true;
    }
}
