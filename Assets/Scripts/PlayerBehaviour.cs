using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Tooltip("If player Reference is not assigned. This GameObject will attempt to Find one in it's parent heirarchy.")]
    [SerializeField] PlayerReference m_playerRef;

    public PlayerReference playerRef { get { return m_playerRef; } }

    // If awake is overriden, Ensure that m_playerRef is Assigned.
    protected virtual void Awake()
    {
        FindPlayerReference();
    }

    protected void FindPlayerReference()
    {
        if (m_playerRef == null)
        {
            m_playerRef = gameObject.GetComponentInParent<PlayerReference>();
            if (m_playerRef == null)
            {
                Debug.LogError("PlayerReference was not found in Parent heirarchy.");
            }
        }
    }
}
