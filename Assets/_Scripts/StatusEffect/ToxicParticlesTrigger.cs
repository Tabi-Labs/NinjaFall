using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ToxicParticlesTrigger : MonoBehaviour
{

    [Header("Collider Settings")]
    [SerializeField] private float _expansionSpeed = 20f;
    [SerializeField] private float _maxWidth = 20f;
    private BoxCollider2D _damageCollider;
    bool stopCollider;

    private IDamageable _selfDamageable;

    
    private float _currentWidth;

    void Awake()
    {
        _damageCollider = GetComponent<BoxCollider2D>();
        _damageCollider.enabled = true;
        stopCollider = false;
    }

    private void Update()
    {
        ExpandCollider();
    }

    private void ExpandCollider()
    {
        if (_currentWidth < _maxWidth && !stopCollider)
        {
            _currentWidth += _expansionSpeed * Time.deltaTime;
            _damageCollider.size = new Vector2(
                _currentWidth,
                _damageCollider.size.y
            );
        } else
        {
            ResetCollider();
        }
    }

    public void ResetCollider()
    {
        _currentWidth = 0.1f; // Mínimo tamaño inicial
        _damageCollider.size = new Vector2(_currentWidth, _damageCollider.size.y);
        stopCollider = true;
        _damageCollider.enabled = false;
    }


    public void OnTriggerEnter2D(Collider2D collider)
    {
       
        var damageableComponent = collider.GetComponentInParent<IDamageable>();

        Player player = collider.GetComponentInParent<Player>();

        if ( player != null && player.IsInmune())
        {
            return;
        }

        damageableComponent.TakeDamage(1.0f);
    
}
}
