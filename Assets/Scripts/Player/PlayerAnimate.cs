using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimate : PlayerBehaviour
{
    Animator m_anim;
    [SerializeField] AnimationStateID m_movementParameter;
    [SerializeField] AnimationStateID m_horizontalMovementParameter;

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
    float m_smoothHoriSpeed = 0.0f;
    float m_smoothSpeedVel = 0.0f;
    float m_smoothHoriSpeedVel = 0.0f;
    [SerializeField] float m_smoothSpeedTime = 0.02f;

    //[SerializeField] CameraLockon m_camLockOn;

    CameraLockon camLockOn { get { return playerRef.lockOn; } }
    PlayerController playerController { get { return playerRef.controller; } }

    protected override void Awake()
    {
        base.Awake();

        m_anim = GetComponent<Animator>();
        m_movementParameter.Initialise();
        m_horizontalMovementParameter.Initialise();

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
        float normalisedSpeed = m_speedCurve.Evaluate(playerController.currentSpeed / playerController.speed);

        if(camLockOn.isLockedOn)
        {
            Vector3 toLockOn = camLockOn.lockOnTarget.GetTargetPosition() - transform.position;
            toLockOn.y = 0;
            toLockOn = toLockOn.normalized;
            float headingDot = Vector3.Dot(toLockOn, playerController.heading);
            float forwardSpeed = normalisedSpeed * headingDot;
            Quaternion additionalRot = Quaternion.Slerp(m_standRot, m_runRot, forwardSpeed);
            //toLockOn = additionalRot * toLockOn;

            Vector3 playerRight = -Vector3.Cross(Vector3.up, playerController.heading);
            float rightDot = Vector3.Dot(toLockOn, playerRight);

            m_smoothSpeed = Mathf.SmoothDamp(m_smoothSpeed, forwardSpeed, ref m_smoothSpeedVel, m_smoothSpeedTime);
            m_anim.SetFloat(m_movementParameter.GetID(), m_smoothSpeed);
            m_smoothHoriSpeed = Mathf.SmoothDamp(m_smoothHoriSpeed, normalisedSpeed * rightDot, ref m_smoothHoriSpeedVel, m_smoothSpeedTime);
            m_anim.SetFloat(m_horizontalMovementParameter.GetID(), m_smoothHoriSpeed);

            SmoothHeading(toLockOn, additionalRot);
        }
        else
        {
            m_smoothSpeed = Mathf.SmoothDamp(m_smoothSpeed, normalisedSpeed, ref m_smoothSpeedVel, m_smoothSpeedTime);

            m_anim.SetFloat(m_movementParameter.GetID(), m_smoothSpeed);
            if(Mathf.Abs(m_smoothHoriSpeed) > 0.0001f)
            {
                m_smoothHoriSpeed = Mathf.SmoothDamp(m_smoothHoriSpeed, 0.0f, ref m_smoothHoriSpeedVel, m_smoothSpeedTime);
            }
            else
            {
                m_smoothHoriSpeed = 0.0f;
            }
            m_anim.SetFloat(m_horizontalMovementParameter.GetID(), m_smoothHoriSpeed);
            SmoothHeading(m_smoothSpeed);
        }
    }

    // Smoothing based on a directional vector rather than linear interpolation.
    void SmoothHeading(float normalisedSpeed, Vector3 destinaionDirection)
    {
        Quaternion additionalRot = Quaternion.Slerp(m_standRot, m_runRot, normalisedSpeed);
        SmoothHeading(destinaionDirection, additionalRot);
    }

    void SmoothHeading(float normalisedSpeed)
    {
        SmoothHeading(normalisedSpeed, playerController.heading);
    }

    void SmoothHeading(Vector3 destinationDirection, Quaternion additionalRot)
    {
        float tValue = 1 - Mathf.Pow(m_headingSpeed * m_headingSpeed, Time.deltaTime * Application.targetFrameRate);

        if (destinationDirection.sqrMagnitude > 0.0001f)
        {
            m_targetHeading = Vector3.Slerp(m_targetHeading, destinationDirection, tValue);
        }
        transform.forward = additionalRot * m_targetHeading;
    }

    void SmoothHeading(Vector3 destinationDirection)
    {
        float tValue = 1 - Mathf.Pow(m_headingSpeed * m_headingSpeed, Time.deltaTime * Application.targetFrameRate);

        if (destinationDirection.sqrMagnitude > 0.0001f)
        {
            m_targetHeading = Vector3.Slerp(m_targetHeading, destinationDirection, tValue);
        }
        transform.forward = m_targetHeading;
    }

    private void OnValidate()
    {
        m_standRot = Quaternion.Euler(m_standEuler);
        m_walkRot = Quaternion.Euler(m_walkEuler);
        m_runRot = Quaternion.Euler(m_runEuler);
    }
}
