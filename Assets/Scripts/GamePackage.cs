using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GamePackage : ScriptableObject
{
    [SerializeField] GameManager m_gameManager;
    [SerializeField] PlayerReference m_player;
    [SerializeField] Cinemachine.CinemachineBrain m_camera;
    [SerializeField] Cinemachine.CinemachineVirtualCamera m_virtualCamera;

    [SerializeField] PayloadController m_payload;
    [SerializeField] PayloadSpline m_payloadPath;

    [SerializeField] SpawnZone m_spawnZone;
    [SerializeField] EnemySpawnController m_spawner;

    [System.Flags]
    public enum CameraBrainVirtualSetup
    {
        UnDefined = 0,

        BrainValid = 1,
        VirtualValid = 2,
        SerialisedVirtualValid = 4,

        ChildVirtualValid = 8,

        BrainVirtualSameObject = 16,
    }

    public CameraBrainVirtualSetup GetCameraSetup()
    {
        CameraBrainVirtualSetup cameraBrainVirtualSetup = CameraBrainVirtualSetup.UnDefined;

        if (m_virtualCamera != null)
        {
            cameraBrainVirtualSetup |= CameraBrainVirtualSetup.SerialisedVirtualValid;
        }

        if (m_camera != null)
        {
            cameraBrainVirtualSetup |= CameraBrainVirtualSetup.BrainValid;

            var childVirtualCam = m_camera.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
            if (childVirtualCam != null)
            {
                cameraBrainVirtualSetup |= CameraBrainVirtualSetup.ChildVirtualValid;
            
                if (childVirtualCam.gameObject == m_camera.gameObject)
                {
                    cameraBrainVirtualSetup |= CameraBrainVirtualSetup.BrainVirtualSameObject;
                }
            }
            
            if (m_virtualCamera != null || childVirtualCam != null)
            {
                cameraBrainVirtualSetup |= CameraBrainVirtualSetup.VirtualValid;
            }
        }

        return cameraBrainVirtualSetup;
    }

    public bool ValidateCamera()
    {
        return GetCameraSetup().HasFlag(CameraBrainVirtualSetup.BrainValid | CameraBrainVirtualSetup.VirtualValid);
    }
}
