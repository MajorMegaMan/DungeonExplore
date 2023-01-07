using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAttackTarget : MonoBehaviour, IAttackTarget
{
    [SerializeField] int m_team = 0;
    [SerializeField] float m_hitRadius = 1.0f;

    public int GetTeam()
    {
        return m_team;
    }

    public Vector3 GetAttackTargetPosition()
    {
        return transform.position;
    }

    public float GetRadius()
    {
        return m_hitRadius;
    }    
}
