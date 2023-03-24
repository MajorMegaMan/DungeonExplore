using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemySettings : ScriptableObject
{
    [Header("Entity")]
    [SerializeField] float m_entityRadius = 1.0f;
    [SerializeField] EntityStats m_stats;

    [Header("Movement")]
    [SerializeField] float m_retargetDistance = 0.2f;

    [Header("AI States")]
    [SerializeField] float m_deadTime = 1.0f;

    public float entityRadius { get { return m_entityRadius; } }
    public EntityStats defaultStats { get { return m_stats; } }
    public float retargetDistance { get { return m_retargetDistance; } }
    public float deadTime { get { return m_deadTime; } }
}
