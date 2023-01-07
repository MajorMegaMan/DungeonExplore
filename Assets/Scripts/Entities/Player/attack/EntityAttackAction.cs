using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttackAction
{
    [SerializeField] ScriptableAttackAction m_scriptableAttackAction;
    int m_animationHashID = 0;

    public float readyTime { get { return m_scriptableAttackAction.readyTime; } }
    public float animationTransitionTime { get { return m_scriptableAttackAction.animationTransitionTime; } }

    StraightAttack m_straightMoveAction;
    LockOnAttack m_lockOnMoveAction;

    public void Initialise(Transform origin)
    {
        m_straightMoveAction = new StraightAttack(origin);
        m_lockOnMoveAction = new LockOnAttack(origin);

        SetAttackAction(m_scriptableAttackAction);
    }

    public void SetAttackAction(ScriptableAttackAction attackAction)
    {
        if (m_scriptableAttackAction != null)
        {
            m_straightMoveAction.SetAsAttackAction(attackAction);
            m_lockOnMoveAction.SetAsAttackAction(attackAction);
            m_animationHashID = Animator.StringToHash(attackAction.animationStateID);
        }
    }

    public StraightAttack BeginStraghtAttack(Vector3 direction)
    {
        //m_straightMoveAction.SetDirection(direction);
        return m_straightMoveAction;
    }

    public LockOnAttack BeginLockOnAttack(ILockOnTarget lockOnTarget)
    {
        //m_lockOnMoveAction.SetTarget(lockOnTarget);
        return m_lockOnMoveAction;
    }

    public int GetAnimationHashID()
    {
        return m_animationHashID;
    }
}

public class StraightAttack : IEntityMoveAction
{
    ScriptableAttackAction m_scriptableAttackAction;

    Transform m_origin;
    Vector3 m_direction;

    public StraightAttack(Transform origin)
    {
        this.m_origin = origin;
    }

    public void SetDirection(Vector3 direction)
    {
        this.m_direction = direction;
    }

    public void SetAsAttackAction(ScriptableAttackAction scriptableAttackAction)
    {
        m_scriptableAttackAction = scriptableAttackAction;
    }

    public float GetActionTime()
    {
        return m_scriptableAttackAction.moveTime;
    }

    public Vector3 GetDestination()
    {
        return m_origin.position + m_direction;
    }

    public void BeginAction(IActionable actionableEntity, ILockOnTarget target)
    {
        SetDirection(actionableEntity.GetActionHeading());
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        actionableEntity.ForceMovement(Vector3.ClampMagnitude(m_direction, m_scriptableAttackAction.velocityCurve.Evaluate(t)));
    }
}

public class LockOnAttack : IEntityMoveAction
{
    ScriptableAttackAction m_scriptableAttackAction;

    Transform m_origin;
    ILockOnTarget m_target;

    public LockOnAttack(Transform origin)
    {
        this.m_origin = origin;
    }

    public void SetTarget(ILockOnTarget target)
    {
        this.m_target = target;
    }

    public void SetAsAttackAction(ScriptableAttackAction scriptableAttackAction)
    {
        m_scriptableAttackAction = scriptableAttackAction;
    }

    public float GetActionTime()
    {
        return m_scriptableAttackAction.moveTime;
    }

    public Vector3 GetDestination()
    {
        Vector3 toTarget = m_target.GetTargetPosition() - m_origin.position;

        return m_target.GetTargetPosition() - toTarget.normalized * m_target.GetTargetRadius();
    }

    public void BeginAction(IActionable actionableEntity, ILockOnTarget target)
    {
        SetTarget(target);
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        Vector3 toDestination = GetDestination() - actionableEntity.position;
        actionableEntity.ForceMovement(Vector3.ClampMagnitude(toDestination, m_scriptableAttackAction.velocityCurve.Evaluate(t)));
    }
}

