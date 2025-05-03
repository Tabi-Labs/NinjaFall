using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Parallax : MonoBehaviour
{

    [SerializeField] float _speed = 0.5f;
    [SerializeField] float _limitX = 0f;
    [SerializeField] float _resetX = 0f;
    [SerializeField] bool _useYOffset = false;
    [SerializeField, Range(0, 100)]  float _resetYOffset = 0f;

    private float _initalY;

    void Start()
    {
        _initalY = transform.localPosition.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool reset = _speed > 0 ? transform.localPosition.x > _limitX : transform.localPosition.x <= _limitX;

        if (reset)
        {
            Debug.Log("Resetting localPosition");
            float yOffset = _useYOffset ? Random.value * _resetYOffset : 0;
            transform.localPosition = new Vector3(_resetX, _initalY + yOffset, transform.localPosition.z);
        } else
        {
            transform.Translate(Vector3.right * _speed * Time.fixedDeltaTime);
        }

    }
}
