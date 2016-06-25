using UnityEngine;
using MissileBehaviours.Triggers;
using System.Collections.Generic;

namespace MissileBehaviours.Actions
{
    public abstract class ActionBase : TriggerBase
    {
        #region Properties
        /// <summary>
        /// The list of triggers this action uses.
        /// </summary>
        public List<TriggerBase> Triggers
        {
            get { return triggers; }
        }
        /// <summary>
        /// Wether or not this action was already fired this frame.
        /// </summary>
        public bool WasTriggered
        {
            get { return wasTriggered; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField]
        internal List<TriggerBase> triggers;
        #endregion

        #region Fields
        // Most actions should only be allowed to trigger once or once per frame. Use this variable to find out if it was already triggered this frame.
        internal bool wasTriggered;
        #endregion


        internal virtual void Awake()
        {
            // Subscribe to all events which were added to the trigger list via the inspector.
            foreach (TriggerBase t in triggers)
            {
                t.Fired += OnTrigger;
            }
        }

        /// <summary>
        /// This method gets called as soon as one of the listed triggers fires.
        /// </summary>
        internal abstract void OnTrigger(object sender, System.EventArgs e);

        internal virtual void Update ()
        {
            wasTriggered = false; // Resets the fired variable.
        }

        internal void OnDestroy()
        {
            // Unsubscribe from all events. This is only needed if you want to remove only this script, while keeping at least one trigger this script has subscribed to.
            // If you do not unsubscribe in that situation, the script will be kept alive by the references created through the use of events.
            foreach (TriggerBase t in triggers)
            {
                t.Fired -= OnTrigger;
            }
        }
    }
}