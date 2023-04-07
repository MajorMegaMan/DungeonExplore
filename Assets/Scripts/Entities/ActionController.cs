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

    bool m_isActioning = false;

    public float actionTime { get { return m_actionTimer.targetTime; } set { m_actionTimer.targetTime = value; } }
    public bool isActioning { get { return m_isActioning; } }

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

        m_actionableEntity.BeginAction(moveAction);
        InternalStartPerformAction(moveAction, lockOnTarget);
        return true;
    }

    public void ForceBeginAction(IEntityMoveAction moveAction, IEntity lockOnTarget)
    {
        if (isActioning)
        {
            m_currentAction.CancelAction(m_actionableEntity);
            m_actionableEntity.SwitchAction(m_currentAction, moveAction);
        }
        else
        {
            m_actionableEntity.BeginAction(moveAction);
        }

        InternalStartPerformAction(moveAction, lockOnTarget);
    }

    // Sets the required variables for an action to begin.
    void InternalStartPerformAction(IEntityMoveAction moveAction, IEntity lockOnTarget)
    {
        m_currentAction = moveAction;
        m_performUpdate = InternalPerformAction;

        moveAction.BeginAction(m_actionableEntity, lockOnTarget);

        m_beginActionEvent.Invoke();

        m_actionTimer.targetTime = moveAction.GetActionTime();
        m_actionTimer.Reset();

        m_isActioning = true;
    }

    // This is the update Tick for the Attack Controller
    public void PerformAction(float deltaTime)
    {
        // This is performed by using a delegate as a sort of state machine to ensure that events are called safely, if the user happens to call it at innappropriate times.
        m_performUpdate.Invoke(deltaTime);
    }

    void InternalPerformAction(float deltaTime)
    {
        m_actionTimer.Tick(deltaTime * m_currentAction.GetTimeSpeed());
        m_currentAction.PerformAction(m_actionableEntity, m_actionTimer.normalisedTime);

        if (m_actionTimer.IsTargetReached())
        {
            m_currentAction.EndAction(m_actionableEntity);
            m_currentAction = null;
            // Must set isAction to false before calling end.
            m_isActioning = false;
            m_actionableEntity.EndAction();
            m_performUpdate = Empty;
        }
    }

    // Cancels without calling the actionableEntity Cancel. This is used when the actionable entity has chosen to cancel the action and therefore does not need to use it's own cancel.
    // ie. A goblin is trying to pick up a sword on the ground but the player has reached it first. 
    // The logic for cancelling the action would be held by the goblin. The goblin would need to stop the current action and find a new one. It did not complete the action and is not auto updated.
    public void RawCancelAction()
    {
        if(m_isActioning)
        {
            m_isActioning = false;
            m_currentAction.CancelAction(m_actionableEntity);
            m_currentAction = null;
            m_performUpdate = Empty;
        }
    }

    static void Empty(float deltaTime)
    {

    }
}
