using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Explode : MonoBehaviour
{
    public GameObject m_Prefab;

    public bool m_ExplodeOnCollision = false;

    public void OnCollisionEnter(Collision collision)
    {
        if (!m_ExplodeOnCollision)
        {
            return;
        }
    }
}

