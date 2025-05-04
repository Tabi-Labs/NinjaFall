using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RectParallax : MonoBehaviour
{
    [SerializeField] float _speed = 0.5f;
    [SerializeField] float _limitX = 0f;
    [SerializeField] float _resetX = 0f;
    [SerializeField] bool _useYOffset = false;
    [SerializeField, Range(0, 100)] float _resetYOffset = 0f;

    private RectTransform _rectTransform;
    private float _initialY;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
        {
            Debug.LogError("RectParallax requires a RectTransform component.");
            enabled = false;
            return;
        }

        _initialY = _rectTransform.anchoredPosition.y;
    }

    void FixedUpdate()
    {
        bool reset = _speed > 0 ? _rectTransform.anchoredPosition.x > _limitX : _rectTransform.anchoredPosition.x <= _limitX;

        if (reset)
        {
            float yOffset = _useYOffset ? Random.value * _resetYOffset : 0;
            _rectTransform.anchoredPosition = new Vector2(_resetX, _initialY + yOffset);
        }
        else
        {
            _rectTransform.anchoredPosition += new Vector2(_speed * Time.fixedDeltaTime, 0);
        }
    }
}