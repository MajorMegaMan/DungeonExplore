using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityStats : ISerializationCallbackReceiver
{
    [SerializeField] float m_currentHealth = 100.0f;
    [SerializeField] float m_baseMaxHealth = 100.0f;

    [SerializeField] float m_baseStrength = 10.0f;
    [SerializeField] float m_baseDefense = 5.0f;

    public float currentHealth { get { return m_currentHealth; } }
    public float maxHealth { get { return m_baseMaxHealth; } }
    public float strength { get { return m_baseStrength; } }
    public float defense { get { return m_baseDefense; } }

    public void ReceiveDamage(float damage)
    {
        damage -= defense;
        m_currentHealth -= damage;
    }

    public void ReceiveHealing(float healing)
    {
        m_currentHealth += healing;
    }

    public void HealToFull()
    {
        m_currentHealth = maxHealth;
    }

    public float CalculateAttackStrength()
    {
        return strength;
    }

    public bool IsDead()
    {
        return m_currentHealth <= 0;
    }

    public void CopyStats(EntityStats target)
    {
        m_currentHealth = target.m_currentHealth;
        m_baseMaxHealth = target.m_baseMaxHealth;
        m_baseStrength  = target.m_baseStrength;
        m_baseDefense   = target.m_baseDefense;
    }

    public void Die()
    {
        m_currentHealth = 0.0f;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {

    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {

    }
}
