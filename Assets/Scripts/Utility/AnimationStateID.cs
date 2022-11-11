using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class AnimationStateID
{
    [SerializeField] string m_name;

    [System.NonSerialized] int m_id;

    public void Initialise()
    {
        m_id = Animator.StringToHash(m_name);
    }

    public int GetID()
    {
        return m_id;
    }
}
