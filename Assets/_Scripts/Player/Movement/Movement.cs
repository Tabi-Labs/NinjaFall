using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //[Header("STATS")]
    //[SerializeField] PlayerMovementStats _stats;
    /* [Header("REFERENCES")]
    [SerializeField] Collider2D _feetCollider;
    [SerializeField] Collider2D _headCollider;
    [SerializeField] Collider2D _bodyCollider;
 */
    #region ----- COMPONENTS ------
    Rigidbody2D _rigidbody2D;
    #endregion
    float HorizontalVelocity, VerticalVelocity;

    #region ------ INITIALIZERS -----
    void InitRigidbody2D() => _rigidbody2D = GetComponent<Rigidbody2D>();
    #endregion
    #region ----- UNITY CALLBACKS -----

    void Awake()
    {
        InitRigidbody2D();
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
       /*  if (!IsDashing)
        {
            float moveSpeed = 0f;
            if (InputManager.RunIsHeld)
            {
                moveSpeed = MoveStats.MaxRunSpeed;
            }
            else { moveSpeed = MoveStats.MaxWalkSpeed; }

            if (Mathf.Abs(moveInput.x) > MoveStats.MoveThreshold)
            {
                //Implement somewhere else the turn check. On an Animation Handler script.
                //TurnCheck(moveInput);
                float targetVelocity = moveInput.x * moveSpeed;
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime); 
            }

            else
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.deltaTime);
            }
        } */ 
        //if(Mathf.Abs(moveInput.x) > _stats.MoveThreshold)
        //{

            float targetVelocity = moveInput.x * targetSpeed;
            HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        //}

    }

    /// <summary>
    /// Swiftly decelerates the Horizontal velocity to zero movement.
    /// </summary>
    /// <param name="deceleration">Deceleration Speed</param>
    public void Decelerate(float deceleration)
    {
        float targetVelocity = 0f;
        HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
    }

    private void ApplyVelocities()
    {
        _rigidbody2D.velocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }
    #endregion
}
