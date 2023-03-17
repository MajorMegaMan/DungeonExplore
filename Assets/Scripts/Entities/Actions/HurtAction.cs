using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtAction : IEntityMoveAction
{
    ScriptableMoveAction m_scriptableMoveAction;
    int m_animationHashID = 0;
    int m_animationDriveParameter = 0;

    Transform m_origin;
    Vector3 m_direction;

    public string actionName { get { return "HurtAction"; } }

    public HurtAction(Transform origin, ScriptableMoveAction scriptableMoveAction)
    {
        this.m_origin = origin;
        SetMoveAction(scriptableMoveAction);
    }

    public void SetDirection(Vector3 direction)
    {
        this.m_direction = direction;
    }

    public void SetMoveAction(ScriptableMoveAction scriptableMoveAction)
    {
        m_scriptableMoveAction = scriptableMoveAction;
        m_animationHashID = Animator.StringToHash(scriptableMoveAction.animationStateID);
        m_animationDriveParameter = Animator.StringToHash(scriptableMoveAction.animationDriveParameter);
    }

    public float GetActionTime()
    {
        return m_scriptableMoveAction.moveTime;
    }

    public Vector3 GetDestination()
    {
        return m_origin.position + m_direction;
    }

    public float GetTimeSpeed()
    {
        return m_scriptableMoveAction.animationDriveValue;
    }

    public void BeginAction(IActionable actionableEntity, IEntity target)
    {
        if (target == null)
        {
            SetDirection(actionableEntity.GetActionHeading());
        }
        else
        {
            var toTarget = target.position - actionableEntity.position;
            SetDirection(toTarget.normalized);
        }
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        actionableEntity.ForceMovement(Vector3.ClampMagnitude(-m_direction, m_scriptableMoveAction.velocityCurve.Evaluate(t)));
    }

    public void CancelAction(IActionable actionableEntity)
    {
        
    }

    public void Animate(Animator anim)
    {
        anim.SetFloat(m_animationDriveParameter, m_scriptableMoveAction.animationDriveValue);
        anim.CrossFade(m_animationHashID, m_scriptableMoveAction.animationTransitionTime, 0, 0.0f);
    }
}
