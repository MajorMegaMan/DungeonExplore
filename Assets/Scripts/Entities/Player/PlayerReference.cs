using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    [SerializeField] PlayerController m_controller;
    [SerializeField] EntityAnimate m_animate;
    [SerializeField] PlayerInputReceiver m_input;
    [SerializeField] PlayerCameraController m_cameraController;
    [SerializeField] CameraLockon m_lockOn;

    public PlayerController controller { get { return m_controller; } }
    public EntityAnimate animate { get { return m_animate; } }
    public PlayerInputReceiver input { get { return m_input; } }
    public PlayerCameraController cameraController { get { return m_cameraController; } }
    public CameraLockon lockOn { get { return m_lockOn; } }


    private void Awake()
    {
        FindComponent(ref m_controller);
        FindComponent(ref m_animate);
        FindComponent(ref m_input);
        FindComponent(ref m_cameraController);
        FindComponent(ref m_lockOn);

        if(m_animate != null)
        {
            m_animate.SetEntity(m_controller, m_lockOn);
        }
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
