using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class IdleState : AIState
{
    public static IdleState StaticState = new IdleState();
    
    public void Enter(AIData data)
    {
        data.AICharacter.SetTarget(null);
        data.AICharacter.Stop();
    }
}

