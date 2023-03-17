using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : PlayerBehaviour, IActionable, IEntity
{
    PlayerInputReceiver inputReceiver { get { return playerRef.input; } }

    [SerializeField] CharacterController m_characterControl = null;

    Vector3 m_moveInput = Vector3.zero;
    bool m_jumpInput = false;

    Vector3 m_velocity = Vector3.zero;
    float m_currentSpeed = 0.0f;
    Vector3 m_heading = Vector3.forward;

    // Movement
    [Header("Movement")]
    [SerializeField] float m_speed = 5.0f;
    [SerializeField] float m_acceleration = 200.0f;
    [SerializeField] float m_headingAllowance = 0.0001f;

    [Header("Jumping")]
    [SerializeField] float m_jumpHeight = 3.0f;

    [SerializeField] BBB.EventTimer m_jumpSquatTimer = new BBB.EventTimer(0.1f);
    [SerializeField] UnityEvent m_jumpStartEvent;
    [SerializeField] UnityEvent m_landEvent;

    [SerializeField] float m_jumpGroundedLockoutTime = 0.1f;
    bool m_jumpLockout = false;
    [SerializeField] float m_coyoteTime = 0.5f;
    bool m_canGroundJump = false;

    [SerializeField] int m_airJumpCount = 1;
    int m_currentRemainingAirJump = 1;
    [SerializeField] UnityEvent m_airjumpEvent;

    // Ground Checking
    [Header("GroundChecking")]
    [SerializeField] float m_groundRayDistance = 0.02f;
    [SerializeField] float m_gravity = Physics.gravity.y;
    [SerializeField] LayerMask m_groundLayer = ~0;
    [SerializeField] float m_maxAngle = 45.0f;
    [SerializeField] float m_slopeLimit = 0.2f; // Measured in Dot product at the moment.

    [SerializeField] UnityEvent m_fallEvent;

    float m_airborneTimer = 0.0f;

    Vector3 m_groundNormal = Vector3.up;

    float m_verticalVelocity = 0.0f;

    PackagedStateMachine<PlayerController> m_groundedStateMachine;
    RaycastHit m_lastGroundHit;

    const float GROUND_ERROR = 0.1f;

    // Action variables
    [Header("Attack Controller")]
    [SerializeField] ActionController m_attackController;
    [SerializeField] EntityAttackAction m_entityAttack;

    [Header("Entity")]
    [SerializeField] float m_entityRadius = 1.0f;
    [SerializeField] int m_team = 0;
    [SerializeField] EntityStats m_stats;

    [SerializeField] ScriptableMoveAction m_hurtActionSettings;
    HurtAction m_hurtAction;

    [SerializeField] WeaponHitReceiver m_weaponHitReceiver;

    [Header("Navigation")]
    [SerializeField] UnityEngine.AI.NavMeshObstacle m_navMeshObstacle;

    [Header("Debug")]
    [SerializeField] bool debug_invulnerable = false;
    [SerializeField] bool debug_revive = false;

    // Getters
    public string entityName { get { return "Player, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return m_speed; } }
    public Vector3 velocity { get { return m_velocity; } }
    public float currentSpeed { get { return m_currentSpeed; } }
    public Vector3 heading { get { return m_heading; } }
    public EntityStats entityStats { get { return m_stats; } }

    // totalVelocity is the movement velocity + vertical Velocity
    public Vector3 totalVelocity { get { return m_velocity + Vector3.up * m_verticalVelocity; } }

    public Vector3 groundNormal { get { return m_groundNormal; } }

    public bool isGrounded { get { return m_groundedStateMachine.GetCurrentState() != GroundedStateEnum.airborne; } }

    protected override void Awake()
    {
        base.Awake();

        InitialiseStateMachine();

        m_jumpSquatTimer.targetReachedEvent.AddListener(JumpRelease);

        m_attackController.Init(this);
        m_entityAttack.Initialise(transform, this);

        m_hurtAction = new HurtAction(transform, m_hurtActionSettings);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();

        if (m_jumpInput)
        {
            if(m_canGroundJump)
            {
                JumpBegin();
            }
        }

        if(inputReceiver.GetAttack())
        {
            TryBeginAttack();
        }

        m_groundedStateMachine.Invoke();
    }

    private void LateUpdate()
    {
        m_navMeshObstacle.velocity = velocity;
    }

    void UpdateMovement()
    {
        Vector3 moveDir = m_moveInput;
        UpdateMovement(m_moveInput);
    }

    public void UpdateMovement(Vector3 moveDir)
    {
        float inputMagnitude = moveDir.magnitude;

        Vector3 acceleration = (moveDir * m_acceleration * Time.deltaTime);
        Vector3 limiter = BBB.CharacterPhysics.LimitAcceleration(velocity, moveDir, inputMagnitude, m_acceleration, Time.deltaTime, m_speed, float.MaxValue);
        acceleration += limiter;

        m_velocity += acceleration;
        m_currentSpeed = m_velocity.magnitude;

        if (m_currentSpeed > m_headingAllowance)
        {
            m_heading = m_velocity / m_currentSpeed;
        }

        Vector3 moveVector = m_velocity + Vector3.up * m_verticalVelocity;
        if (isGrounded)
        {
            // Redirect movement based on the ground normal
            Quaternion normalRot = Quaternion.FromToRotation(Vector3.up, m_groundNormal);
            moveVector = normalRot * moveVector;
        }

        m_characterControl.Move(moveVector * Time.deltaTime);
    }

    Vector3 CalculateHeightOffset()
    {
        float halfHeight = m_characterControl.height * 0.5f;
        Vector3 heightOffset = Vector3.up * Mathf.Max((halfHeight - m_characterControl.radius), 0.0f); // height offset cannot be negative.
        return heightOffset;
    }

    bool CheckForGround(out RaycastHit hitInfo)
    {
        Vector3 origin = transform.position + Vector3.up * GROUND_ERROR;
        bool castCheck = Physics.Raycast(origin, Vector3.down, out hitInfo, m_groundRayDistance + m_characterControl.skinWidth + GROUND_ERROR, m_groundLayer, QueryTriggerInteraction.Ignore);

        if(castCheck)
        {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            if(angle > m_characterControl.slopeLimit)
            {
                return false;
            }

            //float slopeDot = Vector3.Dot(Vector3.up, hitInfo.normal);
            //if(slopeDot < m_slopeLimit)
            //{
            //    // too steep
            //    return false;
            //}
        }

        return castCheck;
    }

    // checks for ground and updates the last ground hit
    bool CheckForGroundUpdate()
    {
        bool groundCheck = CheckForGround(out RaycastHit hitInfo);
        m_lastGroundHit = hitInfo;
        return groundCheck;
    }

    void GetInputs()
    {
        m_moveInput = inputReceiver.GetMovement();
        m_jumpInput = inputReceiver.GetJump();
    }

    void JumpBegin()
    {
        m_jumpStartEvent.Invoke();
        m_groundedStateMachine.ChangeToState(GroundedStateEnum.jumpSquat);
    }

    void JumpRelease()
    {
        m_jumpLockout = true;
        m_groundedStateMachine.ChangeToState(GroundedStateEnum.airborne);
        m_verticalVelocity = BBB.CharacterPhysics.CalculateJumpForce(m_jumpHeight, m_gravity);
        m_canGroundJump = false;
    }

    void AirJump()
    {
        m_currentRemainingAirJump--;
        m_jumpLockout = true;
        m_groundedStateMachine.ChangeToState(GroundedStateEnum.airborne);
        m_verticalVelocity = BBB.CharacterPhysics.CalculateJumpForce(m_jumpHeight, m_gravity);
        m_airjumpEvent.Invoke();
    }

    void TryBeginAttack()
    {
        IEntityMoveAction attackAction;
        if (playerRef.lockOn.isLockedOn)
        {
            attackAction = m_entityAttack.BeginLockOnAttack();
        }
        else
        {
            attackAction = m_entityAttack.BeginStraghtAttack();
        }
        if (m_attackController.TryBeginAction(attackAction, playerRef.lockOn.lockOnTarget))
        {
            m_entityAttack.Animate(playerRef.animate.anim);
        }
    }

    #region IEntity
    static Bounds _zeroBounds = new Bounds();
    public Bounds GetAABB()
    {
        return _zeroBounds;
    }

    public Transform GetCameraLookTransform()
    {
        return null;
    }

    public float GetTargetRadius()
    {
        return m_entityRadius;
    }

    public int GetTeam()
    {
        return m_team;
    }

    public void ReceiveHit(IEntity attacker)
    {
        if(debug_invulnerable)
        {
            // Exit early to prevent a hit.
            return;
        }
        m_attackController.ForceBeginAction(m_hurtAction, attacker);
        m_hurtAction.Animate(playerRef.animate.anim);

        entityStats.ReceiveDamage(attacker.entityStats.CalculateAttackStrength());

        //// Turn towards the attacker
        //var toAttacker = attacker.position - position;
        //m_usableHeading = toAttacker.normalized;

        m_velocity = Vector3.zero;

        if (entityStats.IsDead())
        {
            // Should Die
            Die();
        }
    }
    #endregion // ! IEntity

    void Die()
    {
        m_groundedStateMachine.ChangeToState(GroundedStateEnum.dead);
    }

    void EnterDeadState()
    {
        m_weaponHitReceiver.gameObject.SetActive(false);
        playerRef.animate.SetAnimToDeath();
    }

    void ExitDeadState()
    {
        m_weaponHitReceiver.gameObject.SetActive(true);
        playerRef.animate.SetAnimToMovement();
    }

    public void Revive()
    {
        entityStats.HealToFull();
        if(m_groundedStateMachine.GetCurrentState() == GroundedStateEnum.dead)
        {
            SmartSetControllableState();
        }
    }

    #region IActionable
    public void BeginAction(IEntityMoveAction playerAction)
    {
        m_groundedStateMachine.ChangeToState(GroundedStateEnum.actioned);
    }

    public void EndAction()
    {
        SmartSetControllableState();
    }

    public void CancelAction()
    {
        SmartSetControllableState();
    }

    public void SwitchAction(IEntityMoveAction previous, IEntityMoveAction next)
    {

    }

    public Vector3 GetActionHeading()
    {
        return m_heading;
    }

    public Transform GetActionTransform()
    {
        return transform;
    }

    void IActionable.ForceMovement(Vector3 moveDir)
    {
        UpdateMovement(moveDir);
    }
    #endregion // ! IActionable

    void SmartSetControllableState()
    {
        if (CheckForGroundUpdate())
        {
            m_groundedStateMachine.ChangeToState(GroundedStateEnum.grounded);
        }
        else
        {
            m_groundedStateMachine.ChangeToState(GroundedStateEnum.airborne);
        }
    }

    #region GroundedStates

    public enum GroundedStateEnum
    {
        grounded,
        airborne,
        jumpSquat,
        actioned,
        dead
    }

    void InitialiseStateMachine()
    {
        var enumArray = System.Enum.GetValues(typeof(GroundedStateEnum));
        IState<PlayerController>[] groundedStates = new IState<PlayerController>[enumArray.Length];
        groundedStates[(int)GroundedStateEnum.grounded] = new GroundedState();
        groundedStates[(int)GroundedStateEnum.airborne] = new AirborneState();
        groundedStates[(int)GroundedStateEnum.jumpSquat] = new JumpSquatState();
        groundedStates[(int)GroundedStateEnum.actioned] = new ActionState();
        groundedStates[(int)GroundedStateEnum.dead] = new DeadState();

        m_groundedStateMachine = new PackagedStateMachine<PlayerController>(this, groundedStates);

        if (CheckForGroundUpdate())
        {
            m_groundedStateMachine.InitialiseState(GroundedStateEnum.grounded);
        }
        else
        {
            m_groundedStateMachine.InitialiseState(GroundedStateEnum.airborne);
        }
    }

    class GroundedState : IState<PlayerController>
    {
        void IState<PlayerController>.Enter(PlayerController owner)
        {
            owner.m_verticalVelocity = 0.0f;
            owner.m_groundNormal = owner.m_lastGroundHit.normal;

            // Force the character to collide with the ground and snap to the skin width
            owner.m_characterControl.Move(-owner.m_groundNormal * (owner.m_characterControl.skinWidth + owner.m_groundRayDistance));

            owner.m_landEvent.Invoke();

            owner.m_canGroundJump = true;

            owner.m_currentRemainingAirJump = owner.m_airJumpCount;
        }

        void IState<PlayerController>.Exit(PlayerController owner)
        {
            owner.m_airborneTimer = 0.0f;
        }

        void IState<PlayerController>.Invoke(PlayerController owner)
        {
            if (owner.m_jumpInput)
            {
                owner.JumpBegin();
                owner.m_groundedStateMachine.Invoke();
                return;
            }

            if (owner.CheckForGroundUpdate())
            {
                owner.m_groundNormal = owner.m_lastGroundHit.normal;
            }
            else
            {
                owner.m_verticalVelocity += owner.m_gravity * Time.deltaTime;
                owner.m_groundedStateMachine.ChangeToState(GroundedStateEnum.airborne);
                owner.m_jumpLockout = false;
                owner.m_fallEvent.Invoke();
            }

            owner.UpdateMovement();
        }
    }

    class JumpSquatState : IState<PlayerController>
    {
        void IState<PlayerController>.Enter(PlayerController owner)
        {
            owner.m_jumpSquatTimer.Reset();
            owner.m_jumpSquatTimer.Start();
        }

        void IState<PlayerController>.Exit(PlayerController owner)
        {
            owner.m_jumpSquatTimer.Stop();
        }

        void IState<PlayerController>.Invoke(PlayerController owner)
        {
            owner.m_jumpSquatTimer.Update(Time.deltaTime);
            owner.UpdateMovement();
        }
    }

    class AirborneState : IState<PlayerController>
    {
        void IState<PlayerController>.Enter(PlayerController owner)
        {
            owner.m_groundNormal = Vector3.up;
        }

        void IState<PlayerController>.Exit(PlayerController owner)
        {
            owner.m_jumpLockout = false;
        }

        void IState<PlayerController>.Invoke(PlayerController owner)
        {
            if (owner.m_jumpInput)
            {
                if (owner.m_canGroundJump)
                {
                    owner.JumpBegin();
                    owner.m_groundedStateMachine.Invoke();
                    return;
                }
                else if(owner.m_currentRemainingAirJump > 0)
                {
                    // should air jump
                    owner.AirJump();
                }
            }

            owner.m_airborneTimer += Time.deltaTime;
            if(owner.m_airborneTimer > owner.m_coyoteTime)
            {
                owner.m_canGroundJump = false;
            }

            if (owner.CheckForGroundUpdate())
            {
                if(owner.m_jumpLockout)
                {
                    if (owner.m_airborneTimer > owner.m_jumpGroundedLockoutTime)
                    {
                        owner.m_jumpLockout = false;
                        if (owner.m_lastGroundHit.distance < owner.m_characterControl.skinWidth + owner.m_groundRayDistance)
                        {
                            owner.m_groundedStateMachine.ChangeToState(GroundedStateEnum.grounded);
                        }
                        else
                        {
                            owner.m_verticalVelocity += owner.m_gravity * Time.deltaTime;
                        }
                    }
                    else
                    {
                        owner.m_verticalVelocity += owner.m_gravity * Time.deltaTime;
                    }
                }
                else
                {
                    if (owner.m_lastGroundHit.distance < owner.m_characterControl.skinWidth + owner.m_groundRayDistance)
                    {
                        owner.m_groundedStateMachine.ChangeToState(GroundedStateEnum.grounded);
                    }
                    else
                    {
                        owner.m_verticalVelocity += owner.m_gravity * Time.deltaTime;
                    }
                }
            }
            else
            {
                owner.m_verticalVelocity += owner.m_gravity * Time.deltaTime;
            }

            owner.UpdateMovement();
        }
    }

    class ActionState : IState<PlayerController>
    {
        void IState<PlayerController>.Enter(PlayerController owner)
        {
            //owner.m_actionTimer.Reset();
        }

        void IState<PlayerController>.Exit(PlayerController owner)
        {
            if (owner.m_attackController.isActioning)
            {
                owner.m_attackController.RawCancelAction();
            }
        }

        void IState<PlayerController>.Invoke(PlayerController owner)
        {
            owner.m_attackController.PerformAction(Time.deltaTime);
        }
    }

    class DeadState : IState<PlayerController>
    {
        void IState<PlayerController>.Enter(PlayerController owner)
        {
            owner.EnterDeadState();
        }

        void IState<PlayerController>.Exit(PlayerController owner)
        {
            owner.ExitDeadState();
        }

        void IState<PlayerController>.Invoke(PlayerController owner)
        {

        }
    }
    #endregion // ! GroundedStates

    private void OnValidate()
    {
        if (Application.isPlaying && m_groundedStateMachine != null)
        {
            if (debug_revive)
            {
                debug_revive = false;
                Revive();
            }
        }
    }
}

public static class PackagedSMExtensionPlayer
{
    public static void InitialiseState(this PackagedStateMachine<PlayerController> packagedStateMachine, PlayerController.GroundedStateEnum selectionState)
    {
        packagedStateMachine.InitialiseState((int)selectionState);
    }

    public static void ChangeToState(this PackagedStateMachine<PlayerController> packagedStateMachine, PlayerController.GroundedStateEnum selectionState)
    {
        packagedStateMachine.ChangeToState((int)selectionState);
    }

    public static PlayerController.GroundedStateEnum GetCurrentState(this PackagedStateMachine<PlayerController> packagedStateMachine)
    {
        return (PlayerController.GroundedStateEnum)packagedStateMachine.currentIndex;
    }
}