using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public Transform m_Player;
    public MechAIControl m_EnemyAI;

    public void Update()
    {
        m_EnemyAI.SetTarget(m_Player);
    }
}
