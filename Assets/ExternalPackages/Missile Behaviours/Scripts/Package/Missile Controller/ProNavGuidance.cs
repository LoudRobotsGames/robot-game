using UnityEngine;
using System.Collections;
namespace MissileBehaviours.Controller
{
    /// <summary>
    /// This class implements proportional navigation which makes the gameobject rotate in the same direction as the line of sight to the target does. This results in an intercept course.
    /// </summary>
    [RequireComponent(typeof(MissileController))]
    public class ProNavGuidance : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Gets or sets the navigational constant. The rotational change in the line of sight between the missile and the target is multiplied by this value. It should be greater than 1.
        /// Even though it is called a constant, feel free to change this at any point to get more control over the missile.
        /// </summary>
        public float NavigationalConstant
        {
            get
            {
                return navigationalConstant;
            }
            set
            {
                navigationalConstant = value;
            }
        }
        /// <summary>
        /// Gets or sets wether or not the advanced mode is used. The advanced mode, often called "augmented proportional navigation", adds a term to consider acceleration. Usually the less expensive 'normal' mode should be good enough though.
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
        [SerializeField, Tooltip("Wether or not to use the augmented version of the algorithm, which is a little more expensive to compute, but can improve the hitrate even further.")]
        bool advancedMode;
        [SerializeField, Tooltip("The time, in seconds, after which the guidance becomes active.")]
        float delay;
        [SerializeField, Tooltip("The navigational constant defines how fast the missile tries to rotate. High values will enable it to reach the perfect course more quickly, but may lead to overshooting issues once the target is close")]
        float navigationalConstant = 5;
        #endregion

        #region Fields
        Vector3 previousLos;
        Vector3 los;
        Vector3 losDelta;
        Vector3 desiredRotation;
        MissileController controller;
        float elapsedTime;
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


            // Do some fancy math which is too complicated for a little comment. I suggest reading the Wikipedia entry on proportional navigation if you want to know how it works.
            previousLos = los;
            los = controller.Target.position - transform.position;
            losDelta = los - previousLos;

            losDelta = losDelta - Vector3.Project(losDelta, los);

            if (advancedMode)
                desiredRotation = (Time.deltaTime * los) + (losDelta * navigationalConstant) + (Time.deltaTime * desiredRotation * navigationalConstant * 0.5f); // Augmented version of proportional navigation.
            else
                desiredRotation = (Time.deltaTime * los) + (losDelta * navigationalConstant); // Plain proportional navigation.

            controller.Rotate(Quaternion.LookRotation(desiredRotation, transform.up)); // Use the Rotate function of the controller, instead of the transforms, to consider the rotation rate of the missile.
        }
    }
}