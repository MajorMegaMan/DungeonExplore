using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : PlayerBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] float m_mouseSensitivity = 1.0f;
    [SerializeField, Range(-1, 1)] int m_mouseInvertX = 1;
    [SerializeField, Range(-1, 1)] int m_mouseInvertY = 1;

    [SerializeField] float m_gamepadSensitivity = 1.0f;
    [SerializeField, Range(-1, 1)] int m_gamepadInvertX = 1;
    [SerializeField, Range(-1, 1)] int m_gamepadInvertY = 1;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] GameObject m_cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] float m_topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] float m_bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] float m_cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] bool m_lockCameraPosition = false;

    // cinemachine
    private float m_cinemachineTargetYaw;
    private float m_cinemachineTargetPitch;

    private const float _threshold = 0.01f;

    [Header("Smoothing")]
    [SerializeField] float m_smoothTime = 0.1f;
    Vector2 m_smoothVelocity = Vector2.zero;


    public PlayerInputReceiver inputReceiver { get { return playerRef.input; } }

    public GameObject cinemachineCameraTarget { get { return m_cinemachineCameraTarget; } set { m_cinemachineCameraTarget = value; } }
    public float topClamp { get { return m_topClamp; } set { m_topClamp = value; } }
    public float bottomClamp { get { return m_bottomClamp; } set { m_bottomClamp = value; } }
    public float cameraAngleOverride { get { return m_cameraAngleOverride; } set { m_cameraAngleOverride = value; } }
    public bool lockCameraPosition { get { return m_lockCameraPosition; } set { m_lockCameraPosition = value; } }

    public float targetYaw { get { return m_cinemachineTargetYaw; } }
    public float targetPitch { get { return m_cinemachineTargetPitch; } }

    private void Start()
    {
        m_cinemachineTargetYaw = ClampAngle(m_cinemachineCameraTarget.transform.rotation.eulerAngles.y);
        m_cinemachineTargetPitch = ClampAngle(m_cinemachineCameraTarget.transform.rotation.eulerAngles.x, m_bottomClamp, m_topClamp);
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if(!m_lockCameraPosition)
        {
            ApplyLookSensitivity(inputReceiver.GetMouseLook(), m_mouseSensitivity, m_mouseInvertX, m_mouseInvertY);
            ApplyLookSensitivity(inputReceiver.GetGamepadLook(), m_gamepadSensitivity, m_gamepadInvertX, m_gamepadInvertY);
        }

        // clamp our rotations so our values are limited 360 degrees
        m_cinemachineTargetYaw = ClampAngle(m_cinemachineTargetYaw);
        m_cinemachineTargetPitch = ClampAngle(m_cinemachineTargetPitch, m_bottomClamp, m_topClamp);

        // Cinemachine will follow this target
        Vector3 currentEuler = m_cinemachineCameraTarget.transform.eulerAngles;
        Vector3 targetEuler = Vector3.zero;

        targetEuler.x = m_cinemachineTargetPitch + m_cameraAngleOverride;
        targetEuler.y = m_cinemachineTargetYaw;

        currentEuler.x = Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref m_smoothVelocity.x, m_smoothTime);
        currentEuler.y = Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref m_smoothVelocity.y, m_smoothTime);
        m_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(currentEuler);
    }

    void ApplyLookSensitivity(Vector2 look, float sens, int invertX, int invertY)
    {
        if (look.sqrMagnitude >= _threshold)
        {
            m_cinemachineTargetYaw += look.x * sens * invertX;
            m_cinemachineTargetPitch -= look.y * sens * invertY;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin = float.MinValue, float lfMax = float.MaxValue)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void SetTargetEuler(float pitch, float yaw)
    {
        m_cinemachineTargetYaw = yaw;
        m_cinemachineTargetPitch = pitch;
    }
}
