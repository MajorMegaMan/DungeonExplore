using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAction : IEntityMoveAction
{
    ScriptableMoveAction m_scriptableMoveAction;
    int m_animationHashID = 0;
    int m_animationDriveParameter = 0;

    EntityAnimate m_entityAnim;
    IEntity m_owner;

    Vector3 m_destination;

    public string actionName { get { return "SpawnAction"; } }

    public SpawnAction(IEntity owner, EntityAnimate entityAnim, ScriptableMoveAction scriptableMoveAction)
    {
        m_owner = owner;
        m_entityAnim = entityAnim;
        SetMoveAction(scriptableMoveAction);
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
        return m_owner.position;
    }

    public float GetTimeSpeed()
    {
        return m_scriptableMoveAction.animationDriveValue;
    }

    public void BeginAction(IActionable actionableEntity, IEntity target)
    {
        m_destination = m_owner.position;
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        float heightOffset = m_scriptableMoveAction.velocityCurve.Evaluate(1.0f - t);

        Vector3 height = Vector3.up * -heightOffset;

        m_entityAnim.transform.localPosition = height;
    }

    public void EndAction(IActionable actionableEntity)
    {
        m_entityAnim.transform.localPosition = Vector3.zero;
    }

    public void CancelAction(IActionable actionableEntity)
    {
        m_entityAnim.transform.localPosition = Vector3.zero;
    }

    public void Animate(Animator anim)
    {
        anim.SetFloat(m_animationDriveParameter, m_scriptableMoveAction.animationDriveValue);
        anim.CrossFade(m_animationHashID, m_scriptableMoveAction.animationTransitionTime, 0, 0.0f);
    }
}
