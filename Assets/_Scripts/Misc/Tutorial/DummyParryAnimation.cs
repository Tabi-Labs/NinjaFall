using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyParryAnimation : MonoBehaviour
{
    private Animator _animator;
    private Damageable _damageable;

    void Awake()
    {
        _damageable = GetComponent<Damageable>();
        _animator = GetComponent<Animator>();

        _damageable.OnParryEvent += OnParry;   
    }

    private void OnParry()
    {
        StartCoroutine(ParryAnimation());
    }

    private IEnumerator ParryAnimation()
    {
        _animator.Play("p_MeleeAttack_1");
        yield return new WaitForSeconds(0.5f);
        _animator.Play("p_Idle");
    }
}
