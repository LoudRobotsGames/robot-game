using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MechSystem : MonoBehaviour, IDamageable
{
    public string m_Name;
    public Armor m_Armor;
    public DamageableStructure m_InternalStructure;
    public List<InternalSystem> m_InternalSystem;
    
    public void Start()
    {
    }

    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Armor ablates first and reduces damage
        int excessDamage = m_Armor.TakeDamage(damage);

        // Calculate internal damage. Armor might have failed or it might be too weak to stop all the damage
        int internalDamage = m_Armor.CalculatePiercing(excessDamage);
        
        // If any damage done to internals, it may damage an internal system
        if (internalDamage > 0)
        {

        }

        // If any damage remaining, deal it to the internal structure
        m_InternalStructure.TakeDamage(internalDamage);
    }

    #region local types
    [Serializable]
    public class DamageableStructure
    {
        public int m_MaxHealth;
        [HideInInspector]
        public int m_CurrentHealth;

        public DamageableStructure()
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

    [Serializable]
    public class Armor : DamageableStructure
    {
        public float AblativeStrength = 0.1f;

        public int CalculatePiercing(int damage)
        {
            int ablationAmount = Mathf.RoundToInt((float)m_CurrentHealth * AblativeStrength);
            return Mathf.Max(damage - ablationAmount, 0);
        }

    }
    #endregion
}
