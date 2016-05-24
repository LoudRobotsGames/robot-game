using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [SerializeField]
    private float m_ThinkInterval = 0.1f;
    private float m_Timer = 0f;

    protected AIState m_CurrentState = null;
    protected AIState m_NextState = IdleState.StaticState;
    
    private AIData m_AIData = new AIData();
    public AIData AIData { get { return m_AIData; } }

    public void OnEnable()
    {
        m_AIData.AICharacter = GetComponent<MechAIControl>();
        m_AIData.Sensor = GetComponent<ISensor>();
    }

    public void Update()
    {
        m_Timer += Time.deltaTime;

        while (m_Timer > m_ThinkInterval)
        {
            m_Timer -= m_ThinkInterval;

            Think();
        }

        CheckStateAbortFlag();

        if (m_CurrentState != m_NextState)
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.Exit(m_AIData);
            }
            if (m_NextState != null)
            {
                m_NextState.Enter(m_AIData);
            }
            m_CurrentState = m_NextState;
        }

        // More frequent update method for states that need it
        if (m_CurrentState != null)
        {
            m_CurrentState.Update(m_AIData);
        }
    }

    private void CheckStateAbortFlag()
    {
        if (m_CurrentState != null && m_CurrentState.AbortFlag == true)
        {
            AIState suggestedNextState = m_CurrentState.ClearAbort();
            SetNextState(suggestedNextState);
        }
    }

    public virtual void Think()
    {
        // Think update
        if (m_CurrentState != null)
        {
            m_CurrentState.Think(m_AIData);
        }
    }

    public void SetNextState(AIState state)
    {
        m_NextState = state; 
    }
}

