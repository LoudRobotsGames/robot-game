using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class DamageableStructure
{
    public int m_MaxHealth;
    [HideInInspector]
    public int m_CurrentHealth;

    public DamageableStructure()
    {
        m_MaxHealth = 10;
        m_CurrentHealth = 10;
    }

    public DamageableStructure(int max)
    {
        m_MaxHealth = max;
        m_CurrentHealth = max;
    }

    public void Reset()
    {
        m_CurrentHealth = m_MaxHealth;
    }

    public int TakeDamage(int damage)
    {
        int damageDone = Mathf.Min(m_CurrentHealth, damage);
        m_CurrentHealth -= damageDone;

        int excessDamage = damage - damageDone;
        return excessDamage;
    }
}
