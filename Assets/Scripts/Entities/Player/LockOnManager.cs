using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnManager : BBB.VariableMonoSingletonBase<LockOnManager>
{
    List<IEntity> m_lockOnTargets = null;
    List<IEntity> m_visibleLockOnTargets = null;

    // This is an additional variable to keep track of if Unity has destroyed the instance.
    // If the instance is destroyed there is no longer a need to reference into it anymore.
    static bool _instanceIsAwake = false;

    ILockOnTargeter m_ownerLockOnTargeter;

    void Awake()
    {
        if(instance != this)
        {
            Destroy(this);
            return;
        }
        SetInstance(this);
        OnCreateInstance();
    }

    protected override void OnCreateInstance()
    {
        if(m_lockOnTargets == null)
        {
            _instanceIsAwake = true;
            m_lockOnTargets = new List<IEntity>();
            m_visibleLockOnTargets = new List<IEntity>();
        }
    }

    public static void SetOwner(ILockOnTargeter lockOnTargeter)
    {
        if (_instanceIsAwake)
        {
            instance.m_ownerLockOnTargeter = lockOnTargeter;
        }
    }

    public static void RegisterLockOnTarget(IEntity lockOnTarget)
    {
        instance.m_lockOnTargets.Add(lockOnTarget);
    }

    void MemberDeregisterLockOnTarget(IEntity lockOnTarget)
    {
        m_lockOnTargets.Remove(lockOnTarget);
        if (m_ownerLockOnTargeter != null && m_ownerLockOnTargeter.lockOnTarget == lockOnTarget)
        {
            m_ownerLockOnTargeter.SetLockOnTarget(null);
        }
    }

    public static void DeregisterLockOnTarget(IEntity lockOnTarget)
    {
        if(_instanceIsAwake)
        {
            instance.MemberDeregisterLockOnTarget(lockOnTarget);
        }
    }

    public static IEntity FindLockOnTarget(Camera camera)
    {
        return FindLockOnTarget(camera.transform.position, camera.transform.forward, GeometryUtility.CalculateFrustumPlanes(camera));
    }

    public static IEntity FindLockOnTarget(Vector3 camPosition, Vector3 camForward, Plane[] camFrustumPlanes)
    {
        return instance.InstanceFindLockOnTarget(camPosition, camForward, camFrustumPlanes);
    }

    IEntity InstanceFindLockOnTarget(Vector3 camPosition, Vector3 camForward, Plane[] camFrustumPlanes)
    {
        m_visibleLockOnTargets.Clear();
        foreach (IEntity lockOnTarget in m_lockOnTargets)
        {
            if (GeometryUtility.TestPlanesAABB(camFrustumPlanes, lockOnTarget.GetAABB()))
            {
                // can see this target
                m_visibleLockOnTargets.Add(lockOnTarget);
            }
        }

        if (m_visibleLockOnTargets.Count == 0)
        {
            return null;
        }

        IEntity focusedTarget = m_visibleLockOnTargets[0];
        Vector3 camToTarget = (focusedTarget.position - camPosition).normalized;
        float focusedDot = Vector3.Dot(camForward, camToTarget);

        for (int i = 1; i < m_visibleLockOnTargets.Count; i++)
        {
            // Simple Distance check for now
            IEntity target = m_visibleLockOnTargets[i];
            camToTarget = (target.position - camPosition).normalized;
            float dot = Vector3.Dot(camForward, camToTarget);

            if (dot > focusedDot)
            {
                focusedTarget = target;
                focusedDot = dot;
            }
        }

        return focusedTarget;
    }

    protected override void OnDestroy()
    {
        if (instance == this)
        {
            _instanceIsAwake = false;
        }
        base.OnDestroy();
    }
}
