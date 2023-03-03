using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableMoveAction : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] float m_readyTime = 1.0f;
    [SerializeField] float m_moveTime = 1.0f;
    [SerializeField] AnimationCurve m_velocityCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

    [Header("Animation")]
    [SerializeField] string m_animationID;
    [SerializeField] float m_animationTransitionTime = 0.1f;
    [SerializeField] string m_animationDriveParameter;
    [SerializeField] float m_animationDriveValue = 1.0f;

    public float readyTime { get { return m_readyTime; } }
    public float moveTime { get { return m_moveTime; } }
    public AnimationCurve velocityCurve { get { return m_velocityCurve; } }

    public string animationStateID { get { return m_animationID; } }
    public float animationTransitionTime { get { return m_animationTransitionTime; } }
    public string animationDriveParameter { get { return m_animationDriveParameter; } }
    public float animationDriveValue { get { return m_animationDriveValue; } }
}
