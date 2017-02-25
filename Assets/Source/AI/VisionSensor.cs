using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VisionSensor : MonoBehaviour, ISensor
{
    [SerializeField]
    private float m_VisionRange = 50f;
    [SerializeField]
    private string m_VisionTagFilter = "Player";

    private Transform m_Transform;
    private Collider[] m_Colliders = new Collider[64];

    public void Start()
    {
        m_Transform = transform;
    }

    public string VisionTagFiler {
        get { return m_VisionTagFilter; }
        set { m_VisionTagFilter = value; }
    }

    public float VisionRange {
        get { return m_VisionRange; }
        set { m_VisionRange = value; }
    }

    public void GetContacts(ref List<Transform> contacts)
    {
        if (contacts == null)
        {
            contacts = new List<Transform>(32);
        }
        contacts.Clear();

        int count = Physics.OverlapSphereNonAlloc(m_Transform.position, m_VisionRange, m_Colliders);
        for (int i = 0; i < count; ++i)
        {
            Collider collider = m_Colliders[i];
            if (collider.CompareTag(m_VisionTagFilter))
            {
                contacts.Add(collider.transform);
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, m_VisionRange);
    }
}
