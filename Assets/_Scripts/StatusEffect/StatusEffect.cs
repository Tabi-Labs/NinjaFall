using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using DG.Tweening;

public abstract class StatusEffect : MonoBehaviour
{
    public string EffectName;
    public float Duration;
    private bool active = true;

    [Header("Visual Effect")]
    public GameObject orbitingOrbPrefab;
    public float orbitRadius = 10f;
    public float orbitDuration = 2f;      

    // referencias runtime
    private GameObject orbContainer;
    private GameObject activeOrb;
    private Tween orbitTween;


    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        // Obt�n las referencias a los componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public abstract void ApplyEffect(GameObject player);
    public abstract void RemoveEffect(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null && other.CompareTag("Player"))
        {
            Debug.Log("Aplicando efecto");
            player.ApplyEffect(this);

            spriteRenderer.enabled = false;
            boxCollider.enabled = false;
        }
    }

    public virtual void StartVisualEffect(GameObject player)
    {
        orbContainer = new GameObject("OrbContainer");
        orbContainer.transform.SetParent(player.transform);
        orbContainer.transform.localPosition = Vector3.zero;

        activeOrb = Instantiate(orbitingOrbPrefab, orbContainer.transform);
        activeOrb.transform.localPosition = new Vector3(orbitRadius, 0f, 0f);

        float angle = 0f;

        orbitTween = DOVirtual.Float(0f, 360f, orbitDuration, (value) =>
        {
            angle = value * Mathf.Deg2Rad;
            
            float x = Mathf.Cos(angle) * orbitRadius;
            float z = Mathf.Sin(angle) * orbitRadius;

            activeOrb.transform.localPosition = new Vector3(x * 2.0f, 0f, z * 2.0f);

        })
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }

    public void StopVisualEffect()
    {
        if (orbitTween != null && orbitTween.IsActive())
        {
            orbitTween.Kill();
        }
        if (activeOrb != null)
        {
            Destroy(activeOrb);
            
        }

        if(orbContainer != null)
        {
            Destroy(orbContainer);
        }

    }

    public void SetActive(bool active)
    {
        this.active = active;
    }

    public bool IsActive()
    {
        return active;
    }


}
