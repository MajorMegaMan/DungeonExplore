using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableAttackAction : ScriptableObject
{
    [SerializeField] float m_readyTime = 1.0f;
    [SerializeField] float m_moveTime = 1.0f;
    [SerializeField] AnimationCurve m_velocityCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] string m_animationID;
    [SerializeField] float m_animationTransitionTime = 0.1f;

    public float readyTime { get { return m_readyTime; } }
    public float moveTime { get { return m_moveTime; } }
    public AnimationCurve velocityCurve { get { return m_velocityCurve; } }
    public string animationStateID { get { return m_animationID; } }

    public float animationTransitionTime { get { return m_animationTransitionTime; } }
}
