using UnityEngine;
using System;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers if this gameobject collides with something.
    /// </summary>
    public class OnCollision : TriggerBase
    {
        #region Properties
        public LayerMask CollisionMask
        {
            get { return collisionMask; }
            set { collisionMask = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("Additionally to the physics settings, this trigger can ignore collision on certain layers.")]
        LayerMask collisionMask;
        #endregion

        void OnCollisionEnter(Collision collision)
        {
            if (collisionMask == (collisionMask | (1 << collision.gameObject.layer)))
                FireTrigger(this, EventArgs.Empty);
        }
    }
}