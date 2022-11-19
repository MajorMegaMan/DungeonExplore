using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BBB.SimpleMonoSingleton<GameManager>
{
    [SerializeField] PlayerController m_player;
    [SerializeField] PlayerInputReceiver m_playerInput;
    [SerializeField] Camera m_camera;
    [SerializeField] CameraLockon m_cameraLockOn;
    [SerializeField] LockOnReticle m_lockOnReticle;

    List<ILockOnTarget> m_lockOnTargets;
    List<ILockOnTarget> m_visibleLockOnTargets;

    protected override void Awake()
    {
        base.Awake();
        m_lockOnTargets = new List<ILockOnTarget>();
        m_visibleLockOnTargets = new List<ILockOnTarget>();
        m_lockOnReticle.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_playerInput.GetLockOnDown())
        {
            if(!m_cameraLockOn.isLockedOn)
            {
                var lockOnTarget = FindLockOnTarget(GeometryUtility.CalculateFrustumPlanes(m_camera));
                if(lockOnTarget != null)
                {
                    m_cameraLockOn.SetLockOnTarget(lockOnTarget);
                    m_lockOnReticle.gameObject.SetActive(true);
                    m_lockOnReticle.SetTargetFollow(lockOnTarget.GetTransform());
                }
            }
            else
            {
                m_cameraLockOn.SetLockOnTarget(null);
                m_lockOnReticle.gameObject.SetActive(false);
            }    
        }
    }

    public void RegisterLockOnTarget(ILockOnTarget lockOnTarget)
    {
        m_lockOnTargets.Add(lockOnTarget);
    }

    public void DeregisterLockOnTarget(ILockOnTarget lockOnTarget)
    {
        m_lockOnTargets.Remove(lockOnTarget);
    }

    public ILockOnTarget FindLockOnTarget(Plane[] camFrustumPlanes)
    {
        m_visibleLockOnTargets.Clear();
        foreach(ILockOnTarget lockOnTarget in m_lockOnTargets)
        {
            if(GeometryUtility.TestPlanesAABB(camFrustumPlanes, lockOnTarget.GetAABB()))
            {
                // can see this target
                m_visibleLockOnTargets.Add(lockOnTarget);
            }
        }

        if(m_visibleLockOnTargets.Count == 0)
        {
            return null;
        }

        ILockOnTarget focusedTarget = m_visibleLockOnTargets[0];
        Vector3 camToTarget = (focusedTarget.GetTargetPosition() - m_camera.transform.position).normalized;
        float focusedDot = Vector3.Dot(m_camera.transform.forward, camToTarget);

        Debug.Log(focusedTarget.GetTransform().parent.name);
        for (int i = 1; i < m_visibleLockOnTargets.Count; i++)
        {
            // Simple Distance check for now
            ILockOnTarget target = m_visibleLockOnTargets[i];
            camToTarget = (target.GetTargetPosition() - m_camera.transform.position).normalized;
            float dot = Vector3.Dot(m_camera.transform.forward, camToTarget);

            if(dot > focusedDot)
            {
                focusedTarget = target;
                focusedDot = dot;

                Debug.Log(focusedTarget.GetTransform().parent.name);
            }
        }

        return focusedTarget;
    }
}
