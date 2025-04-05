using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AimPointer : MonoBehaviour
{
    [SerializeField] private CustomInputManager _inputManager;
    [SerializeField] private SpriteRenderer _aimPointer;
    [SerializeField] private Transform _projectileSpawnPoint;
    Camera _camera;

    bool _tweenActive = false;

    void Start()
    {
        _aimPointer.DOFade(0, 0.2f).SetEase(Ease.OutSine);
        _camera = Camera.main;
    }
    void Update()
    {
        if(_inputManager.IsAiming )
        {
            if (!_tweenActive)
            {
                _aimPointer.DOFade(1, 0.2f).SetEase(Ease.OutSine);
                _tweenActive = true;
            }
            Vector2 aimDirection = _inputManager.AimMovement;
            if(aimDirection.SqrMagnitude() > 3f)
            {
                aimDirection = _camera.ScreenToWorldPoint(aimDirection) - _projectileSpawnPoint.position;
                
            }
            aimDirection.Normalize();
            if(aimDirection.sqrMagnitude == 0f)
                aimDirection = _inputManager.transform.right;
            transform.up = aimDirection;
        }
        else
        {
            if(_tweenActive)
                _aimPointer.DOFade(0, 0.2f).SetEase(Ease.OutSine);
            _tweenActive = false;
        }
    }

}
