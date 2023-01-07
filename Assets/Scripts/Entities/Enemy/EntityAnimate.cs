using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EntityAnimate : MonoBehaviour
{
    Animator m_anim;
    [SerializeField] EntityAnimationData m_animationData;

    AnimationStateID m_movementParameter;
    AnimationStateID m_horizontalMovementParameter;

    Vector3 m_targetHeading;

    float m_smoothSpeed = 0.0f;
    float m_smoothHoriSpeed = 0.0f;
    float m_smoothSpeedVel = 0.0f;
    float m_smoothHoriSpeedVel = 0.0f;

    IEntity m_entity = null;
    ILockOnTargeter m_lockOnTargeter = BBB.LockOn.Internal.EmptyLockOnTargeter.instance;

    public Animator anim { get { return m_anim; } }

    protected void Awake()
    {
        m_anim = GetComponent<Animator>();

        InitialiseAnimationIDs();

        m_targetHeading = transform.forward;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(m_entity == null)
        {
            var tryEntity = GetComponent<IEntity>();
            if(tryEntity == null)
            {
                Debug.LogError("Entity was not found. Ensure an entity is assigned to this component.");
            }
            var tryTargeter = GetComponent<ILockOnTargeter>();
            SetEntity(tryEntity, tryTargeter);
        }
    }

    private void LateUpdate()
    {
        float normalisedSpeed = m_animationData.speedCurve.Evaluate(m_entity.currentSpeed / m_entity.speed);

        if (m_lockOnTargeter.IsLockedOn())
        {
            Vector3 toLockOn = m_lockOnTargeter.lockOnTarget.GetTargetPosition() - transform.position;
            toLockOn.y = 0;
            toLockOn = toLockOn.normalized;
            float headingDot = Vector3.Dot(toLockOn, m_entity.heading);
            float forwardSpeed = normalisedSpeed * headingDot;
            Quaternion additionalRot = Quaternion.Slerp(m_animationData.standRot, m_animationData.runRot, forwardSpeed);
            //toLockOn = additionalRot * toLockOn;

            Vector3 playerRight = -Vector3.Cross(Vector3.up, m_entity.heading);
            float rightDot = Vector3.Dot(toLockOn, playerRight);

            m_smoothSpeed = Mathf.SmoothDamp(m_smoothSpeed, forwardSpeed, ref m_smoothSpeedVel, m_animationData.smoothSpeedTime);
            m_anim.SetFloat(m_movementParameter.GetID(), m_smoothSpeed);
            m_smoothHoriSpeed = Mathf.SmoothDamp(m_smoothHoriSpeed, normalisedSpeed * rightDot, ref m_smoothHoriSpeedVel, m_animationData.smoothSpeedTime);
            m_anim.SetFloat(m_horizontalMovementParameter.GetID(), m_smoothHoriSpeed);

            SmoothHeading(toLockOn, additionalRot);
        }
        else
        {
            m_smoothSpeed = Mathf.SmoothDamp(m_smoothSpeed, normalisedSpeed, ref m_smoothSpeedVel, m_animationData.smoothSpeedTime);

            m_anim.SetFloat(m_movementParameter.GetID(), m_smoothSpeed);
            if (Mathf.Abs(m_smoothHoriSpeed) > 0.0001f)
            {
                m_smoothHoriSpeed = Mathf.SmoothDamp(m_smoothHoriSpeed, 0.0f, ref m_smoothHoriSpeedVel, m_animationData.smoothSpeedTime);
            }
            else
            {
                m_smoothHoriSpeed = 0.0f;
            }
            m_anim.SetFloat(m_horizontalMovementParameter.GetID(), m_smoothHoriSpeed);
            SmoothHeading(m_smoothSpeed);
        }
    }

    void InitialiseAnimationIDs()
    {
        m_movementParameter = new AnimationStateID(m_animationData.movementParameter);
        m_horizontalMovementParameter = new AnimationStateID(m_animationData.horizontalMovementParameter);
        m_movementParameter.Initialise();
        m_horizontalMovementParameter.Initialise();
    }

    public void SetEntity(IEntity entity)
    {
        m_entity = entity;
    }

    public void SetEntity(IEntity entity, ILockOnTargeter lockOnTargeter)
    {
        m_entity = entity;
        SetLockOnTargeter(lockOnTargeter);
    }

    public void SetLockOnTargeter(ILockOnTargeter lockOnTargeter)
    {
        if (lockOnTargeter != null)
        {
            m_lockOnTargeter = lockOnTargeter;
        }
        else
        {
            m_lockOnTargeter = BBB.LockOn.Internal.EmptyLockOnTargeter.instance;
        }
    }

    // Smoothing based on a directional vector rather than linear interpolation.
    void SmoothHeading(float normalisedSpeed, Vector3 destinaionDirection)
    {
        Quaternion additionalRot = Quaternion.Slerp(m_animationData.standRot, m_animationData.runRot, normalisedSpeed);
        SmoothHeading(destinaionDirection, additionalRot);
    }

    void SmoothHeading(float normalisedSpeed)
    {
        SmoothHeading(normalisedSpeed, m_entity.heading);
    }

    void SmoothHeading(Vector3 destinationDirection, Quaternion additionalRot)
    {
        float tValue = 1 - Mathf.Pow(m_animationData.headingSpeed * m_animationData.headingSpeed, Time.deltaTime * Application.targetFrameRate);

        if (destinationDirection.sqrMagnitude > 0.0001f)
        {
            m_targetHeading = Vector3.Slerp(m_targetHeading, destinationDirection, tValue);
        }
        transform.forward = additionalRot * m_targetHeading;
    }
}
