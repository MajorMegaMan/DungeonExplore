using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnReticle : MonoBehaviour
{
    [SerializeField] Camera m_camera;
    [SerializeField] Transform m_targetFollow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(m_targetFollow != null)
        {
            transform.position = m_targetFollow.position;
            transform.LookAt(m_camera.transform);
        }
    }

    public void SetTargetFollow(Transform target)
    {
        m_targetFollow = target;
    }
}
