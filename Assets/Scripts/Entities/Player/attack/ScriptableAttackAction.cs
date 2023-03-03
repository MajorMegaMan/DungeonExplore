using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableAttackAction : ScriptableMoveAction
{
    [Header("Attack Values")]
    [SerializeField] float m_attackDistance = 1.0f;
    [SerializeField] float m_hitEnableTime = 0.2f;
    [SerializeField] float m_hitDisableTime = 0.8f;

    public float attackDistance { get { return m_attackDistance; } }
    public float hitEnableTime { get { return m_hitEnableTime; } }
    public float hitDisableTime { get { return m_hitDisableTime; } }
}
