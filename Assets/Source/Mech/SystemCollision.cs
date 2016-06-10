using UnityEngine;
using System.Collections;
using System;

public class SystemCollision : MonoBehaviour, IDamageable
{
    [SerializeField]
    private MechSystem m_TargetSystem;

    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (m_TargetSystem != null)
        {
            m_TargetSystem.TakeDamage(damage, hitPoint, hitNormal);
        }
    }
}
