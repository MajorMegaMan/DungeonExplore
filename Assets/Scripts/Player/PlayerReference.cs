using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    [SerializeField] PlayerController m_controller;
    [SerializeField] PlayerAnimate m_animate;
    [SerializeField] PlayerInputReceiver m_input;
    [SerializeField] PlayerCameraController m_cameraController;
    [SerializeField] CameraLockon m_lockOn;
    [SerializeField] PlayerAttackController m_attackController;

    public PlayerController controller { get { return m_controller; } }
    public PlayerAnimate animate { get { return m_animate; } }
    public PlayerInputReceiver input { get { return m_input; } }
    public PlayerCameraController cameraController { get { return m_cameraController; } }
    public CameraLockon lockOn { get { return m_lockOn; } }
    public PlayerAttackController attackController { get { return m_attackController; } }


    private void Awake()
    {
        FindComponent(ref m_controller);
        FindComponent(ref m_animate);
        FindComponent(ref m_input);
        FindComponent(ref m_cameraController);
        FindComponent(ref m_lockOn);
        FindComponent(ref m_attackController);
    }

    void FindComponent<T>(ref T component) where T : MonoBehaviour
    {
        if(component == null)
        {
            component = gameObject.GetComponentInChildren<T>();
        }
        if (component == null)
        {
            Debug.LogError(typeof(T).ToString() + " was not found in heirarchy. Ensure that all Fields are allocated or located within the children of this GameObject::" + name);
        }
    }
}
