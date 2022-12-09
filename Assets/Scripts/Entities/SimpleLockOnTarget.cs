using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLockOnTarget : MonoBehaviour, ILockOnTarget
{
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    [SerializeField] float m_targetRadius = 1.0f;

    private void Start()
    {
        GameManager.instance.RegisterLockOnTarget(this);
    }

    private void OnDestroy()
    {
        var gameManager = GameManager.instance;
        if (gameManager != null)
        {
            gameManager.DeregisterLockOnTarget(this);
        }
    }

    public Vector3 GetTargetPosition()
    {
        return transform.position;
    }

    public Bounds GetAABB()
    {
        return m_renderer.bounds;
    }

    public Transform GetCameraLookTransform()
    {
        return m_lockOnTransform;
    }

    public float GetTargetRadius()
    {
        return m_targetRadius;
    }
}
