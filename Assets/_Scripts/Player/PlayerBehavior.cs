using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    [Header("INPUT REFERENCE")]
    [SerializeField]InputProcessor _input;
    [Header("STATS")]
    [SerializeField]PlayerMovementStats _movementStats;

    #region ---- COMPONENTS ----
    Movement _movement;
    #endregion

    #region ----- INITIALIZERS -----
    void InitInput() => _input.Enable();
    void InitMoveComponent()
    {
        _movement = GetComponent<Movement>();
        if(!_movement)
        {
            Debug.LogWarning($"{gameObject} lacks a Movement Component to move.");
        }
    }
    #endregion

    void Awake()
    {
        InitMoveComponent();
    }
    void OnEnable()
    {
        InitInput();
        
    }

    void FixedUpdate()
    {
        _movement.Move(_movementStats.MaxWalkSpeed, _movementStats.GroundAcceleration, _input.MoveInput);
    }
}
