using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableAttackAction : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] float m_readyTime = 1.0f;
    [SerializeField] float m_moveTime = 1.0f;
    [SerializeField] float m_attackDistance = 1.0f;
    [SerializeField] AnimationCurve m_velocityCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

    [Header("Attack Values")]
    [SerializeField] float m_hitEnableTime = 0.2f;
    [SerializeField] float m_hitDisableTime = 0.8f;

    [Header("Animation")]
    [SerializeField] string m_animationID;
    [SerializeField] float m_animationTransitionTime = 0.1f;

    public float readyTime { get { return m_readyTime; } }
    public float moveTime { get { return m_moveTime; } }
    public float attackDistance { get { return m_attackDistance; } }
    public AnimationCurve velocityCurve { get { return m_velocityCurve; } }
    public float hitEnableTime { get { return m_hitEnableTime; } }
    public float hitDisableTime { get { return m_hitDisableTime; } }
    public string animationStateID { get { return m_animationID; } }
    public float animationTransitionTime { get { return m_animationTransitionTime; } }
}
