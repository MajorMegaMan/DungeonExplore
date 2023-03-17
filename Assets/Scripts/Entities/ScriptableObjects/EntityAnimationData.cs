using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EntityAnimationData : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] AnimationStateID m_movementParameter;
    [SerializeField] AnimationStateID m_horizontalMovementParameter;

    [SerializeField] AnimationCurve m_speedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

    // Needs to be a minimum of 1.0f as 1.0 or lower will result in the character not turning towards the heading. A larger number will increase the turn rate.
    [SerializeField, Min(1.0f)] float m_headingSpeed = 1.0f;

    [SerializeField] Vector3 m_standEuler = Vector3.zero;
    [SerializeField] Vector3 m_walkEuler = Vector3.zero;
    [SerializeField] Vector3 m_runEuler = Vector3.zero;
    Quaternion m_standRot = Quaternion.identity;
    Quaternion m_walkRot = Quaternion.identity;
    Quaternion m_runRot = Quaternion.identity;

    [SerializeField] float m_smoothSpeedTime = 0.2f;

    [Header("Common States")]
    [SerializeField] AnimationStateID m_movementState;
    [SerializeField] AnimationStateID m_deathState;

    [SerializeField] float m_commonStateTransitionTime = 0.1f;

    public AnimationStateID movementParameter { get { return m_movementParameter; } }
    public AnimationStateID horizontalMovementParameter { get { return m_horizontalMovementParameter; } }

    public AnimationCurve speedCurve { get { return m_speedCurve; } }
    public float headingSpeed { get { return m_headingSpeed; } }

    public Quaternion standRot { get { return m_standRot; } }
    public Quaternion walkRot { get { return m_walkRot; } }
    public Quaternion runRot { get { return m_runRot; } }

    public float smoothSpeedTime { get { return m_smoothSpeedTime; } }

    public AnimationStateID movementState { get { return m_movementState; } }
    public AnimationStateID deathState { get { return m_deathState; } }
    public float commonStateTransitionTime { get { return m_commonStateTransitionTime; } }

    private void OnValidate()
    {
        m_standRot = Quaternion.Euler(m_standEuler);
        m_walkRot = Quaternion.Euler(m_walkEuler);
        m_runRot = Quaternion.Euler(m_runEuler);
    }
}
