using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DG.Tweening;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Damageable : NetworkBehaviour, IDamageable
{   
    [Header("DEBUGGING"), SerializeField] private bool _canParry;
    [SerializeField] protected bool _isParrying;
    [SerializeField] private Material _hitEffectMaterial;
    [SerializeField] private float _hitEffectDuration = 0.1f;
    private float _lerpAmount;
    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");
    private bool inmune = false;

    private List<Transform> spawnPoints = new List<Transform>();

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

    void Start()
    {
        LoadSpots();
    }

    void LoadSpots()
    {
        GameObject[] foundSpots = GameObject.FindGameObjectsWithTag("Spot"); 

        // Comprobar si se han encontrado spots
        if (foundSpots.Length == 0)
        {
            Debug.LogWarning("No se encontraron objetos con la etiqueta 'Spot'.");
            return; 
        }


        // Asignar los transform de los spots a spawnPoints
        for (int i = 0; i < foundSpots.Length; i++)
        {
            if (foundSpots[i] != null)
            {
                spawnPoints.Add(foundSpots[i].transform);
            }
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
        inmune = false; // Desactivar la inmunidad despu�s del tiempo
        Debug.Log("La inmunidad ha terminado.");
    }

    void Spawn()
    {
        int spawnPointIndex = Random.Range(0, spawnPoints.Count);

        Transform newSpot = spawnPoints[spawnPointIndex];

        Vector3 newPosition = newSpot.position + new Vector3(GetRandomSign(), 0, 0);

        transform.position = newPosition;


    }

    private float GetRandomSign()
    {
        return Random.Range(0, 2) == 0 ? 1.0f : -1.0f;
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

    public virtual bool CanParry()
    {
        return _canParry;
    }

    public virtual bool IsParrying()
    {
        return _isParrying;
    }
}
