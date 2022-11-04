using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCameraController))]
public class CameraLockon : MonoBehaviour
{
    [SerializeField] Transform m_viewTransform;
    [SerializeField] Transform m_origin;
    [SerializeField] Transform m_lockOnTarget;

    [SerializeField] float m_lerpAmount = 0.5f;
    [SerializeField] float m_distanceDegradeRatio = 1.0f;
    [SerializeField] float debugCamDistance = 5.0f;

    [SerializeField] float m_angleFocus = 15.0f;

    PlayerCameraController m_camControl;

    [SerializeField] float m_smoothTime = 0.1f;
    Vector2 m_smoothVelocity = Vector2.zero;

    private void Awake()
    {
        m_camControl = GetComponent<PlayerCameraController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_lockOnTarget != null)
        {
            SetLockOnPosition();
            SetRotation();
        }
    }

    void SetLockOnPosition()
    {
        float t = m_lerpAmount;
        float targetDistance = (m_lockOnTarget.position - m_origin.position).magnitude;
        if(targetDistance > debugCamDistance)
        {
            t = debugCamDistance / targetDistance;
            t -= 1.0f;
            t *= m_distanceDegradeRatio;
            t += m_lerpAmount;
        }
        transform.position = Vector3.Lerp(m_origin.position, m_lockOnTarget.position, t);
    }

    void SetRotation()
    {
        Vector3 targetEuler = CalculateTargetEuler();
        float smoothPitch = Mathf.SmoothDampAngle(m_camControl.targetPitch, targetEuler.x, ref m_smoothVelocity.x, m_smoothTime);
        float smoothYaw = Mathf.SmoothDampAngle(m_camControl.targetYaw, targetEuler.y, ref m_smoothVelocity.y, m_smoothTime);
        m_camControl.SetTargetEuler(smoothPitch, smoothYaw);
    }

    Quaternion CalcTargetLookRotation()
    {
        Vector3 toLockOn = (m_lockOnTarget.position - m_origin.position).normalized;
        float viewDot = Vector3.Dot(toLockOn, m_viewTransform.forward);

        if (viewDot > Mathf.Cos(m_angleFocus * Mathf.Deg2Rad))
        {
            // too much alignment
            Vector3 originToView = m_viewTransform.position - m_origin.position;
            float viewSign = Mathf.Sign(Vector3.Dot(originToView, Vector3.Cross(Vector3.up, toLockOn)));

            toLockOn = Quaternion.AngleAxis(m_angleFocus * -viewSign, Vector3.up) * toLockOn;
        }

        Quaternion targetLookRot = Quaternion.FromToRotation(Vector3.forward, toLockOn);
        return targetLookRot;
    }

    Vector3 CalculateTargetEuler()
    {
        Quaternion targetLookRot = CalcTargetLookRotation();
        return targetLookRot.eulerAngles;
    }

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            if(m_origin != null && m_lockOnTarget == null)
            {
                transform.position = m_origin.position;
            }
        }
    }
}
