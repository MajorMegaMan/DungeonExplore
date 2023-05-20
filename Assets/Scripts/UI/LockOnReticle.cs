using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnReticle : MonoBehaviour
{
    [SerializeField] Camera m_camera;
    [SerializeField] Transform m_targetFollow;

    [Header("Colour Change")]
    [SerializeField] UnityEngine.UI.RawImage m_reticleImage;
    [SerializeField] string m_colourPropertyName = "_BaseColor";
    Material m_imageMaterial;
    int m_colourHashID = 0;

    private void Awake()
    {
        m_imageMaterial = m_reticleImage.material;
        m_colourHashID = Shader.PropertyToID(m_colourPropertyName);
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

    public void SetCamera(Camera camera)
    {
        m_camera = camera;
    }

    public void SetColour(Color colour)
    {
        m_imageMaterial.SetColor(m_colourHashID, colour);
    }
}
