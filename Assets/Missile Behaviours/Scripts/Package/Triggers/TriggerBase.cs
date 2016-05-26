using UnityEngine;
using System;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// This class represents a somewhat dirty hack, which is necessary because Unity can not display interfaces in the inspector. It can however display abstract classes.
    /// Every trigger extends this class so it can be added to a list of events in an action class (for example the Explosion.cs).
    /// </summary>
    public abstract class TriggerBase : MonoBehaviour
    {
        public event EventHandler Fired;

        #region Properties
        /// <summary>
        /// Gets or sets wether or not this script is destroyed after the trigger fires.
        /// </summary>
        public bool DestroyWhenFired
        {
            get { return destroyWhenFired; }
            set { destroyWhenFired = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("Wether or not this script should be removed once the trigger fired. Note that this only happens when the trigger actually triggered another script. It will not be destroyed if no other script is listening.")]
        bool destroyWhenFired;
        #endregion

        internal void FireTrigger(object sender, EventArgs e)
        {
            if (Fired != null)
            {
                Fired(sender, e);
                if (destroyWhenFired)
                    Destroy(this);
            }
        }
    }
}