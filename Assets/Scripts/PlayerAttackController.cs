using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField] PlayerInputReceiver m_inputReceiver;
    [SerializeField] BBB.SimpleTimer m_attackTimer = new BBB.SimpleTimer(1.0f);

    delegate void AttackUpdate();
    AttackUpdate m_attackUpdate;
    bool m_isAttacking = false;

    [SerializeField] Animator debugAnim;
    [SerializeField] AnimationStateID debugAttackState;

    // Start is called before the first frame update
    void Start()
    {
        m_attackUpdate = TryBeginAttack;
        debugAttackState.Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        m_attackUpdate.Invoke();
    }

    void TryBeginAttack()
    {
        if (m_inputReceiver.GetAttack())
        {
            // Perform attack
            if (m_isAttacking)
            {
                return;
            }
            m_attackUpdate = PerformingAttack;
            m_isAttacking = true;
            m_attackTimer.Reset();

            debugAnim.CrossFade(debugAttackState.GetID(), 0.0f);
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
