using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AttackAction : ScriptableObject
{
    [SerializeField] float m_time = 1.0f;

    public float time { get { return m_time; } }
}
