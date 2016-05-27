using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    public EventHandler OnCollision;

    private Collision m_ActiveCollision;
    public Collision ActiveCollision { get { return m_ActiveCollision; } }


    private EffectSettings m_EffectSettings;

    public void Start()
    {
        GetEffectSettingsComponent(this.transform);
        if (m_EffectSettings != null) m_EffectSettings.CollisionEnter += effectSettings_CollisionEnter;
    }

    void effectSettings_CollisionEnter(object sender, CollisionInfo e)
    {
        if (OnCollision != null)
        {
            OnCollision(this, null);
        }

        if (e.Hit.collider == null)
            return;

        IDamageable damageable = e.Hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(5, e.Hit.point, e.Hit.normal);
        }
    }

    private void GetEffectSettingsComponent(Transform tr)
    {
        var parent = tr.parent;
        if (parent != null)
        {
            m_EffectSettings = parent.GetComponentInChildren<EffectSettings>();
            if (m_EffectSettings == null)
                GetEffectSettingsComponent(parent.transform);
        }
    }
}

