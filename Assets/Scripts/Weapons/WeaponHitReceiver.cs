using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitReceiver : MonoBehaviour
{
    [SerializeField] GameObject m_owner = null;
    IEntity m_entityOwner = null;

    public IEntity owner { get { return m_entityOwner; } }

    private void Awake()
    {
        if(m_owner != null)
        {
            SetOwner(m_owner);
        }
    }

    public void SetOwner(GameObject owner)
    {
        m_entityOwner = IEntity.ValidateGameObject(owner);
        if (m_entityOwner != null)
        {
            m_owner = owner;
        }
        else
        {
            m_owner = null;
        }
    }

    public void SetOwner(IEntity owner)
    {
        var component = IEntity.ValidateEntityAsMonobehaviour(owner);
        if (component == null)
        {
            m_owner = null;
            m_entityOwner = null;
            return;
        }
        else
        {
            var ownerGameObject = component.gameObject;
            if (ownerGameObject != null)
            {
                m_owner = ownerGameObject;
                m_entityOwner = owner;
            }
            else
            {
                Debug.LogError("GameObject does not exist in component somehow, this probvably means this method doesn't work properly. " + component.name);
                m_owner = null;
                m_entityOwner = null;
            }
        }
    }

    private void OnValidate()
    {
        SetOwner(m_owner);
    }
}
