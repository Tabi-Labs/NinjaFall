using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Damageable : MonoBehaviour, IDamageable
{   
    [SerializeField] private Material _hitEffectMaterial;
    [SerializeField] private float _hitEffectDuration = 0.1f;
    private float _lerpAmount;
    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private void Awake()
    {
        _spriteRenderers  = GetComponentsInChildren<SpriteRenderer>();

        _materials = new Material[_spriteRenderers.Length];
        for(int i = 0; i < _spriteRenderers.Length; i++)
        {  
            _materials[i] = _spriteRenderers[i].material;
        }
    }
    public void TakeDamage(float damage)
    {
        OnDamageTaken();
        HitAnimation();
    }

    protected virtual void OnDamageTaken()
    {
        //override this function to add more functionality
    }

    private void HitAnimation()
    {
        _lerpAmount = 0;
        foreach(SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.material = _hitEffectMaterial;
        }
        DOTween.To(GetLerpValue, SetLerpValue, 1f, _hitEffectDuration).SetEase(Ease.OutExpo).OnUpdate(OnLerpUpdate).OnComplete(OnLerpComplete);
    }

    private void OnLerpUpdate()
    {
        for(int i = 0; i < _materials.Length; i++)
        {
            _hitEffectMaterial.SetFloat(_hitEffectAmount, GetLerpValue());
            //_materials[i].SetFloat(_hitEffectAmount, GetLerpValue());
        }
    }   

    private void OnLerpComplete()
    {
        DOTween.To(GetLerpValue, SetLerpValue, 0f, _hitEffectDuration).OnUpdate(OnLerpUpdate).OnComplete(() => 
        {
            foreach(SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.material = _materials[0];
            }
        });
    }
    private float GetLerpValue()
    {
       
        return _lerpAmount;
    }

    private void SetLerpValue(float newValue)
    {
        _lerpAmount = newValue;
    }
}
