using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationStateID
{
    [SerializeField] string m_name;

    [System.NonSerialized] int m_id;

    public AnimationStateID() { }

    public AnimationStateID(string name)
    {
        m_name = name;
        Initialise();
    }

    public AnimationStateID(AnimationStateID copy)
    {
        Copy(copy);
    }

    public void Initialise()
    {
        m_id = Animator.StringToHash(m_name);
    }

    public int GetID()
    {
        return m_id;
    }

    public void Copy(AnimationStateID other)
    {
        m_name = other.m_name;
        m_id = other.m_id;
    }
}
