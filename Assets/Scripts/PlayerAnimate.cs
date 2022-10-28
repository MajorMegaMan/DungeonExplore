using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimate : MonoBehaviour
{
    Animator m_anim;
    [SerializeField] PlayerController m_playerController;
    [SerializeField] AnimationStateID m_movementParameter;

    [SerializeField] AnimationCurve m_speedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    Vector3 m_targetHeading;
    // Needs to be a minimum of 1.0f as 1.0 or lower will result in the character not turning towards the heading. A larger number will increase the turn rate.
    [SerializeField, Min(1.0f)] float m_headingSpeed = 1.0f;

    [SerializeField] Vector3 m_standEuler = Vector3.zero;
    [SerializeField] Vector3 m_walkEuler = Vector3.zero;
    [SerializeField] Vector3 m_runEuler = Vector3.zero;
    Quaternion m_standRot = Quaternion.identity;
    Quaternion m_walkRot = Quaternion.identity;
    Quaternion m_runRot = Quaternion.identity;

    float m_smoothSpeed = 0.0f;
    float m_smoothSpeedVel = 0.0f;
    float m_smoothSpeedTime = 0.02f;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
        m_movementParameter.Initialise();

        m_targetHeading = transform.forward;

        m_standRot = Quaternion.Euler(m_standEuler);
        m_walkRot = Quaternion.Euler(m_walkEuler);
        m_runRot = Quaternion.Euler(m_runEuler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        float normalisedSpeed = m_speedCurve.Evaluate(m_playerController.currentSpeed / m_playerController.speed);
        m_smoothSpeed = Mathf.SmoothDamp(m_smoothSpeed, normalisedSpeed, ref m_smoothSpeedVel, m_smoothSpeedTime);
        m_anim.SetFloat(m_movementParameter.GetID(), m_smoothSpeed);
        SmoothHeading(m_smoothSpeed);
    }

    // Smoothing based on a directional vector rather than linear interpolation.
    void SmoothHeading(float normalisedSpeed)
    {
        float tValue = 1 - Mathf.Pow(m_headingSpeed * m_headingSpeed, Time.deltaTime * Application.targetFrameRate);

        Quaternion additionalRot = Quaternion.Slerp(m_standRot, m_runRot, normalisedSpeed);

        if (m_playerController.heading.sqrMagnitude > 0.0001f)
        {
            m_targetHeading = Vector3.Slerp(m_targetHeading, m_playerController.heading, tValue);
        }
        transform.forward = additionalRot * m_targetHeading;
    }

    private void OnValidate()
    {
        m_standRot = Quaternion.Euler(m_standEuler);
        m_walkRot = Quaternion.Euler(m_walkEuler);
        m_runRot = Quaternion.Euler(m_runEuler);
    }
}
