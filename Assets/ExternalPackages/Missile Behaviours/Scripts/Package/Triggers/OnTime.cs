using UnityEngine;
using System;

namespace MissileBehaviours.Triggers
{
    /// <summary>
    /// Triggers after a specified amount of time.
    /// </summary>
    public class OnTime : TriggerBase
    {
        #region Properties
        /// <summary>
        /// Gets or sets the time after which this trigger fires.
        /// </summary>
        public float TheTime
        {
            get { return time; }
            set { time = value; }
        }
        /// <summary>
        /// Wether or not the timer should start over once it fired. Note that this will not work if DestroyWhenFired is true.
        /// </summary>
        public bool Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("The time after which the trigger fires.")]
        float time = 10;
        [SerializeField, Tooltip("Wether or not the timer should start over once it fired. Note that this will not work if DestroyWhenFired is true.")]
        bool repeat;
        #endregion

        #region Fields
        float elapsedTime;
        #endregion

        void Update()
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= time)
            {
                if (repeat)
                    elapsedTime = 0;
                FireTrigger(this, EventArgs.Empty);
            }
        }
    }
}