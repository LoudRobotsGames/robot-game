using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    public EventHandler OnCollision;

    private Collision m_ActiveCollision;
    public Collision ActiveCollision { get { return m_ActiveCollision; } }

    public void OnCollisionEnter(Collision collision)
    {
        if (OnCollision != null)
        {
            OnCollision(this, null);
        }
    }
}

