using System;
using UnityEngine;

namespace MissileBehaviours.Actions
{
    /// <summary>
    /// Instantiates an explosion prefab and destroys this gameobject as soon as one of the specified triggers fire.
    /// Once triggered, it also fires an event on its own.
    /// </summary>
    public class Explode : ActionBase
    {
        [Tooltip("The prefab that gets instantiated once this action is triggered.")]
        public GameObject explosionPrefab;
        [Tooltip("When triggered this script destroys the gameobject. Use this to introduce a delay for the destruction. Any value bigger than 0 will make it wait at least one frame. This way you can ensure that other things that need to be done this frame will be doen first.")]
        public float destructionDelay;

        /// <summary>
        /// This method gets called as soon as one of the listed triggers fires. It then instantiates a prefab which controls the actual explosion and destroys this gameobject.
        /// </summary>
        internal override void OnTrigger(object sender, EventArgs e)
        {
            if (!wasTriggered && this.enabled)
            {
                wasTriggered = true;
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                FireTrigger(this, EventArgs.Empty);
                Destroy(gameObject, destructionDelay);
            }
        }
    }
}