using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class AimPointer : NetworkBehaviour
{
    [SerializeField] private CustomInputManager _inputManager;
    [SerializeField] private SpriteRenderer _aimPointer;
    [SerializeField] private Transform _projectileSpawnPoint;
    bool _tweenActive = false;

    void Start()
    {
        _aimPointer.DOFade(0, 0.2f).SetEase(Ease.OutSine);
    }

    void Update()
    {
        if((NetworkManager))
            if (!IsOwner) return;
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
                aimDirection = Camera.main.ScreenToWorldPoint(aimDirection) - _projectileSpawnPoint.position;
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
