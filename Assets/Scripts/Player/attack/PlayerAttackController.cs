using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : PlayerBehaviour
{
    [SerializeField] BBB.SimpleTimer m_attackTimer = new BBB.SimpleTimer(1.0f);

    [SerializeField] PlayerController m_playerController;
    [SerializeField] CameraLockon m_lockOn;

    delegate void AttackUpdate();
    AttackUpdate m_attackUpdate;
    bool m_isAttacking = false;

    [SerializeField] Animator debugAnim;
    [SerializeField] PlayerAttackAction debug_attackAction;

    PlayerInputReceiver inputReceiver { get { return playerRef.input; } }

    // Start is called before the first frame update
    void Start()
    {
        debug_attackAction.Initialise(m_playerController.transform);

        m_attackUpdate = TryBeginAttack;
    }

    // Update is called once per frame
    void Update()
    {
        m_attackUpdate.Invoke();
    }

    void TryBeginAttack()
    {
        if (inputReceiver.GetAttack())
        {
            // Perform attack
            if (m_isAttacking)
            {
                return;
            }
            m_attackUpdate = PerformingAttack;
            m_isAttacking = true;

            m_attackTimer.targetTime = debug_attackAction.readyTime;
            m_attackTimer.Reset();

            IPlayerMoveAction attackAction;
            if (m_lockOn.isLockedOn)
            {
                attackAction = debug_attackAction.BeginLockOnAttack(m_lockOn.lockOnTarget);
            }
            else
            {
                attackAction = debug_attackAction.BeginStraghtAttack(m_playerController.heading * m_attackTimer.targetTime * 5);
            }
            m_playerController.BeginAction(attackAction);

            debugAnim.CrossFade(debug_attackAction.GetAnimationHashID(), debug_attackAction.animationTransitionTime, 0, 0.0f);
        }
    }

    void PerformingAttack()
    {
        m_attackTimer.Tick(Time.deltaTime);
        if(m_attackTimer.IsTargetReached())
        {
            m_isAttacking = false;
            m_attackUpdate = TryBeginAttack;
        }
    }
}
