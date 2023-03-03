using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAttackTarget : MonoBehaviour, IEntity
{
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    [SerializeField] float m_targetRadius = 1.0f;
    [SerializeField] int m_team = 0;

    public string entityName { get { return "SimpleAttackTarget, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return 0.0f; } }
    public Vector3 velocity { get { return Vector3.zero; } }
    public float currentSpeed { get { return 0.0f; } }
    public Vector3 heading { get { return Vector3.zero; } }

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

    public int GetTeam()
    {
        return m_team;
    }
}
