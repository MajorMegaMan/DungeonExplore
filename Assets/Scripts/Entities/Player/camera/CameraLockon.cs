using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCameraController))]
public class CameraLockon : MonoBehaviour, ILockOnTargeter
{
    [SerializeField] LockOnReticle m_lockOnReticle;

    [SerializeField] Transform m_origin;
    [SerializeField] GameObject m_lockOnObject;
    IEntity m_lockOnTarget;

    [SerializeField] float m_lerpAmount = 0.5f;
    [SerializeField] float m_distanceDegradeRatio = 1.0f;
    [SerializeField] float debugCamDistance = 5.0f;

    [SerializeField] float m_angleFocus = 15.0f;

    PlayerCameraController m_camControl;

    [SerializeField] float m_smoothTime = 0.1f;
    Vector2 m_smoothVelocity = Vector2.zero;

    [SerializeField] Color m_lockOnColour = Color.red;
    [SerializeField] Color m_quickTargetColour = new Color(1.0f, 1.0f, 0.0f, 0.4f);

    // switches when the player presses the button.
    bool m_isLockedOnMode = false;

    float m_currentCamDistance = 0.0f;

    ILockOnTargeter m_selfTargeter;
    public IEntity lockOnTarget { get { return m_lockOnTarget; } set { SetLockOnTarget(value); } }
    public bool isLockedOn { get { return m_selfTargeter.IsLockedOn(); } }

    PlayerInputReceiver inputReceiver { get { return m_camControl.inputReceiver; } }

