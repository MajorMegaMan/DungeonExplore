using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnManager : BBB.VariableMonoSingletonBase<LockOnManager>
{
    List<ILockOnTarget> m_lockOnTargets = null;
    List<ILockOnTarget> m_visibleLockOnTargets = null;

    // This is an additional variable to keep track of if Unity has destroyed the instance.
    // If the instance is destroyed there is no longer a need to reference into it anymore.
    static bool _instanceIsAwake = false;

    void Awake()
    {
        Debug.Log("Awaked");
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
            m_lockOnTargets = new List<ILockOnTarget>();
            m_visibleLockOnTargets = new List<ILockOnTarget>();
        }
    }

    public static void RegisterLockOnTarget(ILockOnTarget lockOnTarget)
    {
        instance.m_lockOnTargets.Add(lockOnTarget);
    }

    public static void DeregisterLockOnTarget(ILockOnTarget lockOnTarget)
    {
        if(_instanceIsAwake)
            instance.m_lockOnTargets.Remove(lockOnTarget);
    }

    public static ILockOnTarget FindLockOnTarget(Camera camera)
    {
        return FindLockOnTarget(camera.transform.position, camera.transform.forward, GeometryUtility.CalculateFrustumPlanes(camera));
    }

    public static ILockOnTarget FindLockOnTarget(Vector3 camPosition, Vector3 camForward, Plane[] camFrustumPlanes)
    {
        return instance.InstanceFindLockOnTarget(camPosition, camForward, camFrustumPlanes);
    }

    ILockOnTarget InstanceFindLockOnTarget(Vector3 camPosition, Vector3 camForward, Plane[] camFrustumPlanes)
    {
        m_visibleLockOnTargets.Clear();
        foreach (ILockOnTarget lockOnTarget in m_lockOnTargets)
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

        ILockOnTarget focusedTarget = m_visibleLockOnTargets[0];
        Vector3 camToTarget = (focusedTarget.GetTargetPosition() - camPosition).normalized;
        float focusedDot = Vector3.Dot(camForward, camToTarget);

        for (int i = 1; i < m_visibleLockOnTargets.Count; i++)
        {
            // Simple Distance check for now
            ILockOnTarget target = m_visibleLockOnTargets[i];
            camToTarget = (target.GetTargetPosition() - camPosition).normalized;
            float dot = Vector3.Dot(camForward, camToTarget);

            if (dot > focusedDot)
            {
                focusedTarget = target;
                focusedDot = dot;

                Debug.Log(focusedTarget.GetCameraLookTransform().parent.name);
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
