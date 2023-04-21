using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressionTargeter : MonoBehaviour
{
    IEntity m_highestAggroTarget = null;
    float m_highestAggroValue = float.MinValue;
    Dictionary<IEntity, AggroValue> m_aggroTargets = new Dictionary<IEntity, AggroValue>();
    HashSet<IEntity> m_bufferTargets = new HashSet<IEntity>();

    [SerializeField] float m_aggroRange = 15.0f;
    [SerializeField] LayerMask m_attackLayers = ~0;
    [SerializeField] float m_passiveAggroRate = 5.0f;
    [SerializeField] float m_passiveAggroLoss = 5.0f;
    [SerializeField] float m_maxAggro = 100.0f;

    public IEntity target { get { return m_highestAggroTarget; } }

    struct AggroValue
    {
        public float value;
        public float min;

        static AggroValue _zero;
        public static AggroValue zero { get { return _zero; } }

        static AggroValue()
        {
            _zero = new AggroValue();
            _zero.value = 0.0f;
            _zero.min = 0.0f;
        }
    }

    private void Update()
    {
        FindAggroTargets();
    }

    public void RegisterEntity(IEntity entity, float aggro = 0.0f, float minAggro = 0.0f)
    {
        AggroValue aggroValue = AggroValue.zero;
        aggroValue.value = Mathf.Clamp(aggro, minAggro, m_maxAggro);
        aggroValue.min = minAggro;

        m_aggroTargets.Add(entity, aggroValue);
        if(aggro > m_highestAggroValue)
        {
            m_highestAggroTarget = entity;
            m_highestAggroValue = aggro;
        }
    }

    public void DeregisterEntity(IEntity entity)
    {
        m_aggroTargets.Remove(entity);
        if (entity == m_highestAggroTarget)
        {
            FindHighestAggro();
        }
    }

    public void AddAggro(IEntity entity, float aggro)
    {
        if(m_aggroTargets.TryGetValue(entity, out AggroValue targetValue))
        {
            targetValue.value = Mathf.Clamp(targetValue.value + aggro, targetValue.min, m_maxAggro);
        }
        else
        {
            // not present
            targetValue = AggroValue.zero;
            targetValue.value = aggro;
        }

        m_aggroTargets[entity] = targetValue;
        if (targetValue.value > m_highestAggroValue)
        {
            m_highestAggroTarget = entity;
            m_highestAggroValue = aggro;
        }
    }

    public void RemoveAggro(IEntity entity, float aggro)
    {
        if (m_aggroTargets.TryGetValue(entity, out AggroValue targetValue))
        {
            targetValue = m_aggroTargets[entity];
            targetValue.value = Mathf.Clamp(targetValue.value - aggro, targetValue.min, m_maxAggro);
            if (targetValue.value <= 0.0f)
            {
                DeregisterEntity(entity);
                return;
            }
            m_aggroTargets[entity] = targetValue;

            if (entity == m_highestAggroTarget)
            {
                FindHighestAggro();
            }
        }
    }

    void FindHighestAggro()
    {
        float highest = float.MinValue;
        IEntity target = null;
        foreach (var pair in m_aggroTargets)
        {
            if(pair.Value.value > highest)
            {
                highest = pair.Value.value;
                target = pair.Key;
            }
        }

        m_highestAggroValue = highest;
        m_highestAggroTarget = target;
    }

    public void ResetAggro()
    {
        var keys = m_aggroTargets.Keys;
        foreach(var key in keys)
        {
            AggroValue aggro = m_aggroTargets[key];
            aggro.value = 0.0f;
            m_aggroTargets[key] = aggro;
        }
        m_highestAggroValue = float.MinValue;
    }

    void FindAggroTargets()
    {
        var keys = m_aggroTargets.Keys;
        foreach (var key in keys)
        {
            m_bufferTargets.Add(key);
        }

        var colliders = Physics.OverlapSphere(transform.position, m_aggroRange, m_attackLayers);
        foreach(var collider in colliders)
        {
            // only attackable targets should be used.
            var hitReceiver = collider.GetComponent<WeaponHitReceiver>();
            if(hitReceiver != null)
            {
                var entity = hitReceiver.owner;
                AddAggro(entity, m_passiveAggroRate * Time.deltaTime);
                m_bufferTargets.Remove(entity);
            }
        }

        foreach (var entity in m_bufferTargets)
        {
            RemoveAggro(entity, m_passiveAggroLoss * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if(!gameObject.activeInHierarchy)
        {
            return;
        }

        Color colour = Color.cyan;

        colour.a *= 0.8f;
        Gizmos.color = colour;
        Gizmos.DrawWireSphere(transform.position, m_aggroRange);

        colour.a *= 0.4f;
        Gizmos.color = colour;
        Gizmos.DrawSphere(transform.position, m_aggroRange);
    }
}
