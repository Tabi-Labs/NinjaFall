using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;


public class Player : NetworkBehaviour
{
    #region Variables
  
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D FeetColl;
    [SerializeField] private Collider2D HeadColl;
    [SerializeField] private Collider2D BodyColl;
 
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }
    private PlayerInput _playerInput;

    public StatusEffectManager EffectManager { get; private set; }

    [Header("FX")]
    public GameObject JumpParticles;
    public GameObject SecondJumpParticles;
    public GameObject ToxicParticles;
    public GameObject LandParticles;
    public Transform ParticleSpawnTransform;
    public TrailRenderer TrailRenderer;
    public ParticleSystem SpeedParticles;
    public GameObject DashParticles;
    public ParticleSystem WallSlideParticles;
    //public GameObject Shuriken;
    //public Transform FirePoint;
    //public GameObject Sword;

    [Header("Height Tracker")]
    public Transform HeightTracker;

    [Header("Dont Destroy On Load")]
    public bool DontDestroyOnLoadFlag = true;

    [Header("Events")]
    public GameEvent OnPlayerDeath;

    [Header("Debug")]
    public bool ShowEnteredStateDebugLog = false;

    public bool isReady = false;

    //animation vars
    public const string IS_WALKING = "isWalking";
    public const string IS_RUNNING = "isRunning";
    public const string IS_WALL_SLIDING = "isWallSliding";
    public const string IS_DASHING = "isDashing";
    public const string IS_AIR_DASH_FALLING = "isAirDashFalling";
    public const string JUMP = "jump";
    public const string LAND = "land";
    public const string FALL = "fall";
    public const string IS_ATTACKING = "isAttacking";
    public const string IS_DEATH = "isDeath";

      
    #region ----- COMPONENT VARS -------
    private CustomInputManager _input;
    public CustomInputManager InputManager => _input;
    private Movement _movement;
    public Movement Movement => _movement;
    
    #endregion

    //state vars
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerInAirState InAirState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerDashState DashState { get; private set; }

    public PlayerMeleeAttackState MeleeAttackState { get; private set; }

    public PlayerRangeAttackState RangeAttackState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }


    //collision vars
    public bool IsDead { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool BumpedHead { get; private set; }
    public bool IsTouchingWall { get; private set; }
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    public RaycastHit2D WallHit { get; private set; }
    private RaycastHit2D _lastWallHit;

    public bool isFacingRight = true;

    public bool SpeedBuff { get; private set; }

    public bool InvertControlDebuff { get; private set; }

    // death vars
    public bool IsDeath { get; private set; }

    // attack vars
    public bool IsAttacking { get; private set; }

    public bool ToxicBuff {  get; private set; }

    public bool InmuneToxic {  get; private set; }


    //jump vars
    public bool IsJumping { get; set; }
    public bool IsFastFalling { get; set; }
    public bool IsFalling { get; set; }
    public float FastFallTime { get; set; }
    public float FastFallReleaseSpeed { get; set; }
    public int NumberOfJumpsUsed { get; set; }
    //apex vars
    public float ApexPoint { get; set; }
    public float TimePastApexThreshold { get; set; }
    public bool IsPastApexThreshold { get; set; }
    //jump buffer vars
    public float JumpBufferTimer { get; set; }
    public bool JumpReleasedDuringBuffer { get; set; }
    //coyote time vars
    public float CoyoteTimer { get; private set; }

    public bool IsWallSliding { get; set; }
    public bool IsWallSlideFalling { get; set; }
    public bool UseWallJumpMoveStats { get; set;}
    public bool IsWallJumping { get; private set;}
    public float WallJumpTime { get; private set;}
    public bool IsWallJumpFastFalling { get; private set;}
    public bool IsWallJumpFalling { get; private set;}
    public float WallJumpFastFallTime { get; private set;}
    public float WallJumpFastFallReleaseSpeed { get; private set;}

    public float WallJumpPostBufferTimer { get; private set;}

    public float WallJumpApexPoint { get; private set;}
    public float TimePastWallJumpApexThreshold { get; private set;}
    public bool IsPastWallJumpApexThreshold { get; private set;}

    //dash vars
    public bool IsDashing { get; private set;}
    public bool IsAirDashing { get; private set;}
    public float DashTimer { get; private set;}
    public float DashOnGroundTimer { get; private set;}
    public int DashDirectionMult { get; private set;}
    public int NumberOFDashesUsed { get; private set;}
    public Vector2 DashDirection { get; private set;}
    public bool IsDashFastFalling { get; private set;}
    public float DashFastFallTime { get; private set;}
    public float DashFastFallReleaseSpeed { get; private set;}

    //high point trackers
    public float HighestPoint { get; private set; }
    public float HeightTrackerStartingPoint { get; private set; }

    #endregion

    #region ---- INITIALIZERS ----
    private void InitInput() => _input = GetComponent<CustomInputManager>();
    private void InitStateMachine()
    {
        StateMachine = new PlayerStateMachine();

        IdleState = new PlayerIdleState(this, StateMachine);
        WalkState = new PlayerWalkState(this, StateMachine);
        RunState = new PlayerRunState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        InAirState = new PlayerInAirState(this, StateMachine);
        WallSlideState = new PlayerWallSlideState(this, StateMachine);
        WallJumpState = new PlayerWallJumpState(this, StateMachine);
        DashState = new PlayerDashState(this, StateMachine);
        MeleeAttackState = new PlayerMeleeAttackState(this, StateMachine);
        RangeAttackState = new PlayerRangeAttackState(this, StateMachine);
        DeathState = new PlayerDeathState(this, StateMachine);

        
    }
    private void InitMovement() => _movement = GetComponent<Movement>();
    private void InitRigidbody() => RB = GetComponent<Rigidbody2D>();
    private void InitAnimator() =>  Anim = GetComponent<Animator>();
    private void InitPlayerInput() => _playerInput = GetComponent<PlayerInput>();

    private void InitEffectManager() => EffectManager = GetComponent<StatusEffectManager>();
    #endregion

    #region ---- GETTERS / SETTERS ----
    public CustomInputManager Input() 
    {
        if(_input == null)
            _input = transform.AddComponent<CustomInputManager>();
        return _input;
    }
    #endregion

    #region ---- UNITY CALLBACKS ----
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        Initialize();

       
    }
    private void Awake()
    {
        if(!NetworkManager)
            Initialize();
    }

    private void Initialize()
    {
        InitInput();
        InitStateMachine(); 
        InitMovement();  
        InitAnimator();
        InitRigidbody();
        InitPlayerInput();

        StateMachine.InitializeDefaultState(IdleState);
        WallSlideParticles.gameObject.SetActive(false);
        if(DontDestroyOnLoadFlag) DontDestroyOnLoad(transform.root);
    }
    private void OnDisable()
    {
        
    }
    private void Update()
    {
        if(NetworkManager && !IsOwner) return;
        StateMachine.CurrentState.StateUpdate();
    }

    private void FixedUpdate()
    {
        if(NetworkManager && !IsOwner) return;
        StateMachine.CurrentState.StateFixedUpdate();
    }

    public void ApplyEffect(StatusEffect effect)
    {
        Debug.Log("Effect manager aplica efecto " + effect.name + " al jugador: ", this.gameObject);
        EffectManager.ApplyStatusEffect(effect, gameObject);
    }


    public void SetInmuneToxic(bool inmune)
    {
        this.InmuneToxic = inmune;
    }

    public bool IsInmuneToxic()
    {
        return InmuneToxic;
    }

    #endregion
   
    #region ------ SPRITE HANDLING ------
    public void TurnCheck(Vector2 moveInput)
    {
        if(moveInput.x < 0 && isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            isFacingRight = false;
        }
        else if(moveInput.x > 0 && !isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isFacingRight = true;
        }
    }

    public void SetSpeedBuff(bool speedBuff)
    {
        SpeedBuff = speedBuff;
    }

    public void SetInvertControlDebuff(bool invertControl)
    {
        InvertControlDebuff = invertControl;
    }

    public Vector2 GetMovement()
    {
        Vector2 movement = InputManager.Movement;

        if (InvertControlDebuff)
        {
            movement.x = -movement.x;
        }

        return movement;
    }


    #endregion
   

    #region Death

    public bool CheckIsDead()
    {
        return IsDead;
    }

    public void OnHit()
    {
        IsDead = true;
    }
    [Rpc(SendTo.Everyone)]
    public void DeathRPC()
    {
        Death();
    }
    public void Death()
    {
        //if (!IsOwner) return;
        StateMachine.ChangeState(DeathState);
        Debug.Log("Player ID: "+ _playerInput.playerIndex);
        KillsCounter.Instance.PlayerKilled(_playerInput.playerIndex);
        
    }

    public void DeletePlayer()
    {
        StateMachine.ChangeState(IdleState);
        PlayerSpawner.Instance.RespawnPlayer(this.gameObject);
        if (!NetworkManager)
            this.gameObject.SetActive(false);
        else
        {
            if (IsOwner)
                DeletePlayerRPC();
        }
    }
    [Rpc(SendTo.Server)]
    void DeletePlayerRPC()
    {
        NetworkObject.Despawn(true); 

        Destroy(gameObject); 
    }
    #endregion
    #region Landed
    public bool HasLanded()
    {
        if ((IsJumping || IsFalling || IsWallJumpFalling || IsWallJumping || IsWallSlideFalling || IsWallSliding || IsDashFastFalling) && IsGrounded && Movement.VerticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSliding();
            IsWallSlideFalling = false;
            ResetWallJumpValues();
            ResetDashes();

            Movement.ChangeVerticalVelocity(Physics2D.gravity.y);

            ReplenishJumps();
            TrailRenderer.emitting = false;

            if (IsDashFastFalling && IsGrounded)
            {
              /*   if (!Anim.GetBool(IS_AIR_DASH_FALLING))
                {
                    ResetDashValues();
                    return true;
                } */
            }

            ResetDashValues();

            Anim.Play("p_Land");
            //Anim.SetTrigger(LAND);

            //height tracker
            if (MoveStats.DebugShowHeightLogOnLand)
            {
                Debug.Log("Highest Point : " + (HighestPoint - HeightTrackerStartingPoint));
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Melee Attack

    public void SetIsAttacking(bool isAttacking)
    {
        IsAttacking = isAttacking;
    }

    #endregion

    #region Jump

    #region Jump Inputs

    public void JumpInputChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            //cancel if we should jump from a post wall jump buffer
            if (IsWallSlideFalling && WallJumpPostBufferTimer >= 0f)
            {
                return;
            }

            //cancel if we are wall sliding or touching a wall in the air
            else if (IsWallSliding || (IsTouchingWall && !IsGrounded))
            {
                return;
            }

            JumpWasPressed();
        }

        if (InputManager.JumpWasReleased)
        {
            JumpWasReleased();
        }
    }

    private void JumpWasPressed()
    {
        JumpBufferTimer = MoveStats.JumpBufferTime;
        JumpReleasedDuringBuffer = false;
    }

    private void JumpWasReleased()
    {
        if (JumpBufferTimer > 0f)
        {
            JumpReleasedDuringBuffer = true;
        }

        if (IsJumping && Movement.VerticalVelocity > 0f)
        {
            if (IsPastApexThreshold)
            {
                IsPastApexThreshold = false;
                IsFastFalling = true;
                FastFallTime = MoveStats.TimeForUpwardsCancel;

                //gets rid of floatiness
                Movement.ChangeVerticalVelocity(0f);

            }
            else
            {
                IsFastFalling = true;
                FastFallReleaseSpeed = Movement.VerticalVelocity;
            }
        }
    }

    #endregion

    #region Jump Checks

    public bool JumpBufferedOrCoyoteTimed()
    {
        if (JumpBufferTimer > 0f && !IsJumping && (IsGrounded || CoyoteTimer > 0f))
        {
            JumpBufferTimer = 0f;

            return true;
        }
        return false;
    }

    public bool CanJump()
    {
        //ACTUAL JUMP WITH COYOTE TIME AND JUMP BUFFER
        if (JumpBufferTimer > 0f && !IsJumping && (IsGrounded || CoyoteTimer > 0f))
        {
            if (IsDashFastFalling)
            {
                IsDashFastFalling = false;
            }

            JumpBufferTimer = 0f;
            return true;
        }
        return false;
    }

    public bool CanAirJump()
    {
        //handle double jump
        //if (JumpBufferTimer > 0f && IsJumping && NumberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        if (JumpBufferTimer > 0f && (IsJumping || IsWallJumping || IsWallSlideFalling || IsAirDashing || IsDashFastFalling) && !IsTouchingWall && NumberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            if (IsDashFastFalling)
            {
                IsDashFastFalling = false;
            }

            JumpBufferTimer = 0f;
            IsFastFalling = false;
            return true;
        }

        //handle air jump AFTER the coyote time has elapsed (subtract one extra jump here so we don't get a bonus jump)
        //else if (JumpBufferTimer > 0f && IsFalling && NumberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed -1)
        else if ((JumpBufferTimer > 0f && IsFalling && !IsWallSlideFalling && NumberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1))
        {
            NumberOfJumpsUsed++;
            JumpBufferTimer = 0f;
            IsFastFalling = false;
            return true;
        }

        return false;
    }

    public void CheckForFalling()
    {
        //REGULAR FALLING
        if (!IsGrounded && !IsJumping && !IsFalling && !IsWallSliding && !IsWallJumping && !IsDashing && !IsDashFastFalling)
        {
            if (!IsFalling)
            {
                //Anim.ResetTrigger(LAND);
                //Anim.SetTrigger(FALL);
                IsFalling = true;

                if (!IsAttacking)
                {
                    
                    Anim.Play("p_Fall");
                }        
            }

            StateMachine.ChangeState(InAirState);
        }
    }

    #endregion

    #region Jump Functions

    public void ResetJumpValues()
    {
        IsJumping = false;
        IsFalling = false;
        IsFastFalling = false;
        FastFallTime = 0f;
        IsPastApexThreshold = false;
    }

    public void ReplenishJumps()
    {
        NumberOfJumpsUsed = 0;
    }

    public void JumpPhysics()
    {
        if (IsJumping)
        {
            //HIT HEAD
            if (BumpedHead)
            {
                IsFastFalling = true;
            }

            if (Movement.VerticalVelocity >= 0f)
            {
                //APEX CONTROLS
                ApexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, Movement.VerticalVelocity);

                if (ApexPoint > MoveStats.ApexThreshold)
                {
                    if (!IsPastApexThreshold)
                    {
                        IsPastApexThreshold = true;
                        TimePastApexThreshold = 0f;
                    }

                    if (IsPastApexThreshold)
                    {
                        TimePastApexThreshold += Time.fixedDeltaTime;
                        if (TimePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            Movement.ChangeVerticalVelocity(0f);
                        }
                        else
                        {
                            //start moving downward
                            Movement.ChangeVerticalVelocity(-0.01f);
                        }
                    }
                }

                else if (!IsFastFalling)
                {
                    Movement.IncrementVerticalVelocity(MoveStats.Gravity * Time.fixedDeltaTime);

                    if (IsPastApexThreshold)
                    {
                        IsPastApexThreshold = false;
                    }
                }

            }

            else if (!IsFastFalling)
            {
                Movement.IncrementVerticalVelocity(MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime);
            }

            else if (Movement.VerticalVelocity < 0f)
            {
                if (!IsFalling)
                    IsFalling = true;
            }
        }

        //NORMAL FALLING (Without Jumping)
        if (IsFalling && !IsJumping && !IsGrounded)
        {
            Movement.IncrementVerticalVelocity(MoveStats.Gravity * Time.fixedDeltaTime);
        }

        //HANDLE RELEASED JUMP DECELERATION
        if (IsFastFalling)
        {
            if (FastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                Movement.IncrementVerticalVelocity(MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime);
            }
            else if (FastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                Movement.ChangeVerticalVelocity(Mathf.Lerp(FastFallReleaseSpeed, 0f, (FastFallTime / MoveStats.TimeForUpwardsCancel)));
            }

            FastFallTime += Time.fixedDeltaTime;
        }

        if (MoveStats.DebugShowHeightLogOnLand)
        {
            if (HeightTracker.position.y > HighestPoint)
            {
                HighestPoint = HeightTracker.position.y;
            }
        }
    }

    public void InitiateJump()
    {
        Movement.ChangeVerticalVelocity(MoveStats.InitialJumpVelocity);

        ResetWallJumpValues();
        IsJumping = true;
        NumberOfJumpsUsed++;

        //Anim.SetTrigger(Player.JUMP);
        //Anim.ResetTrigger(Player.LAND);
        if (!TrailRenderer.emitting)
        {
            TrailRenderer.emitting = true;
        }

        if (JumpReleasedDuringBuffer)
        {
            IsFastFalling = true;
            FastFallReleaseSpeed = Movement.VerticalVelocity;
        }

        TrackHeight();
    }

    public void TrackHeight()
    {
        if (MoveStats.DebugShowHeightLogOnLand)
        {
            HeightTrackerStartingPoint = HeightTracker.position.y;
            HighestPoint = HeightTracker.position.y;
        }
    }

    #endregion

    #endregion

    #region Wall Slide

    public bool ShouldWallSlide()
    {
        if (IsTouchingWall && !IsGrounded && !IsDashing)
        {
            if (Movement.VerticalVelocity < 0f && !IsWallSliding)
            {
                return true;
            }
        }

        return false;
    }

    public bool ShouldStopWallSliding()
    {
        if (IsWallSliding && !IsTouchingWall && !IsGrounded && !IsWallSlideFalling)
        {
            return true;
        }

        return false;
    }

    public void StopWallSliding()
    {
        if (IsWallSliding)
        {
            NumberOfJumpsUsed++;

            IsWallSliding = false;
            //Anim.SetBool(IS_WALL_SLIDING, false);
        }
    }

    #endregion

    #region Wall Jump

    public bool CanWallJumpDueToPostBufferTimer()
    {
        if (WallJumpPostBufferTimer > 0f)
        {
            return true;
        }

        return false;
    }

    public void WallJumpChecks()
    {
        if (ShouldApplyPostWallJumpBuffer())
        {
            WallJumpPostBufferTimer = MoveStats.WallJumpPostBufferTime;
        }

        //START FAST FALLING
        if (InputManager.JumpWasReleased)
        {
            WallJumpWasReleased();
        }
    }

    private void WallJumpWasReleased()
    {
        if (!IsWallSliding && !IsTouchingWall && IsWallJumping)
        {
            if (IsWallJumping && Movement.VerticalVelocity > 0f)
            {
                if (IsPastWallJumpApexThreshold)
                {
                    IsPastWallJumpApexThreshold = false;
                    IsWallJumpFastFalling = true;
                    WallJumpFastFallTime = MoveStats.TimeForUpwardsCancel;

                    //gets rid of floatiness
                    Movement.ChangeVerticalVelocity(0f);
                }
                else
                {
                    IsWallJumpFastFalling = true;
                    WallJumpFastFallReleaseSpeed = Movement.VerticalVelocity;
                }
            }
        }
    }

    public void WallJumpPhysics()
    {
        //APPLY WALL JUMP GRAVITY
        if (IsWallJumping)
        {
            //TIME TO TAKE OVER MOVEMENT CONTROLS WHILE WALL JUMPING
            WallJumpTime += Time.fixedDeltaTime;
            if (WallJumpTime >= MoveStats.TimeTillJumpApex)
            {
                UseWallJumpMoveStats = false;
            }

            //HIT HEAD
            if (BumpedHead)
            {
                IsWallJumpFastFalling = true;
                UseWallJumpMoveStats = false;
            }

            //GRAVITY IN ASCENDING
            if (Movement.VerticalVelocity >= 0f)
            {
                //APEX CONTROLS
                WallJumpApexPoint = Mathf.InverseLerp(MoveStats.WallJumpDirection.y, 0f, Movement.VerticalVelocity);

                if (WallJumpApexPoint > MoveStats.ApexThreshold)
                {
                    if (!IsPastWallJumpApexThreshold)
                    {
                        IsPastWallJumpApexThreshold = true;
                        TimePastWallJumpApexThreshold = 0f;
                    }

                    if (IsPastWallJumpApexThreshold)
                    {
                        TimePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (TimePastWallJumpApexThreshold < MoveStats.ApexHangTime)
                        {
                            Movement.ChangeVerticalVelocity(0f);
                        }
                        else
                        {
                            Movement.ChangeVerticalVelocity(-0.01f);
                        }
                    }
                }

                //GRAVITY IN ASCENDING BUT NOT PAST APEX THRESHOLD
                else if (!IsWallJumpFastFalling)
                {
                    Movement.IncrementVerticalVelocity(MoveStats.WallJumpGravity * Time.fixedDeltaTime);

                    if (IsPastWallJumpApexThreshold)
                    {
                        IsPastWallJumpApexThreshold = false;
                    }
                }

            }

            //GRAVITY ON DESCENDING
            else if (!IsWallJumpFastFalling)
            {
                Movement.IncrementVerticalVelocity(MoveStats.WallJumpGravity * Time.fixedDeltaTime);
            }

            else if (Movement.VerticalVelocity < 0f)
            {
                if (!IsWallJumpFalling)
                    IsWallJumpFalling = true;
            }

        }

        //WALL JUMP FALLING
        if (IsWallJumpFastFalling)
        {
            if (WallJumpFastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                Movement.IncrementVerticalVelocity(MoveStats.WallJumpGravity * MoveStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime);
            }
            else if (WallJumpFastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                Movement.ChangeVerticalVelocity(Mathf.Lerp(WallJumpFastFallReleaseSpeed, 0f, (WallJumpFastFallTime / MoveStats.TimeForUpwardsCancel)));
            }

            WallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    public void InitiateWallJump(GameObject particlesToSpawn = null)
    {
        if (!IsWallJumping)
        {
            IsWallJumping = true;
            UseWallJumpMoveStats = true;
        }

        StopWallSliding();

        ResetJumpValues();
        WallJumpTime = 0f;
        Movement.ChangeVerticalVelocity(MoveStats.InitialWallJumpVelocity);

        int dirMultiplier = 0;
        Vector2 hitDir = _lastWallHit.collider.ClosestPoint(BodyColl.bounds.center);

        if (hitDir.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else { dirMultiplier = 1; }

        Vector2 wallJumpDir = new Vector2(Mathf.Abs(MoveStats.WallJumpDirection.x) * dirMultiplier, Movement.VerticalVelocity);
        Movement.Impulse(wallJumpDir, 0f);

        //FX
        //Anim.SetTrigger("jump");
        //Anim.ResetTrigger("land");
        TrailRenderer.emitting = true;

        //Instantiate(particlesToSpawn, _particleSpawnTransform.position, Quaternion.identity);
    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if (!IsGrounded && (IsTouchingWall || IsWallSliding))
        {
            return true;
        }
        else { return false; }
    }

    public void ResetWallJumpValues()
    {
        IsWallSlideFalling = false;
        UseWallJumpMoveStats = false;
        IsWallJumping = false;
        IsWallJumpFastFalling = false;
        IsWallJumpFalling = false;
        IsPastWallJumpApexThreshold = false;

        WallJumpFastFallTime = 0f;
        WallJumpTime = 0f;
    }

    #endregion

    #region Dash

    public void ResetDashes()
    {
        NumberOFDashesUsed = 0;
    }

    public void ResetDashValues()
    {
        IsDashFastFalling = false;
        DashOnGroundTimer = -0.01f;
        //Anim.SetBool(IS_AIR_DASH_FALLING, false);
    }

    public bool CanDash()
    {
        //ground dash
        if (IsGrounded && DashOnGroundTimer < 0 && !IsDashing)
        {
            return true;
        }
        return false;              
    }

    public bool CanAirDash()
    {
        //air dash
        if (!IsGrounded && !IsDashing && NumberOFDashesUsed < MoveStats.NumberOfDashes)
        {
            IsAirDashing = true;

            //you left a wallslide but dashed within the wallJumpPostBufferTimer
            if (WallJumpPostBufferTimer > 0f)
            {
                NumberOfJumpsUsed--;
                if (NumberOfJumpsUsed < 0)
                {
                    NumberOfJumpsUsed = 0;
                }
            }
            return true;
        }
        return false;
    }

    public void InitiateDash()
    {
        DashDirection = InputManager.Movement;

        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(DashDirection, MoveStats.DashDirections[0]);

        for (int i = 0; i < MoveStats.DashDirections.Length; i++)
        {
            //skip if we hit it bang on
            if (DashDirection == MoveStats.DashDirections[i])
            {
                closestDirection = DashDirection;
                break;
            }

            float distance = Vector2.Distance(DashDirection, MoveStats.DashDirections[i]);

            // Check if this is a diagonal direction and apply bias
            bool isDiagonal = (Mathf.Abs(MoveStats.DashDirections[i].x) == 1 && Mathf.Abs(MoveStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= MoveStats.DashDiagonallyBias;
            }

            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = MoveStats.DashDirections[i];
            }
        }

        //handle direction if we have no input
        if (closestDirection == Vector2.zero)
        {
            if (isFacingRight)
            {
                closestDirection = Vector2.right;
            }
            else { closestDirection = Vector2.left; }
        }

        DashDirectionMult = 1; //this may not be needed
        DashDirection = new Vector2(closestDirection.x * DashDirectionMult, closestDirection.y * DashDirectionMult);

        NumberOFDashesUsed++;
        IsDashing = true;
        DashTimer = 0f;
        DashOnGroundTimer = MoveStats.TimeBtwDashesOnGround;

        //FX
        Quaternion particleRot = Quaternion.FromToRotation(Vector2.right, -DashDirection);
        Instantiate(DashParticles, transform.position, particleRot);

        ResetJumpValues();
        ResetWallJumpValues();
        StopWallSliding();
    }

    public void DashPhysics()
    {
        if (IsDashing)
        {
            //stop the dash after the timer
            DashTimer += Time.fixedDeltaTime;
            if (DashTimer >= MoveStats.DashTime)
            {
                if (IsGrounded)
                {
                    ResetDashes();
                }
                //else { Anim.SetBool(IS_AIR_DASH_FALLING, true); }

                IsAirDashing = false;
                IsDashing = false;

                //Anim.SetBool(IS_DASHING, false);

                //start the time for upwards cancel
                if (!IsJumping && !IsWallJumping)
                {
                    DashFastFallTime = 0f;
                    DashFastFallReleaseSpeed = Movement.VerticalVelocity;

                    if (!IsGrounded)
                        IsDashFastFalling = true;
                }

                return;
            }

            Vector2 dashDir = new Vector2(MoveStats.DashSpeed * DashDirection.x, 0f);
            Movement.Impulse(dashDir, 0f);

            if (DashDirection.y != 0f || IsAirDashing)
                Movement.ChangeVerticalVelocity(MoveStats.DashSpeed * DashDirection.y);
        }

        //HANDLE DASH CUT TIME
        else if (IsDashFastFalling)
        {
            //new
            if (Movement.VerticalVelocity > 0f)
            {
                if (DashFastFallTime < MoveStats.DashTimeForUpwardsCancel)
                {
                    Movement.ChangeVerticalVelocity(Mathf.Lerp(DashFastFallReleaseSpeed, 0f, (DashFastFallTime / MoveStats.DashTimeForUpwardsCancel)));
                }
                else if (DashFastFallTime >= MoveStats.DashTimeForUpwardsCancel)
                {
                    Movement.IncrementVerticalVelocity(MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime);
                }

                DashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                Movement.IncrementVerticalVelocity(MoveStats.Gravity * MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime);
            }
        }
    }

    #endregion
    
    #region Collision Checks

    public void CollisionChecks()
    {
        CheckForGrounded();
        CheckForBumpedHead();
        CheckForTouchingWall();
    }

    private void CheckForGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(FeetColl.bounds.center.x, FeetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(FeetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            IsGrounded = true;
        }
        else { IsGrounded = false; }

        #region Debug Visualization
        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (IsGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void CheckForBumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(FeetColl.bounds.center.x, HeadColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(FeetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            BumpedHead = true;
        }
        else { BumpedHead = false; }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (BumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    private void CheckForTouchingWall()
    {
        float originEndPoint = 0f;
        if (isFacingRight)
        {
            originEndPoint = BodyColl.bounds.max.x;
        }
        else { originEndPoint = BodyColl.bounds.min.x; }

        float adjustedHeight = BodyColl.bounds.size.y * MoveStats.WallDetectionRayHeightMultiplier;
        Vector2 boxCastOrigin = new Vector2(originEndPoint, BodyColl.bounds.center.y);
        Vector2 boxCastSize = new Vector2(MoveStats.WallDetectionRayLength, adjustedHeight);

        WallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, MoveStats.WallDetectionRayLength, MoveStats.GroundLayer);
        if (WallHit.collider != null)
        {
            _lastWallHit = WallHit;
            IsTouchingWall = true;
        }
        else { IsTouchingWall = false; }

        #region Debug Visualization

        if (MoveStats.DebugShowWallHitBox)
        {
            Color rayColor;
            if (IsTouchingWall)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
        }

        #endregion
    }

    #endregion

    #region Timers

    public void JumpTimers()
    {
        JumpBufferTimer -= Time.deltaTime;

        //HANDLE COYOTE TIMER
        if (!IsGrounded)
        {
            CoyoteTimer -= Time.deltaTime;
        }
        else { CoyoteTimer = MoveStats.JumpCoyoteTime; }
    }

    public void WallJumpTimers()
    {
        //HANDLE WALL JUMP BUFFER TIMER
        if (!ShouldApplyPostWallJumpBuffer())
        {
            WallJumpPostBufferTimer -= Time.deltaTime;
        }
    }

    public void DashTimers()
    {
        //HANDLE DASH TIMER
        if (IsGrounded)
        {
            DashOnGroundTimer -= Time.deltaTime;
        }
    }

    #endregion

    #region FX

    public void SetToxicBuff(bool buff)
    {
        ToxicBuff = buff;
    }

    public bool IsToxicBuffActive()
    {
        return ToxicBuff;
    }

    public void SpawnJumpParticles(GameObject particlesToSpawn)
    {

        Instantiate(particlesToSpawn, ParticleSpawnTransform.position, Quaternion.identity);
    }

    #endregion

    #region Jump Visualition Tool

    private void OnDrawGizmos()
    {
        if (MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
        }

        if (MoveStats.ShowRunJumpArc)
        {
            DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
        }
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(FeetColl.bounds.center.x, FeetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;
        if (MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else { speed = -moveSpeed; }
        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution;

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;
            float downTime = MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier;

            //ascending
            if (simulationTime < MoveStats.TimeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }

            //apex hang time
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime)
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }

            //descending
            else
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime;

                downTime *= descendTime * descendTime;


                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, downTime);
            }

            drawPoint = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    // If a hit is detected, stop drawing the arc at the hit point
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }
    #endregion

}


