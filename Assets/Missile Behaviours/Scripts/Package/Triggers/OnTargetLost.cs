using UnityEngine;
using System;
using MissileBehaviours.Controller;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers if the target, specified in the attached missile controller, becomes null.
    /// </summary>
    [RequireComponent(typeof(MissileController))]
    public class OnTargetLost : TriggerBase
    {
        MissileController controller;

        void Awake()
        {
            controller = GetComponent<MissileController>();
        }

        void Update()
        {
            if (!controller.Target)
            {
                FireTrigger(this, EventArgs.Empty);
            }
        }
    }
}