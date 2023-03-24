using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IActionable, IEntity, ILockOnTargeter
{
    NavMeshAgent m_navAgent;

    PackagedStateMachine<EnemyController> m_movementStateMachine;

    IEntity m_currentAttackTarget = null;

    [SerializeField] ActionController m_attackController;
    [SerializeField] EntityAttackAction m_entityAttack;

    [SerializeField] EntityAnimate m_anim;

    [SerializeField] ScriptableMoveAction m_hurtActionSettings;
    HurtAction m_hurtAction;

    //[SerializeField] SimpleLockOnTarget m_targetComponent;

    MovementStateEnum m_preActionState = 0;

    [SerializeField] EnemySettings m_settings;

    [Header("Entity")]
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    //[SerializeField] float m_entityRadius = 1.0f;
    [SerializeField] int m_team = 0;
    [SerializeField] EntityStats m_stats;

    [SerializeField] WeaponHitReceiver m_weaponHitReceiver;
    [SerializeField] SimpleAudioControl m_audio;

    [Header("Movement")]
    //[SerializeField] float m_retargetDistance = 0.2f;

    [Header("AI States")]
    EnemyDirector m_director;
    //[SerializeField] float m_deadTime = 1.0f;

    [Header("Debug")]
    [SerializeField] GameObject debug_attackTargetObject = null;
    IEntity debug_attackTarget = null;

    [SerializeField] bool debug_revive = false;
    public MovementStateEnum debug_currentState;

    public string entityName { get { return "Enemy, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return m_navAgent.speed; } }
    public Vector3 velocity { get { return m_navAgent.velocity; } }
    public float currentSpeed { get { return m_navAgent.velocity.magnitude; } }

    Vector3 m_usableHeading = Vector3.zero;
    public Vector3 heading { get { return m_usableHeading; } }
    public EntityStats entityStats { get { return m_stats; } }


    ILockOnTargeter m_selfTargeter;
    IEntity m_lockOnTarget;
    IEntity ILockOnTargeter.lockOnTarget { get { return m_lockOnTarget; } }

    private void Awake()
    {
        LockOnManager.RegisterLockOnTarget(this);

        m_selfTargeter = this;

        m_anim.SetEntity(this);

        m_navAgent = GetComponent<NavMeshAgent>();

        InitialiseStateMachine();

        m_attackController.Init(this);
        m_entityAttack.Initialise(transform, this);

        m_hurtAction = new HurtAction(transform, m_hurtActionSettings);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(debug_attackTargetObject != null)
        {
            debug_attackTarget = debug_attackTargetObject.GetComponent<IEntity>();
            if (debug_attackTarget != null)
            {
                AttackFollowTarget(debug_attackTarget);
            }
        }

        m_navAgent.updateRotation = false;
    }

    private void OnDestroy()
    {
        LockOnManager.DeregisterLockOnTarget(this);
    }

    // Update is called once per frame
    void Update()
    {
        m_movementStateMachine.Invoke();
        debug_currentState = m_movementStateMachine.GetCurrentState();
    }

    #region Director
    public void SetDirector(EnemyDirector director)
    {
        m_director = director;
    }

    public void ReceiveAttackSignal(IEntity attackTarget)
    {
        TryBeginAttack(attackTarget, false);
    }

    void RequestAttack()
    {
        if(m_director != null)
        {
            m_director.RequestAttack(this);
        }
        else
        {
            TryBeginAttack(debug_attackTarget, false);
        }
    }

    void RevokeAttack()
    {
        if (m_director != null)
        {
            m_director.RevokeAttack(this);
        }
    }

    #endregion // ! Director

    // Returns the distance remaining to the target
    float UpdateFollowMovement(float distance)
    {
        Vector3 target = m_currentAttackTarget.position;
        Vector3 toTarget = target - transform.position;
        float remainDistance = toTarget.magnitude;

        Vector3 position = target;
        if(remainDistance > 0.0001f)
        {
            position = target - (toTarget / remainDistance) * (distance);
        }
        // Only applying movement when outside the retarget distance should help remove the stuttering when standing in one location.
        if(remainDistance > m_settings.retargetDistance)
        {
            m_navAgent.SetDestination(position);
        }
        return remainDistance;
    }

    // Sets the state to Empty
    public void StopDoingAnything()
    {
        // Sets the state to Empty
        m_currentAttackTarget = null;
        m_movementStateMachine.ChangeToState(MovementStateEnum.empty);
    }

    void FollowTarget(IEntity attackTarget)
    {
        if(attackTarget != null)
        {
            m_currentAttackTarget = attackTarget;
            m_movementStateMachine.ChangeToState(MovementStateEnum.follow);
        }
        else
        {
            // Change to an empty state for now, but the agent should find the behaviour it would like to be doing.
            m_currentAttackTarget = null;
            m_movementStateMachine.ChangeToState(MovementStateEnum.empty);
        }
    }

    public void AttackFollowTarget(IEntity attackTarget)
    {
        if (attackTarget != null)
        {
            m_currentAttackTarget = attackTarget;
            //m_selfTargeter.lockOnTarget = attackTarget.
            m_movementStateMachine.ChangeToState(MovementStateEnum.attackFollow);
        }
        else
        {
            // Change to an empty state for now, but the agent should find the behaviour it would like to be doing.
            m_currentAttackTarget = null;
            m_movementStateMachine.ChangeToState(MovementStateEnum.empty);
        }
    }

    int debugActionIndex = 0;
    void TryBeginAttack(IEntity attackTarget, bool lockOnAttack)
    {
        IEntityMoveAction attackAction;
        if (lockOnAttack)
        {
            attackAction = m_entityAttack.BeginLockOnAttack(debugActionIndex);
        }
        else
        {
            Vector3 toTarget = attackTarget.position - position;
            toTarget = toTarget.normalized;
            attackAction = m_entityAttack.BeginStraghtAttack(debugActionIndex);
            //attackAction = m_entityAttack.BeginStraghtAttack(heading);
        }
        if (m_attackController.TryBeginAction(attackAction, attackTarget))
        {
            m_entityAttack.Animate(m_anim.anim, debugActionIndex);

            debugActionIndex = (debugActionIndex + 1) % m_entityAttack.weaponActionCount;
        }
    }

    #region ILockOnTargeter
    void ILockOnTargeter.SetLockOnTarget(IEntity lockOnTarget)
    {
        m_lockOnTarget = lockOnTarget;
    }
    #endregion // ! ILockOnTargeter

    #region IAcitonable
    public void BeginAction(IEntityMoveAction playerAction)
    {
        var currentState = m_movementStateMachine.GetCurrentState();
        if(currentState != MovementStateEnum.action)
        {
            m_preActionState = currentState;
        }
        m_movementStateMachine.ChangeToState(MovementStateEnum.action);
    }

    public void EndAction()
    {
        m_movementStateMachine.ChangeToState(m_preActionState);
    }

    public void CancelAction()
    {
        m_movementStateMachine.ChangeToState(m_preActionState);
    }

    public void SwitchAction(IEntityMoveAction previous, IEntityMoveAction next)
    {
        
    }

    public Vector3 GetActionHeading()
    {
        return m_usableHeading;
        //return transform.forward;
    }

    public Transform GetActionTransform()
    {
        return transform;
    }

    void IActionable.ForceMovement(Vector3 moveDir)
    {        
        m_navAgent.Move(moveDir * Time.deltaTime * m_navAgent.speed);

        m_navAgent.velocity = moveDir * m_navAgent.speed;
        //m_navAgent.nextPosition = position;
    }
    #endregion // ! IAcitonable

    #region IEntity
    public Bounds GetAABB()
    {
        return m_renderer.bounds;
    }

    public Transform GetCameraLookTransform()
    {
        return m_lockOnTransform;
    }

    public float GetTargetRadius()
    {
        return m_settings.entityRadius;
    }

    public int GetTeam()
    {
        return m_team;
    }

    public void ReceiveHit(IEntity attacker)
    {
        m_attackController.ForceBeginAction(m_hurtAction, attacker);
        m_hurtAction.Animate(m_anim.anim);

        entityStats.ReceiveDamage(attacker.entityStats.CalculateAttackStrength());

        // Turn towards the attacker
        var toAttacker = attacker.position - position;
        m_usableHeading = toAttacker.normalized;

        if(entityStats.IsDead())
        {
            // Should Die
            Die();
        }
        else
        {
            m_audio.PlayHurt();
        }
    }
    #endregion // ! IEntity

    void Die()
    {
        m_audio.PlayDie();
        m_movementStateMachine.ChangeToState(MovementStateEnum.dead);
    }

    void EnterDeadState()
    {
        m_weaponHitReceiver.gameObject.SetActive(false);
        m_navAgent.enabled = false;
        m_anim.SetAnimToDeath();
        LockOnManager.DeregisterLockOnTarget(this);
    }

    void ExitDeadState()
    {
        m_weaponHitReceiver.gameObject.SetActive(true);
        m_navAgent.enabled = true;
        m_anim.SetAnimToMovement();
        LockOnManager.RegisterLockOnTarget(this);
    }

    public void Revive()
    {
        entityStats.HealToFull();
        if (m_movementStateMachine.GetCurrentState() == MovementStateEnum.dead)
        {
            // Debug Switch to auto aggro
            AttackFollowTarget(debug_attackTarget);
        }
    }

    public void ResetEntityStats()
    {
        m_stats.CopyStats(m_settings.defaultStats);
    }

    void SetHeadingToNavAgent()
    {
        //if(currentSpeed > 0.0001f)
        //{
        //    m_usableHeading = velocity / currentSpeed;
        //}

        // use desired velocity as that is the direction that the agent wants to travel.
        // if using actual velocity, There are many times  that the agent will stand still and this does not give it enough time to find it's travel direction between attacks.
        // Resulting in another attack the same direction which is not usually the correct attack direction.
        float desiredSpeed = m_navAgent.desiredVelocity.magnitude;
        if (desiredSpeed > 0.0001f)
        {
            m_usableHeading = m_navAgent.desiredVelocity / desiredSpeed;
        }
    }

    // Used to teleport the enemy
    public void Warp(Vector3 position)
    {
        m_navAgent.Warp(position);
    }

    #region StateMachine

    public enum MovementStateEnum
    {
        empty,
        follow,
        attackFollow,
        action,
        dead
    }

    void InitialiseStateMachine()
    {
        var enumArray = System.Enum.GetValues(typeof(MovementStateEnum));
        IState<EnemyController>[] states = new IState<EnemyController>[enumArray.Length];

        states[(int)MovementStateEnum.empty] = new EmptyState();
        states[(int)MovementStateEnum.follow] = new FollowState();
        states[(int)MovementStateEnum.attackFollow] = new AttackFollowState();
        states[(int)MovementStateEnum.action] = new ActionState();
        states[(int)MovementStateEnum.dead] = new DeadState();

        m_movementStateMachine = new PackagedStateMachine<EnemyController>(this, states);
        m_movementStateMachine.InitialiseState(MovementStateEnum.empty);
    }

    class EmptyState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            owner.m_navAgent.enabled = false;
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            owner.m_navAgent.enabled = true;
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.SetHeadingToNavAgent();
        }
    }

    class FollowState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            owner.m_navAgent.SetDestination(owner.m_currentAttackTarget.position);
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.SetHeadingToNavAgent();

            var distance = owner.m_currentAttackTarget.GetTargetRadius() + owner.GetTargetRadius();
            owner.UpdateFollowMovement(distance);
        }
    }

    class AttackFollowState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            owner.m_navAgent.SetDestination(owner.m_currentAttackTarget.position);
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            owner.RevokeAttack();
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.SetHeadingToNavAgent();

            var distance = owner.m_currentAttackTarget.GetTargetRadius() + owner.GetTargetRadius();
            var remain = owner.UpdateFollowMovement(distance);
            if (remain < distance + owner.m_entityAttack.GetAttackDistance(owner.debugActionIndex))
            {
                //owner.TryBeginAttack(owner.debug_attackTarget, false);
                owner.RequestAttack();
            }
            else
            {
                owner.RevokeAttack();
            }
        }
    }

    class ActionState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            //owner.m_navAgent.enabled = false;
            //owner.m_navAgent.updatePosition = false;
            //owner.m_navAgent.updateRotation = false;
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            //owner.m_navAgent.enabled = true;
            //owner.m_navAgent.updatePosition = true;
            //owner.m_navAgent.updateRotation = true;

            if(owner.m_attackController.isActioning)
            {
                owner.m_attackController.RawCancelAction();
            }
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.m_attackController.PerformAction(Time.deltaTime);
        }
    }

    class DeadState : IState<EnemyController>
    {
        BBB.SimpleTimer m_deadTimer;

        public DeadState()
        {
            m_deadTimer = new BBB.SimpleTimer();
        }

        void IState<EnemyController>.Enter(EnemyController owner)
        {
            m_deadTimer.targetTime = owner.m_settings.deadTime;
            m_deadTimer.Reset();
            owner.EnterDeadState();
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            owner.ExitDeadState();
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            m_deadTimer.Tick(Time.deltaTime);
            if(m_deadTimer.IsTargetReached())
            {
                owner.m_director.DespawnEnemy(owner);
            }
        }
    }

    #endregion // ! StateMachine

    private void OnValidate()
    {
        if(Application.isPlaying && m_movementStateMachine != null)
        {
            if(debug_revive)
            {
                debug_revive = false;
                Revive();
            }
        }
    }
}

public static class PackagedSMExtensionEnemy
{
    public static void InitialiseState(this PackagedStateMachine<EnemyController> packagedStateMachine, EnemyController.MovementStateEnum selectionState)
    {
        packagedStateMachine.InitialiseState((int)selectionState);
    }

    public static void ChangeToState(this PackagedStateMachine<EnemyController> packagedStateMachine, EnemyController.MovementStateEnum selectionState)
    {
        packagedStateMachine.ChangeToState((int)selectionState);
    }

    public static EnemyController.MovementStateEnum GetCurrentState(this PackagedStateMachine<EnemyController> packagedStateMachine)
    {
        return (EnemyController.MovementStateEnum)packagedStateMachine.currentIndex;
    }
}
