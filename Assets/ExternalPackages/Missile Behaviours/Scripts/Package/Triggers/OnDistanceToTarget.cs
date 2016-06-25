using UnityEngine;
using System;
using MissileBehaviours.Controller;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers if the distance to the target, specified in the attached missile controller, is below a certain threshold.
    /// </summary>
    [RequireComponent(typeof(MissileController))]
    public class OnDistanceToTarget : TriggerBase
    {

        #region Properties
        /// <summary>
        /// The distance to the target at which the trigger fires.
        /// </summary>
        public float Distance
        {
            get { return distance; }
            set { distance = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("The distance to the target at which the trigger fires.")]
        float distance = 2000;
        #endregion

        #region Fields
        MissileController controller;
        #endregion

        void Awake()
        {
            controller = GetComponent<MissileController>();
        }

        void Update()
        {
            if (!controller.Target)
                return;

            if (Vector3.Distance(transform.position, controller.Target.position) <= distance)
            {
                FireTrigger(this, EventArgs.Empty);
            }
        }
    }
}
