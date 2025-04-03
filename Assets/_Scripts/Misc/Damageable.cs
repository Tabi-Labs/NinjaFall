using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Damageable : MonoBehaviour, IDamageable
{   
    [SerializeField] private Material _hitEffectMaterial;
    [SerializeField] private float _hitEffectDuration = 0.1f;
    private float _lerpAmount;
    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");
    private bool inmune = false;

    private Transform[] spawnPoints;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    protected virtual void Awake()
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

        if (!inmune)
        {
            HitAnimation();
            OnDamageTaken();
        } else
        {
            Spawn();
        }

        StartCoroutine(RemoveImmunityAfterDelay(0.5f));
    }

    public void SetInmune(bool inmune)
    {
        this.inmune = inmune;
    }

    protected virtual void OnDamageTaken()
    {
        //override this function to add more functionality
    }

    private IEnumerator RemoveImmunityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        inmune = false; // Desactivar la inmunidad después del tiempo
        Debug.Log("La inmunidad ha terminado.");
    }

    void Spawn()
    {
        // TODO: Spawn aleatorio del jugador
        

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
