using UnityEngine;
using System.Collections;
using System;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers if the height (y-coordinate) of this gameobject is greater than a certain threshold.
    /// </summary>
    public class OnHeight : TriggerBase
    {
        #region Properties
        /// <summary>
        /// The height at which the trigger fires.
        /// </summary>
        public float Height
        {
            get { return height; }
            set { height = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("The height at which the trigger fires.")]
        float height = 2000;
        #endregion

        void Update()
        {
            if (transform.position.y >= height)
                FireTrigger(this, EventArgs.Empty);
        }
    }
}