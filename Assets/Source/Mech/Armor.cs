using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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