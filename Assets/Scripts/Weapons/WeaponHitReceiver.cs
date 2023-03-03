using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitReceiver : MonoBehaviour
{
    [SerializeField] GameObject m_owner = null;
    IEntity m_entityOwner = null;

    public IEntity owner { get { return m_entityOwner; } }

    public void SetOwner(GameObject owner)
    {
        if(owner == null)
        {
            m_owner = null;
            m_entityOwner = null;
            return;
        }    

        var entity = owner.GetComponent<IEntity>();
        if(entity != null)
        {
            m_owner = owner;
            m_entityOwner = entity;
        }
        else
        {
            Debug.LogError("GameObject does not contain Component interface of IEntity. " + owner.name);
            m_owner = null;
            m_entityOwner = null;
        }
    }

    public void SetOwner(IEntity owner)
    {
        if (owner == null)
        {
            m_owner = null;
            m_entityOwner = null;
            return;
        }

        var component = (owner as MonoBehaviour);
        if(component == null)
        {
            Debug.LogError("IEntity does not inherit from Monobehaviour. " + owner.entityName);
            m_owner = null;
            m_entityOwner = null;
            return;
        }
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

    private void OnValidate()
    {
        SetOwner(m_owner);
    }
}
