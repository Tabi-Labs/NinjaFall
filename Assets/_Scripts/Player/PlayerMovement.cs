using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player{
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public event Action OnJump;
    public event Action OnLand;
    public event Action<bool> OnMove;
    public event Action OnMoveCancel;

    [Header("REFERENCES")]
    [SerializeField] private MovementParameters moveParams;
    [SerializeField] private InputProcessor input;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Rigidbody2D rb;



    private bool isFacingRight;

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private RaycastHit2D wallHit;
    private RaycastHit2D lastWallHit;
    private bool isGrounded = false;
    private bool bumpedHead;
    private bool isTouchingWall;

    public float HorizontalVelocity{get;private set;}
    public float VerticalVelocity{get; private set;}
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    private float jumpBufferTime;
    private bool jumpReleasedDuringBuffer;

    private float coyoteTimer;

    private bool isWallSliding;
    private bool isWallSlideFalling;
    
    private bool useWallJumpMoveStats;
    private bool isWallJumping;
    private float wallJumpTime;
    private bool isWallJumpFastFalling;
    private bool isWallJumpFalling;
    private float wallJumpFastFallTime;
    private float wallJumpFastFallReleaseSpeed;

    private float walLJumpPostBufferTimer;

    private float wallJumpApexPoint;
    private float timePastWallJumpApexThreshold;
    private bool isPastWallJumpaApexThreshold;

    private bool isDashing;
    private bool isAirDashing;
    private float dashTimer;
    private float dashOnGroundTimer;
    private int numberOfDashesUsed;
    private Vector2 dashDirection;
    private bool isDashFastFalling;
    private float dashFastFallTime;
    private float dashFastFallReleaseSpeed;
    
    // Start is called before the first frame update
    #region ----------- INITIALIZERS -----------
    private void InitializeRigidBody() => rb = GetComponent<Rigidbody2D>();
    #endregion
    #region ----------- UNITY CALLBACKS ------------
    void Awake()
    {
       InitializeInput();
       InitializeRigidBody();
       isFacingRight = true;
       
    }

    void OnDisable()
    {
        //input.JumpEvent -= OnJumpEvent;
    }
    void Start()
    {
     
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();
        WallJump();
        if(isGrounded)
            Move(moveParams.GroundAcceleration, moveParams.GroundDeceleration, 1.0f,input.MoveInput);
        else 
        {
            if(useWallJumpMoveStats)
            {
                Move(moveParams.WallJumpMoveAcceleration, moveParams.WallJumpMoveDeceleratoin, 1.0f, input.MoveInput);
            }
            else 
            {   
                Debug.Log("IN AIR ACCELERATION");
                Move(moveParams.AirAcceleration, moveParams.AirDeceleration, moveParams.AirMultiplier, input.MoveInput);
            }
                
        }
        

        ApplyVelocity();
    }   

    void Update()
    {
        CountTimers();
        JumpChecks();
        LandCheck();
        WallSlideCheck();
        WallJumpCheck();
    }

    #endregion
    

    #region ----------- INPUT ------------

    private void InitializeInput()
    {
        input.Enable();
        input.JumpEvent += () => {

            if(walLJumpPostBufferTimer > 0f)
            {
                InitiateWallJump();
            }
            if(isWallSlideFalling && walLJumpPostBufferTimer >= 0f) return;

            else if(isWallSliding || (isTouchingWall && !isGrounded)) return;

             jumpBufferTime = moveParams.JumpBufferTime;
            jumpReleasedDuringBuffer = false;

            
        };
        input.JumpCanceled += () => {
              if(jumpBufferTime > 0f)
            {
                jumpReleasedDuringBuffer = true;
            }

            if(isJumping && VerticalVelocity > 0f)
            {
                if(isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = moveParams.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }

                else 
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }

            if(!isWallSliding && !isTouchingWall && isWallJumping)
            {
                if(VerticalVelocity >0f)
                {
                    if(isPastWallJumpaApexThreshold)
                    {
                        isPastWallJumpaApexThreshold = false;
                        isWallJumpFastFalling = true;
                        wallJumpFastFallTime = moveParams.TimeForUpwardsCancel;
                        VerticalVelocity = 0f;
                    }

                    else 
                    {
                        isWallJumpFastFalling = true;
                        wallJumpFastFallReleaseSpeed = VerticalVelocity;
                    }
                }
            }
        };
    }

    #endregion

    #region ---------- MOVEMENT ------------

    void Move(float acceleration, float deceleration, float multiplier, Vector2 moveInput)
    {
       
        if(isDashing) return;

       if(Mathf.Abs(moveInput.x) >= moveParams.MoveThreshold)
       {
            float targetVelocity;
            if(input.isRunning)
                targetVelocity =moveInput.x * moveParams.MaxRunSpeed;
            else 
                targetVelocity = moveInput.x * moveParams.MaxWalkSpeed;

            targetVelocity *= multiplier;
            HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            TurnCheck(moveInput);
       }

       else if (Mathf.Abs(moveInput.x) < moveParams.MoveThreshold)
       {
         HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
       }

       if(moveParams.TurnToWallJumpDirection && isWallJumping)
       {
            TurnCheck();
       }
     
       
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if(isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if(!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void TurnCheck()
    {
         if(isFacingRight && HorizontalVelocity < 0)
        {
            Turn(false);
        }
        else if(!isFacingRight && HorizontalVelocity > 0)
        {
            Turn(true);
        }
    }
    private void Turn(bool turnRight)
    {
        if(turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f,180f,0f);
        }

        else 
        {
            isFacingRight = false;
            transform.Rotate(0f,-180f,0f);
        }
    }

    private void ApplyVelocity()
    {
        if(!isDashing)
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -moveParams.MaxFallSpeed, 50f);
        else 
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -50f, 50f);
        rb.velocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }
    #endregion
    #region ---------- JUMP --------------

    private void ResetJumpValues()
    {
        isJumping = false;
        isFalling = false;
        isFastFalling = false;
        fastFallTime = 0F;
        isPastApexThreshold = false;
    }
    private void JumpChecks()
    {
        if(input.hasJumpPerformed)
        {
           
        }

        if(!input.hasJumpPerformed)
        {
          
        }

        if(jumpBufferTime > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);
            if(jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;

            }
        }

        else if(jumpBufferTime > 0f && (isJumping || isWallJumping || isWallSlideFalling || isAirDashing || isDashFastFalling)&& !isTouchingWall && numberOfJumpsUsed < moveParams.NumberOfJumpsAllowed)
        {
            
            isFastFalling = false;
            InitiateJump(1);

            if(isDashFastFalling)
                isDashFastFalling = false;
        }

        else if(jumpBufferTime > 0f && isFalling && !isWallSlideFalling && numberOfJumpsUsed < moveParams.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling= false;
        }

    
    }

    private void InitiateJump(int jumpsUsed)
    {
        if(!isJumping)
            isJumping = true;
        
        ResetWallJumpValues();
        jumpBufferTime = 0f;
        numberOfJumpsUsed += jumpsUsed;
        VerticalVelocity = moveParams.InitialJumpVelocity;

    }
    private void Jump()
    {
        if(isJumping)
        {
            if(bumpedHead)
                isFastFalling = true;
            
            if(VerticalVelocity >= 0f)
            {
                apexPoint = Mathf.InverseLerp(moveParams.InitialJumpVelocity, 0f, VerticalVelocity);

                if(apexPoint > moveParams.ApexThreshold)
                {
                    if(!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if(isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if(timePastApexThreshold < moveParams.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                            VerticalVelocity = -0.01f;
                    }
                }

                else if(!isFastFalling)
                {
                    VerticalVelocity += moveParams.Gravity * Time.fixedDeltaTime;

                    if(isPastApexThreshold)
                        isPastApexThreshold = false;
                }
            }

            else if(!isFastFalling)
            {
                 VerticalVelocity += moveParams.Gravity * moveParams.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity < 0f)
            {
                if(!isFalling)
                    isFalling = true;
            }
             
            
        }

        if(isFastFalling)
        {
            if(fastFallTime >= moveParams.TimeForUpwardsCancel)
            {
                VerticalVelocity += moveParams.Gravity * moveParams.GravityOnReleaseMultiplier * Time.fixedDeltaTime;

            }

            else if (fastFallTime < moveParams.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / moveParams.TimeForUpwardsCancel));
            }

                 fastFallTime += Time.fixedDeltaTime;
        }
        


    }
    #endregion

    #region ---------- LAND/FALL ---------

    private void LandCheck()
    {
          if((isJumping || isFalling || isWallJumpFalling || isWallJumping || isWallSlideFalling || isWallSliding || isDashFastFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();
            ResetDashes();

            if(isWallSliding && moveParams.ResetJumpsOnWallSlide)
                numberOfJumpsUsed = 0;
            else if(isWallSliding && !moveParams.ResetJumpsOnWallSlide)
            {

            }
            else
                numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
            if(isDashFastFalling && isGrounded)
            {
                ResetDashValues();
                return;
            }

            ResetDashValues();
        }
    }

    private void Fall()
    {
        
        if(!isGrounded && !isJumping && !isWallSliding && !isWallJumping && !isDashing && !isDashFastFalling)
        {
            if(!isFalling)
            {
                isFalling = true;
            }

            VerticalVelocity += moveParams.Gravity * Time.fixedDeltaTime;
        }
    }
    #endregion
    #region  ---------- WALL SLIDE ----------
    private void WallSlideCheck()
    {
        if(isTouchingWall && !isGrounded && !isDashing)
        {
            if(VerticalVelocity < 0f && !isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                if(moveParams.ResetDashOnWallSlide)
                {
                    ResetDashes();
                }
                isWallSlideFalling = false;
                isWallSliding = true;
                Debug.Log("TOUCHING A WALL AND WALL SLIDING IS: " + isWallSliding);
                if(moveParams.ResetJumpsOnWallSlide)
                {
                    numberOfJumpsUsed = 0;
                }
            }
        }
        else if(isWallSliding && !isTouchingWall && !isGrounded && !isWallSlideFalling)
        {
            isWallSlideFalling = true;
            StopWallSlide();
        }

        else 
        {
            StopWallSlide();
        }
    }

    private void StopWallSlide()
    {
        if(isWallSliding)
        {
            numberOfJumpsUsed++;
            isWallSliding = false;
        }
    }

    private void WallSlide()
    {
        if(isWallSliding)
        {
            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -moveParams.WallSlideSpped, moveParams.WallSlideDecelerationSpeed * Time.fixedDeltaTime);
            
        }
    }
    #endregion

    #region -------------- WALL JUMP ------------
    private void ResetWallJumpValues()
    {
        isWallSlideFalling = false;
        useWallJumpMoveStats = false;
        isWallJumping = false;
        isWallJumpFalling= false;
        isWallJumpFastFalling = false;
        isPastWallJumpaApexThreshold = false;

        wallJumpFastFallTime = 0f;
        wallJumpTime = 0f;
    }

    private void WallJump()
    {
        if(isWallJumping)
        {
            wallJumpTime += Time.fixedDeltaTime;
            if(wallJumpTime >= moveParams.TimeTillJumpApex)
            {
                useWallJumpMoveStats = false;
            }

            if(bumpedHead)
            {
                isWallJumpFastFalling = true;
                useWallJumpMoveStats = false;
            }

            if(VerticalVelocity >= 0f)
            {
                wallJumpApexPoint = Mathf.InverseLerp(moveParams.WallJumpDirection.y, 0f, VerticalVelocity);

                if(wallJumpApexPoint > moveParams.ApexThreshold)
                {
                    if(!isPastWallJumpaApexThreshold)
                    {
                        isPastWallJumpaApexThreshold = true;
                        timePastWallJumpApexThreshold = 0f;
                    }

                    if(isPastWallJumpaApexThreshold)
                    {
                        timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if(timePastWallJumpApexThreshold < moveParams.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else 
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                else if(!isWallJumpFastFalling)
                {
                    VerticalVelocity += moveParams.WallJumpGravity * Time.fixedDeltaTime;
                    if(isPastWallJumpaApexThreshold)
                    {
                        isPastWallJumpaApexThreshold = false;
                    }
                }

            }

            else if(!isWallJumpFastFalling)
            {
                VerticalVelocity += moveParams.WallJumpGravity * Time.fixedDeltaTime;
            }

            else if(VerticalVelocity <0f)
            {
                if(!isWallJumpFalling)
                    isWallJumpFalling = true;
            }
        }

        if(isWallJumpFastFalling)
        {
            if(wallJumpFastFallTime >= moveParams.TimeForUpwardsCancel)
            {
                VerticalVelocity += moveParams.WallJumpGravity * moveParams.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(wallJumpFastFallTime < moveParams.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(wallJumpFastFallReleaseSpeed, 0f, (wallJumpFastFallTime/ moveParams.TimeForUpwardsCancel));
            }

            wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    private void WallJumpCheck()
    {
        if(ShouldApplyPostWallJumpBuffer())
        {
            walLJumpPostBufferTimer = moveParams.WallJumpPostBufferTime;
        }

        if(!input.hasJumpPerformed)
        {
            // REFACTOR LATER AND ADD THE CODE THAT IS IN INPUT SECTION FOR THE JUMP CANCELED EVENT
        }

         if(input.hasJumpPerformed)
        {
            // REFACTOR LATER AND ADD THE CODE THAT IS IN INPUT SECTION FOR THE JUMP CANCELED EVENT
        }
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if(!isGrounded && (isTouchingWall || isWallSliding))
        {
            return true;
        }

        return false;
    }

    private void InitiateWallJump()
    {
        Debug.Log("INITIAING WALL JUMP");
        if(!isWallJumping)
        {
            isWallJumping = true;
            useWallJumpMoveStats = true;
        }

        StopWallSlide();
        ResetJumpValues();
        wallJumpTime = 0f;
        VerticalVelocity = moveParams.InitialWallJumpVelocity;

        int dirMultiplier;
        Vector2 hitPoint = lastWallHit.collider.ClosestPoint(_bodyCollider.bounds.center);
        if(hitPoint.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else {dirMultiplier = 1;}

        HorizontalVelocity = Mathf.Abs(moveParams.WallJumpDirection.x) * dirMultiplier;
    }
    #endregion

    #region  ------------- DASH ----------------

    private void ResetDashValues()
    {
        isDashFastFalling = false;
        dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        numberOfDashesUsed = 0;
    }
    #endregion
    #region --------------- COLLISION CHECKS --------------

    private void CollisionChecks()
    {
        GroundCheck();
        BumpedHead();
        IsTouchingWall();
    }
    void  GroundCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, moveParams.GroundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, moveParams.GroundDetectionRayLength, moveParams.WallLayer);

        if(groundHit.collider != null)
        {
            isGrounded = true;
        }

        else isGrounded = false;

        #region --------- DEBUG VISUALIZATION ------------

        if(moveParams.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if(isGrounded)
            {
                rayColor = Color.green;
            }
            else rayColor = Color.red;
            
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 , boxCastOrigin.y), Vector2.down * moveParams.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 , boxCastOrigin.y), Vector2.down * moveParams.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 , boxCastOrigin.y - moveParams.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion

       
    }

    void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * moveParams.HeadWidth, moveParams.HeadDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, moveParams.HeadDetectionRayLength, moveParams.GroundLayer);

        if(headHit.collider != null)
        {
            bumpedHead = true;
        }

        else bumpedHead = false;

        #region --------- DEBUG VISUALIZATION ------------

        if(moveParams.DebugShowHeadBump)
        {
            float headWidth = moveParams.HeadWidth;
            Color rayColor;
            if(bumpedHead)
            {
                rayColor = Color.green;
            }
            else rayColor = Color.red;
            
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth , boxCastOrigin.y), Vector2.up * moveParams.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2)  * headWidth , boxCastOrigin.y), Vector2.up * moveParams.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y - moveParams.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
        #endregion

       
    }
    void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if(isFacingRight)
        {
            originEndPoint = _bodyCollider.bounds.max.x;
        }

        else { originEndPoint = _bodyCollider.bounds.min.x;}
        float adjustedHeight = _bodyCollider.bounds.size.y + moveParams.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(moveParams.WallDetectionRayLength, adjustedHeight);

        wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, moveParams.WallDetectionRayLength, moveParams.GroundLayer);
        if(wallHit.collider != null)
        {
            lastWallHit = wallHit;
            isTouchingWall = true;
        }
        else isTouchingWall = false;

        #region --------- DEBUG VISUALIZATION ---------

        if(moveParams.DebugShowWallHitBox)
        {
            Color rayColor;
            if(isTouchingWall)
            {
                rayColor = Color.green;
            }
            else rayColor = Color.red;

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x/2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x/2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x/2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x/2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);    
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopLeft, boxTopRight, rayColor);
            Debug.DrawLine(boxBottomLeft, boxTopLeft, rayColor);
        }

        #endregion
    }
    #endregion
    #region --------------- TIMERS ------------
    private void CountTimers()
    {
         jumpBufferTime -= Time.deltaTime;
         if(!isGrounded)
            coyoteTimer -= Time.deltaTime;
        else coyoteTimer = moveParams.JumpCoyoteTime;

        if(!ShouldApplyPostWallJumpBuffer())
        {
            walLJumpPostBufferTimer -= Time.deltaTime;
        }
    }
    #endregion
}
}

