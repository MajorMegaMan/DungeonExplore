using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ActionController
{
    IActionable m_actionableEntity;
    //[SerializeField] Transform m_ownerTransform = null;

    [SerializeField] BBB.SimpleTimer m_actionTimer = new BBB.SimpleTimer(1.0f);

    [SerializeField] UnityEvent m_beginActionEvent = new UnityEvent();

    public float actionTime { get { return m_actionTimer.targetTime; } set { m_actionTimer.targetTime = value; } }

    public bool isActioning { get { return m_currentAction != null; } }

    IEntityMoveAction m_currentAction = null;

    delegate void PerformUpdate(float deltaTime);
    PerformUpdate m_performUpdate;

    public ActionController()
    {
        m_performUpdate = Empty;
    }

    public void Init(IActionable entity)
    {
        m_actionableEntity = entity;
    }

    public bool TryBeginAction(IEntityMoveAction moveAction, IEntity lockOnTarget)
    {
        // Perform attack
        if (isActioning)
        {
            return false;
        }
        m_currentAction = moveAction;
        m_performUpdate = InternalPerformAction;

        m_actionableEntity.BeginAction(moveAction);
        moveAction.BeginAction(m_actionableEntity, lockOnTarget);

        m_beginActionEvent.Invoke();

        m_actionTimer.Reset();
        return true;
    }

    // This is the update Tick for the Attack Controller
    public void PerformAction(float deltaTime)
    {
        // This is performed by using a delegate as a sort of state machine to ensure that events are called safely, if the user happens to call it at innappropriate times.
        m_performUpdate.Invoke(deltaTime);
    }

    void InternalPerformAction(float deltaTime)
    {
        m_actionTimer.Tick(deltaTime);
        m_currentAction.PerformAction(m_actionableEntity, m_actionTimer.normalisedTime);

        if (m_actionTimer.IsTargetReached())
        {
            m_actionableEntity.EndAction();
            m_currentAction = null;
            m_performUpdate = Empty;
        }
    }

    static void Empty(float deltaTime)
    {

    }
}
