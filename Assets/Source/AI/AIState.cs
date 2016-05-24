using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AIState
{
    public virtual void Enter(AIData data) { }
    public virtual void Think(AIData data) { }
    public virtual void Update(AIData data) { }
    public virtual void Exit(AIData data) { }

    public bool AbortFlag { get; private set; }
    private AIState SuggestedNextState;

    protected void Abort(AIState suggestion)
    {
        SuggestedNextState = suggestion;
        AbortFlag = true;
    }

    public AIState ClearAbort()
    {
        AbortFlag = false;
        AIState suggestion = SuggestedNextState;
        SuggestedNextState = null;
        return suggestion;
    }
}