    private void Awake()
    {
        m_camControl = GetComponent<PlayerCameraController>();
        m_selfTargeter = this;

        LockOnManager.SetOwner(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_lockOnReticle.SetCamera(inputReceiver.playerViewCamera);

        // Set the reticle colour
        if (m_isLockedOnMode)
        {
            m_lockOnReticle.SetColour(m_lockOnColour);
        }
        else
        {
            m_lockOnReticle.SetColour(m_quickTargetColour);
        }
    }

    private void OnDestroy()
    {
        LockOnManager.SetOwner(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isLockedOnMode)
        {
            // Search for quick lock target.
            var lockOnTarget = LockOnManager.FindLockOnTarget(inputReceiver.playerViewCamera);
            SetLockOnTarget(lockOnTarget);
        }

        if (inputReceiver.GetLockOnDown())
        {
            // input from player. Set the lock mode to the current quick lock target.
            m_isLockedOnMode = !m_isLockedOnMode;
            if (m_isLockedOnMode)
            {
                m_lockOnReticle.SetColour(m_lockOnColour);
            }
            else
            {
                m_lockOnReticle.SetColour(m_quickTargetColour);
            }
            //if (!isLockedOn)
            //{
            //
            //    var lockOnTarget = LockOnManager.FindLockOnTarget(inputReceiver.playerViewCamera);
            //    SetLockOnTarget(lockOnTarget);
            //}
            //else
            //{
            //    SetLockOnTarget(null);
            //}
        }

        if (m_lockOnTarget != null)
        {
            if(m_lockOnTarget.entityStats.IsDead())
            {
                // if dead remove LockOn from current target
                if (m_isLockedOnMode)
                {
                    // Search for new lock on target if player is in lock on mode.
                    var lockOnTarget = LockOnManager.FindLockOnTarget(inputReceiver.playerViewCamera);
                    SetLockOnTarget(lockOnTarget);
                }
                else
                {
                    SetLockOnTarget(null);
                }

                return;
            }

            // if the player has locked on to the quick target, then update the camera to view the target.
            if(m_isLockedOnMode)
            {
                // Bill board the lock on image towards the camera.
                Vector3 pos = m_lockOnTarget.GetCameraLookTransform().position;
                m_currentCamDistance = (pos - m_origin.position).magnitude;
                SetLockOnPosition(pos);
                SetRotation(pos);
            }
            else
            {
                // This is un optimised but needs to currently be here.
                m_camControl.cinemachineCameraTarget.transform.position = m_origin.position;
            }
        }
        else
        {
            // This is un optimised but needs to currently be here.
            m_camControl.cinemachineCameraTarget.transform.position = m_origin.position;
        }
    }

    public void SetLockOnObject(GameObject lockOnObject)
    {
        m_lockOnObject = lockOnObject;
        if (m_lockOnObject == null)
        {
            m_lockOnTarget = null;
            return;
        }

        var lockOnTarget = m_lockOnObject.GetComponent<IEntity>();
        if (lockOnTarget != null)
        {
            m_lockOnTarget = lockOnTarget;
        }
        else
        {
            Debug.LogError("LockOnObject must have an ILockOnTarget Compatible Component.");
            m_lockOnObject = null;
        }
    }

    public void SetLockOnTarget(IEntity lockOnTarget)
    {
        m_lockOnTarget = lockOnTarget;
        if (m_lockOnTarget != null)
        {
            m_lockOnReticle.gameObject.SetActive(true);
            m_lockOnReticle.SetTargetFollow(lockOnTarget.GetCameraLookTransform());
        }
        else
        {
            m_camControl.cinemachineCameraTarget.transform.position = m_origin.position;
            m_lockOnReticle.gameObject.SetActive(false);
        }
    }

    void SetLockOnPosition(Vector3 targetPos)
    {
        float t = m_lerpAmount;
        if(m_currentCamDistance > debugCamDistance)
        {
            t = debugCamDistance / m_currentCamDistance;
            t -= 1.0f;
            t *= m_distanceDegradeRatio;
            t += m_lerpAmount;
        }
        m_camControl.cinemachineCameraTarget.transform.position = Vector3.Lerp(m_origin.position, targetPos, t);
    }

    void SetRotation(Vector3 targetPos)
    {
        Vector3 targetEuler = CalculateTargetEuler(targetPos);
        float smoothPitch = Mathf.SmoothDampAngle(m_camControl.targetPitch, targetEuler.x, ref m_smoothVelocity.x, m_smoothTime);
        float smoothYaw = Mathf.SmoothDampAngle(m_camControl.targetYaw, targetEuler.y, ref m_smoothVelocity.y, m_smoothTime);
        m_camControl.SetTargetEuler(smoothPitch, smoothYaw);
    }

    Quaternion CalcTargetLookRotation(Vector3 targetPos)
    {
        Vector3 toLockOn = Vector3.zero;
        if(m_currentCamDistance > 0.0001f)
        {
            toLockOn = (targetPos - m_origin.position) / m_currentCamDistance;
        }
        float viewDot = Vector3.Dot(toLockOn, inputReceiver.viewInputTransform.forward);

        if (viewDot > Mathf.Cos(m_angleFocus * Mathf.Deg2Rad))
        {
            // too much alignment
            Vector3 originToView = inputReceiver.viewInputTransform.position - m_origin.position;
            float viewSign = Mathf.Sign(Vector3.Dot(originToView, Vector3.Cross(Vector3.up, toLockOn)));

            toLockOn = Quaternion.AngleAxis(m_angleFocus * -viewSign, Vector3.up) * toLockOn;
        }

        Quaternion targetLookRot = Quaternion.FromToRotation(Vector3.forward, toLockOn);
        return targetLookRot;
    }

    Vector3 CalculateTargetEuler(Vector3 targetPos)
    {
        Quaternion targetLookRot = CalcTargetLookRotation(targetPos);
        return targetLookRot.eulerAngles;
    }

    private void OnValidate()
    {
        SetLockOnObject(m_lockOnObject);

        if (Application.isPlaying)
        {
            if (m_origin != null && m_lockOnTarget == null && m_camControl != null)
            {
                m_camControl.cinemachineCameraTarget.transform.position = m_origin.position;
            }
        }
    }

    private void OnDrawGizmos()
    {
        var self = this as ILockOnTargeter;
        if(self.IsLockedOn())
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = m_camControl.cinemachineCameraTarget.transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * m_currentCamDistance);

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawSphere(lockOnTarget.position, 0.2f);

            Gizmos.DrawSphere(m_camControl.cinemachineCameraTarget.transform.position, 0.2f);
        }
    }
}

//public interface ILockOnTarget
//{
//    Vector3 GetTargetPosition();
//
//    Bounds GetAABB();
//
//    Transform GetCameraLookTransform();
//
//    float GetTargetRadius();
//}