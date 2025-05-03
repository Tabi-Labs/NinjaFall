using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Damageable : NetworkBehaviour, IDamageable
{   
    [Header("DEBUGGING"), SerializeField] private bool _canParry;
    [SerializeField] protected bool _isParrying;
    [SerializeField] private Material _hitEffectMaterial;
    [SerializeField] private float _hitEffectDuration = 0.1f;
    private float _lerpAmount;
    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");
    private bool inmune = false;
    public bool IsEnabled = true;
    private List<Transform> spawnPoints = new List<Transform>();

    private SpriteRenderer _spriteRenderer;

    public event Action OnDamageTakenEvent;
    public event Action OnParryEvent; // Event to notify when parry is triggered
    protected void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        LoadSpots();
    }

    protected void LoadSpots()
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
        if(!IsEnabled) return; 
        
        if(_isParrying)
        {
            OnParryEvent?.Invoke();
            AudioManager.PlaySound("FX_SwordClash");
            VFXManager.PlayVFX("VFX_Impact",VFXType.Animation, transform.position, Quaternion.identity);
            return;
        }

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

    public bool IsInmune()
    {
        return inmune;
    }

    protected virtual void OnDamageTaken()
    {
        OnDamageTakenEvent?.Invoke();
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
        _spriteRenderer.DOColor(Color.red, _hitEffectDuration).OnComplete(() =>
        {
            _spriteRenderer.DOColor(Color.white, _hitEffectDuration);
        });
    }

    public virtual bool CanParry()
    {
        return _canParry;
    }

    public virtual bool IsParrying()
    {
         if(_isParrying)
            OnParryEvent?.Invoke();
        
        return _isParrying;
    }
}
