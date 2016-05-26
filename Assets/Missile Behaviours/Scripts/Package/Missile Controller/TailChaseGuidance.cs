using UnityEngine;
using System.Collections;

namespace MissileBehaviours.Controller
{
    /// <summary>
    /// Implements a tail chase guidance which always rotates the gameobject towards the target. This approach is almost always worse than proportional navigation, 
    /// but is useful if you desire a less expensive or less accurate guidance. For example for balance reasons. Note that some sort of promixity trigger is adviced, since this approach is likely
    /// to miss targets that are either too small, too fast or too close. It is also adviced to use a higher drag value for the same reason.
    /// </summary>
    [RequireComponent(typeof(MissileController))]
    public class TailChaseGuidance : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Gets or sets wether or not the advanced mode is used. The normal mode is a simple lookat function which always tries to rotate the missile towards the target. The advanced version considers the velocity of the missile relative to its target
        /// and aims towards the resulting direction. Under certain circumstances this leads to a better chance to hit a moving target. However result may vary, depending on the drag of the rigidbody,
        /// the force of the missile and its maximum rotation. It can be quite hard to predict which version will perform better under certain circumstances so I suggest testing them both. 
        /// Note that this is just an experimental attempt to improve the hit rate of the tail-chase approach. Its not guaranteed that this will improve the hit rate in any way.
        /// </summary>
        public bool UseAdvancedMode
        {
            get
            {
                return advancedMode;
            }
            set
            {
                advancedMode = value;
            }
        }
        /// <summary>
        /// Gets or sets the time, in seconds, after which the guidance becomes active. Note that setting this to a value higher than the current life time of the missile will disable the guidance until
        /// the set time is reached again.
        /// </summary>
        public float Delay
        {
            get
            {
                return delay;
            }
            set
            {
                delay = value;
            }
        }

        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("Wether or not this missile should use the experimental advanced mode, which tries to improve the poor hitrate by takeing the relative acceleation of the missile and its target into account. Note that this may not work at all in some cases, its completely experimental.")]
        bool advancedMode;
        [SerializeField, Tooltip("The time, in seconds, after which the guidance becomes active.")]
        float delay;
        #endregion

        #region Fields
        MissileController controller;
        float elapsedTime;
        float currentDistance;
        float previousDistance;
        #endregion

        void Awake()
        {
            controller = GetComponent<MissileController>();
        }

        void Update()
        {
            // If there is no target set, there is no need for calculations.
            if (!controller.Target)
                return;

            elapsedTime += Time.deltaTime;

            // Only use guidance when the life time of the missile is greater than the set delay.
            if (elapsedTime < delay)
                return;

            if (advancedMode)
            {
                // The advanced mode only works when the missile is closing in on the target, so we need to make sure that it only runs if that's the case.
                // Otherwise we just use the normal lookat method to rotate.
                currentDistance = Vector3.Distance(transform.position, controller.Target.position);
                if (currentDistance < previousDistance)
                {
                    // This little function reflects the velocity vector along the LoS vector. The resulting vector "aims ahead" to overcome the missiles inertia more quickly.
                    Vector3 velocity = -controller.Velocity.normalized;
                    Vector3 los = (controller.Target.position - transform.position).normalized;
                    Vector3 modifiedRotation = Vector3.Reflect(velocity, los);

                    controller.Rotate(Quaternion.LookRotation(modifiedRotation)); // Use the Rotate function of the controller, instead of the transforms, to consider the rotation rate of the missile.
                }
                else
                {
                    controller.Rotate(Quaternion.LookRotation(controller.Target.position - transform.position, transform.up)); // Use the Rotate function of the controller, instead of the transforms, to consider the rotation rate of the missile.
                }

                previousDistance = currentDistance;
            }
            else
            {
                controller.Rotate(Quaternion.LookRotation(controller.Target.position - transform.position, transform.up));
            }
        }
    }
}