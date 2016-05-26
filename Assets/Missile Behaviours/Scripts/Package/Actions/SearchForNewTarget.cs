using UnityEngine;
using MissileBehaviours.Controller;
using System;

namespace MissileBehaviours.Actions
{
    /// <summary>
    /// Defines which target to choose.
    /// </summary>
    public enum TargetingPriority
    {
        Closest, Farthest, Random, 
    }



    /// <summary>
    /// As soon as one of the specified triggers fire this script will perform a search for a new target. If it can not find one it will fire an event itself.
    /// </summary>
    [RequireComponent(typeof(MissileController))]
    public class SearchForNewTarget : ActionBase
    {
        #region Properties
        /// <summary>
        /// Defines what layers the script should search on.
        /// </summary>
        public LayerMask SearchMask
        {
            get { return searchMask; }
            set { searchMask = value; }
        }
        /// <summary>
        /// The radius around the object where it will look for targets.
        /// </summary>
        public float SearchRange
        {
            get { return searchRange; }
            set { searchRange = value; }
        }
        /// <summary>
        /// Defines which target to choose.
        /// </summary>
        public TargetingPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("Defines on what layers the script should look for target.")]
        LayerMask searchMask;
        [SerializeField, Tooltip("Defines the radius for the search.")]
        float searchRange = 500;
        [SerializeField, Tooltip("Defines which target to choose.")]
        TargetingPriority priority;
        #endregion

        #region Fields
        // We need access to the missile controller in order to set its target.
        MissileController controller;
        #endregion

        internal override void Awake()
        {
            base.Awake();
            controller = GetComponent<MissileController>();
        }

        /// <summary>
        /// This method gets called as soon as one of the listed triggers fires. It then performs a search for a new target. If it can not find one it will fire an event.
        /// </summary>
        internal override void OnTrigger(object sender, EventArgs e)
        {
            if (!wasTriggered && this.enabled)
            {
                wasTriggered = true;
                Collider[] targets = Physics.OverlapSphere(transform.position, searchRange, searchMask);

                // If we don't find a single target we fire an event (to trigger an explosion for example) and return, since there is nothing else left to do.
                if (targets.Length == 0)
                {
                    FireTrigger(this, EventArgs.Empty);
                    return;
                }

                // If we only find one target we don't need to check for priorities.
                if (targets.Length > 1)
                {
                    switch (priority)
                    {
                        case TargetingPriority.Closest:
                            {
                                // We remember what the last smallest distance was and then check if another distance is smaller. If so we set that as the new target.
                                float smallestDistance = searchRange;
                                float distance = 0;
                                foreach (Collider c in targets)
                                {
                                    distance = Vector3.Distance(transform.position, c.transform.position);

                                    if (smallestDistance > distance)
                                    {
                                        smallestDistance = distance;
                                        controller.Target = c.transform;
                                    }
                                }
                                break;
                            }
                        case TargetingPriority.Farthest:
                            {
                                // We remember what the last biggest distance was and then check if another distance is bigger. If so we set that as the new target.
                                float biggestDistance = 0;
                                float distance = 0;

                                foreach (Collider c in targets)
                                {
                                    distance = Vector3.Distance(transform.position, c.transform.position);
                                    if (biggestDistance < distance)
                                    {
                                        biggestDistance = distance;
                                        controller.Target = c.transform;
                                    }
                                }
                                break;
                            }
                        case TargetingPriority.Random:
                            {
                                // Choose a random available target.
                                controller.Target = targets[UnityEngine.Random.Range(0, targets.Length - 1)].transform;
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    controller.Target = targets[0].transform;
                }
            }
        }
    }
}