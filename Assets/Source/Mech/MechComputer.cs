using UnityEngine;
using System.Collections;
using System;

public class MechComputer : MonoBehaviour
{
    private MechStartupState m_CurrentState = MechStartupState.Shutdown;
    private MechSystem[] m_MechSystems;

    public GameObject m_MechRoot;

    public void InitComputer()
    {
        m_CurrentState = MechStartupState.CollectSystems;
    }

    private void Update()
    {
        switch(m_CurrentState)
        {
            case MechStartupState.Shutdown:
                break;
            case MechStartupState.CollectSystems:
                CollectMechSystems();
                break;
            case MechStartupState.ConnectSystems:
                ConnectMechSystems();
                break;
            case MechStartupState.Running:
                break;
        }
    }

    private void CollectMechSystems()
    {
        m_MechSystems = GetComponentsInChildren<MechSystem>(true);
        m_CurrentState = MechStartupState.ConnectSystems;
    }

    private void ConnectMechSystems()
    {
        for (int i = 0; i < m_MechSystems.Length; ++i)
        {
            m_MechSystems[i].m_RootMechObject = m_MechRoot;
        }
        m_CurrentState = MechStartupState.Running;
    }
}
