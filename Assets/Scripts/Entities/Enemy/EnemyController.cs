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

    [SerializeField] GameObject debug_attackTargetObject = null;
    IEntity debug_attackTarget = null;

    //[SerializeField] SimpleLockOnTarget m_targetComponent;

    MovementStateEnum m_preActionState = 0;

    [Header("Entity")]
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    [SerializeField] float m_entityRadius = 1.0f;
    [SerializeField] int m_team = 0;

    public string entityName { get { return "Enemy, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return m_navAgent.speed; } }
    public Vector3 velocity { get { return m_navAgent.velocity; } }
    public float currentSpeed { get { return m_navAgent.velocity.magnitude; } }

    Vector3 m_usableHeading = Vector3.zero;
    public Vector3 heading { get { return m_usableHeading; } }


    ILockOnTargeter m_selfTargeter;
    IEntity m_lockOnTarget;
    IEntity ILockOnTargeter.lockOnTarget { get { return m_lockOnTarget; } set { SetLockOnTarget(value); } }

    private void Awake()
    {
        LockOnManager.RegisterLockOnTarget(this);

        m_selfTargeter = this;

        m_anim.SetEntity(this);

        m_navAgent = GetComponent<NavMeshAgent>();

        InitialiseStateMachine();

        m_attackController.Init(this);
        m_entityAttack.Initialise(transform);
    }

    // Start is called before the first frame update
    void Start()
    {
        debug_attackTarget = debug_attackTargetObject.GetComponent<IEntity>();
        if (debug_attackTarget != null)
        {
            AttackFollowTarget(debug_attackTarget);
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
        Debug.DrawRay(position, velocity, Color.green);
    }

    // Returns the distance remaining to the target
    float UpdateFollowMovement(float distance)
    {
        Vector3 target = m_currentAttackTarget.position;
        Vector3 toTarget = target - transform.position;
        float remainDistance = toTarget.magnitude;

        Vector3 position = target;
        if(distance > 0.0001f)
        {
            position = target - (toTarget / remainDistance) * (distance);
        }
        m_navAgent.SetDestination(position);
        return remainDistance;
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

    void AttackFollowTarget(IEntity attackTarget)
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
            m_anim.anim.CrossFade(m_entityAttack.GetAnimationHashID(), m_entityAttack.GetAnimationTransitionTime(debugActionIndex), 0, 0.0f);

            debugActionIndex = (debugActionIndex + 1) % m_entityAttack.weaponActionCount;
        }
    }

    void SetLockOnTarget(IEntity lockOnTarget)
    {
        m_lockOnTarget = lockOnTarget;
    }

    #region IAcitonable
    public void BeginAction(IEntityMoveAction playerAction)
    {
        m_preActionState = m_movementStateMachine.GetCurrentState();
        m_movementStateMachine.ChangeToState(MovementStateEnum.action);
    }

    public void EndAction()
    {
        m_movementStateMachine.ChangeToState(m_preActionState);
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
        Debug.DrawRay(position, moveDir * m_navAgent.speed, Color.red);
        //transform.position += moveDir * Time.deltaTime * m_navAgent.speed;
        //
        
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
        return m_entityRadius;
    }

    public int GetTeam()
    {
        return m_team;
    }
    #endregion // ! IEntity

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

    #region StateMachine

    public enum MovementStateEnum
    {
        empty,
        follow,
        attackFollow,
        action
    }

    void InitialiseStateMachine()
    {
        var enumArray = System.Enum.GetValues(typeof(MovementStateEnum));
        IState<EnemyController>[] states = new IState<EnemyController>[enumArray.Length];

        states[(int)MovementStateEnum.empty] = new EmptyState();
        states[(int)MovementStateEnum.follow] = new FollowState();
        states[(int)MovementStateEnum.attackFollow] = new AttackFollowState();
        states[(int)MovementStateEnum.action] = new ActionState();

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

        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.SetHeadingToNavAgent();

            var distance = owner.m_currentAttackTarget.GetTargetRadius() + owner.GetTargetRadius();
            var remain = owner.UpdateFollowMovement(distance);
            if (remain < distance + owner.m_entityAttack.GetAttackDistance(owner.debugActionIndex))
            {
                owner.TryBeginAttack(owner.debug_attackTarget, false);
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
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            owner.m_attackController.PerformAction(Time.deltaTime);
        }
    }

    #endregion // ! StateMachine
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
