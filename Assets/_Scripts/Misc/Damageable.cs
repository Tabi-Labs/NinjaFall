using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DG.Tweening;
using Unity.Netcode;
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

    private SpriteRenderer _spriteRenderer;

    public float blinkDuration = 3f;      // Tiempo total de parpadeo (en segundos)
    public float blinkInterval = 0.5f;    // Tiempo entre cada cambio de opacidad
    public float transparentAlpha = 0.8f; // Nivel de transparencia durante el parpadeo
    private Sequence blinkSequence;

    

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

        Blink();
    }

    public void Blink()
    {
        Debug.Log("Parpadeo");

        Color originalColor = _spriteRenderer.color;

        Color blinkColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f);

        int loopCount = Mathf.FloorToInt(blinkDuration / (blinkInterval * 2));

        blinkSequence = DOTween.Sequence();

        for (int i = 0; i < loopCount; i++)
        {
            blinkSequence.Append(_spriteRenderer.DOColor(originalColor, blinkInterval));
            blinkSequence.Append(_spriteRenderer.DOColor(blinkColor, blinkInterval));
        }
        blinkSequence.OnComplete(() => {
            _spriteRenderer.color = originalColor;
            Debug.Log("Parpadeo completado");
        });
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
        return _isParrying;
    }
}
