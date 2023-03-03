using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    [SerializeField] GameObject m_owner = null;
    IEntity m_entityOwner = null;

    public IEntity owner { get { return m_entityOwner; } }

    [SerializeField] Vector3 m_offset = Vector3.zero;
    [SerializeField] Vector3 m_size = Vector3.one;
    [SerializeField] Quaternion m_rotation = Quaternion.identity;
    [SerializeField] LayerMask m_targetLayers = ~0;
    [SerializeField] bool m_active = false;

    public bool isActive { get { return m_active; } set { m_active = value; enabled = value; } }

    HashSet<WeaponHitReceiver> m_hitTargets;

    private void Awake()
    {
        m_hitTargets = new HashSet<WeaponHitReceiver>();
        if (!m_active)
        {
            enabled = m_active;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var colliders = CaptureBox();
        foreach(var hitColldier in colliders)
        {
            var hitTarget = hitColldier.GetComponent<WeaponHitReceiver>();
            if (hitTarget != null)
            {
                if(m_hitTargets.Add(hitTarget))
                {
                    // Entity was hit during this weapon pass.
                    hitTarget.owner.ReceiveHit(m_entityOwner);
                }
            }
        }
    }

    private void OnEnable()
    {
        m_active = true;
        m_hitTargets.Clear();
    }

    private void OnDisable()
    {
        m_active = false;
    }

    public void ApplyWeaponSettings(WeaponSettings weaponSettings)
    {
        m_offset = weaponSettings.offset;
        m_size = weaponSettings.size;
        m_rotation = Quaternion.Euler(weaponSettings.eulerRotation);
    }

    Collider[] CaptureBox()
    {
        var localOffset = transform.TransformDirection(m_offset);
        var localSize = transform.localScale;
        localSize.x *= m_size.x;
        localSize.y *= m_size.y;
        localSize.z *= m_size.z;
        localSize *= 0.5f;
        return Physics.OverlapBox(transform.position + localOffset, localSize, transform.rotation * m_rotation, m_targetLayers, QueryTriggerInteraction.Ignore);
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

        if (Application.isPlaying)
        {
            enabled = m_active;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Color colour = Color.red;
        if(m_active)
        {
            colour = Color.green;
        }

        Gizmos.color = colour;
        Gizmos.DrawWireCube(m_offset, m_size);

        if (Application.isPlaying)
        {
            if (m_hitTargets.Count > 0 && isActive)
            {
                colour = Color.blue;
            }
        }

        colour.a *= 0.4f;
        Gizmos.color = colour;
        Gizmos.DrawCube(m_offset, m_size);
    }
}
