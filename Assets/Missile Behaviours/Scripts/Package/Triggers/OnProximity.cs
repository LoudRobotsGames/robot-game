using UnityEngine;
using System;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers if this gameobject comes close to any object on the specified layers. 
    /// </summary>
    public class OnProximity : TriggerBase
    {
        #region Properties
        /// <summary>
        /// The layers to consider.
        /// </summary>
        public LayerMask CollisionMask
        {
            get { return collisionMask; }
            set { collisionMask = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("The layers to consider for the proximity check.")]
        LayerMask collisionMask;
        [SerializeField, Tooltip("The maximum distance to an object before the trigger fires.")]
        float proximityDistance;
        #endregion

        void Update()
        {
            if (Physics.OverlapSphere(transform.position, proximityDistance, collisionMask).Length > 0)
            {
                FireTrigger(this, EventArgs.Empty);
            }
        }
    }
}