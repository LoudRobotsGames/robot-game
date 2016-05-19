using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SimpleBrain : MonoBehaviour
{
    public float m_ThinkInterval = 0.5f;

    private ISensor m_Sensor;
    private MechAIControl m_MechControl;

    private float m_Timer = 0f;
    private List<Transform> m_ContactList = new List<Transform>(32);

    public void OnEnable()
    {
        m_MechControl = GetComponent<MechAIControl>();
        m_Sensor = GetComponent<ISensor>();
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        while (m_Timer > m_ThinkInterval)
        {
            m_Timer -= m_ThinkInterval;

            Think();
        }
    }

    public void Think()
    {
        m_Sensor.GetContacts(ref m_ContactList);

        if (m_ContactList.Count > 0)
        {
            m_MechControl.SetTarget(m_ContactList[0]);
        }
    }
}
