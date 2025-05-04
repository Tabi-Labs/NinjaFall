using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{

    #region ----- COMPONENTS ------
    Rigidbody2D _rigidbody2D;
    #endregion
    public float HorizontalVelocity {get; private set;}
    public float VerticalVelocity{get; private set;}
    float targetSpeed, targetAcceleration, targetDeceleration;
    private bool _isImpulsed;
    private float _impulseTimer;
    #region ------ INITIALIZERS -----
    void InitRigidbody2D() => _rigidbody2D = GetComponent<Rigidbody2D>();
    #endregion
    #region ----- UNITY CALLBACKS -----

    void Awake()
    {
        InitRigidbody2D();
    }

    void Update()
    {
        HandleImpulseTimer();   
    }
    void FixedUpdate()
    {
        ApplyVelocities();
    }
    #endregion

    #region ------- MOVEMENT ------
    /// <summary>
    /// Use this to move your player. Call from states in FixedUpdate
    /// </summary>
    /// <param name="acceleration"></param>
    /// <param name="deceleration"></param>
    /// <param name="moveInput"></param>
    /// <param name="MoveSpeed"></param>
    public void Move(float targetSpeed, float acceleration, Vector2 moveInput)
    {
        if(_isImpulsed) return;
        this.targetSpeed = targetSpeed;
        this.targetAcceleration = acceleration;
        float targetVelocity = moveInput.x * targetSpeed;
        float accel = acceleration;
        if(moveInput.sqrMagnitude < 0f) accel = targetDeceleration;
    
        HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, accel * Time.fixedDeltaTime);
    }
    public void Move(float acceleration, Vector2 moveInput) => Move(targetSpeed, acceleration, moveInput);
    public void Move(float targetSpeed, float acceleration, Vector2 moveInput, float deceleration)
    {
        targetDeceleration = deceleration;
        Move(targetSpeed, acceleration, moveInput);
    }
    public void Move(float acceleration, Vector2 moveInput, float deceleration)
    {
        targetDeceleration = deceleration;
        Move(targetSpeed, acceleration, moveInput);
    }
    public void VerticalMove(float targetSpeed, float acceleration, Vector2 moveInput)
    {
        if(_isImpulsed) return;
        float targetVelocity = moveInput.y * targetSpeed;
        VerticalVelocity = Mathf.Lerp(VerticalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }
    
    public void ChangeVerticalVelocity(float changeAmount)
    {
        VerticalVelocity = changeAmount;
    }

    public void IncrementVerticalVelocity(float incrementAmount, float maxFallSpeed = Mathf.NegativeInfinity)
    {
        VerticalVelocity += incrementAmount;
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, maxFallSpeed, Mathf.Infinity);
    }
    /// <summary>
    /// Swiftly decelerates the Horizontal velocity to zero movement.
    /// </summary>
    /// <param name="deceleration">Deceleration Speed</param>
    public void Decelerate()
    {
        float targetVelocity = 0f;
        HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, targetDeceleration * Time.fixedDeltaTime);
    }

    public void ApplyGravity(float gravity, float maxFallSpeed)
    {
        if(_isImpulsed) return;
        VerticalVelocity -= gravity * Time.fixedDeltaTime;
        VerticalVelocity = Mathf.Max(maxFallSpeed, VerticalVelocity);
    }
    public void Stop()
    {
        HorizontalVelocity = 0f;
        VerticalVelocity = 0f;
    }
    public void StopX()
    {
        HorizontalVelocity = 0f;
    }
    public void Impulse(Vector2 impulse, float inertiaTime)
    {
        HorizontalVelocity = impulse.x;
        VerticalVelocity = impulse.y;
        
        _isImpulsed = true;
        _impulseTimer = inertiaTime;
    }
    private void HandleImpulseTimer()
    {
        if(!_isImpulsed) return;
        _impulseTimer -= Time.deltaTime;
        if(_impulseTimer <= 0f) _isImpulsed = false;
    }
    private void ApplyVelocities()
    {
        _rigidbody2D.velocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }
    #endregion
}
