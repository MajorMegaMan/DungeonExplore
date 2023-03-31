using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLockOnTarget : MonoBehaviour, IEntity
{
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    [SerializeField] float m_targetRadius = 1.0f;
    [SerializeField] int m_team = 0;
    [SerializeField] EntityStats m_stats;

    public string entityName { get { return "SimpleLockOnTarget, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return 0.0f; } }
    public Vector3 velocity { get { return Vector3.zero; } }
    public float currentSpeed { get { return 0.0f; } }
    public Vector3 heading { get { return Vector3.zero; } }
    public EntityStats entityStats { get { return m_stats; } }

    private void Start()
    {
        LockOnManager.RegisterLockOnTarget(this);
    }

    private void OnDestroy()
    {
        LockOnManager.DeregisterLockOnTarget(this);
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

    public Vector3 GetAttackTargetPostion(IEntity attacker)
    {
        return position;
    }

    public int GetTeam()
    {
        return m_team;
    }

    public void ReceiveHit(IEntity attacker)
    {
        Debug.Log(entityName + " was hit.");
        Debug.Log(entityName + "::Before::" + entityStats.currentHealth);
        entityStats.ReceiveDamage(attacker.entityStats.CalculateAttackStrength());
        Debug.Log(entityName + "::After::" + entityStats.currentHealth);
    }
}
