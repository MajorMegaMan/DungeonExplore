using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IActionable, IEntity
{
    NavMeshAgent m_navAgent;

    PackagedStateMachine<EnemyController> m_movementStateMachine;

    IAttackTarget m_currentAttackTarget = null;

    [SerializeField] ActionController m_attackController;

    [SerializeField] SimpleAttackTarget debug_attackTarget = null;

    [SerializeField] EntityAnimate m_anim;

    public Vector3 position { get { return transform.position; } }
    public float speed { get { return m_navAgent.speed; } }
    public Vector3 velocity { get { return m_navAgent.velocity; } }
    public float currentSpeed { get { return m_navAgent.velocity.magnitude; } }
    public Vector3 heading { get { return m_navAgent.velocity.normalized; } }

    private void Awake()
    {
        m_anim.SetEntity(this);

        m_navAgent = GetComponent<NavMeshAgent>();

        InitialiseStateMachine();

        m_attackController.Init(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (debug_attackTarget != null)
        {
            FollowTarget(debug_attackTarget);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_movementStateMachine.Invoke();
    }

    void FollowTarget(IAttackTarget attackTarget)
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

    #region IAcitonable
    public void BeginAction(IEntityMoveAction playerAction)
    {
        playerAction.GetActionTime();
    }

    public void EndAction()
    {
        if (debug_attackTarget != null)
        {
            FollowTarget(debug_attackTarget);
        }
        else
        {
            m_movementStateMachine.ChangeToState(MovementStateEnum.empty);
        }
    }

    public Vector3 GetActionHeading()
    {
        return transform.forward;
    }

    public Transform GetActionTransform()
    {
        return transform;
    }

    void IActionable.ForceMovement(Vector3 moveDir)
    {
        m_navAgent.Move(moveDir * Time.deltaTime * m_navAgent.speed);
    }
    #endregion // ! IAcitonable

    #region StateMachine

    public enum MovementStateEnum
    {
        empty,
        follow,
        action
    }

    void InitialiseStateMachine()
    {
        var enumArray = System.Enum.GetValues(typeof(MovementStateEnum));
        IState<EnemyController>[] states = new IState<EnemyController>[enumArray.Length];

        states[(int)MovementStateEnum.empty] = new EmptyState();
        states[(int)MovementStateEnum.follow] = new FollowState();
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
            
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            
        }
    }

    class FollowState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            owner.m_navAgent.enabled = true;
            owner.m_navAgent.SetDestination(owner.m_currentAttackTarget.GetAttackTargetPosition());
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {
            
        }

        void IState<EnemyController>.Invoke(EnemyController owner)
        {
            Vector3 target = owner.m_currentAttackTarget.GetAttackTargetPosition();
            Vector3 toTarget = target - owner.transform.position;
            Vector3 position = target - toTarget.normalized * owner.m_currentAttackTarget.GetRadius();
            owner.m_navAgent.SetDestination(position);
        }
    }

    class ActionState : IState<EnemyController>
    {
        void IState<EnemyController>.Enter(EnemyController owner)
        {
            owner.m_navAgent.enabled = false;
        }

        void IState<EnemyController>.Exit(EnemyController owner)
        {

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
