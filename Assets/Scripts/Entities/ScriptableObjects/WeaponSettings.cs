using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponSettings : ScriptableObject
{
    [SerializeField] Mesh m_mesh;

    [Header("Actions")]
    [SerializeField] ScriptableAttackAction[] m_actions;

    [Header("Collider")]
    [SerializeField] Vector3 m_offset = Vector3.zero;
    [SerializeField] Vector3 m_size = Vector3.one;
    [SerializeField] Vector3 m_eulerRotation = Vector3.zero;


    public Mesh mesh { get { return m_mesh; } }
    public ScriptableAttackAction[] actions { get { return m_actions; } }
    public Vector3 offset { get { return m_offset; } }
    public Vector3 size { get { return m_size; } }
    public Vector3 eulerRotation { get { return m_eulerRotation; } }
}
