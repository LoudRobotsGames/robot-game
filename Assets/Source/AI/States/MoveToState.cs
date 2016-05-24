using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MoveToState : AIState
{
    public static MoveToState StaticState = new MoveToState();
    
    public override void Enter(AIData data)
    {
        data.UpdateContactList();
        data.UpdateCurrentThreat();

        bool success = data.AICharacter.MoveToTarget(data.MoveTarget);
        if (!success)
        {
            Abort(IdleState.StaticState);
        }
    }
}
