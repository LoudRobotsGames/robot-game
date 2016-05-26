using UnityEngine;
using System.Collections;
using MissileBehaviours.Controller;

namespace MissileBehaviours.Actions
{    /// <summary>
     /// As soon as one of the specified triggers fire this script will instantly rotate the object to a random rotation on its own forward axis. 
     /// It will then continuously rotate on its up/down axes. This will result in a fairly random movement. 
     /// When used on several missiles at the same time it will create a dispersion effect.
     /// If the script is triggered while it's already running, it will start over. That means it will pick a new random rotation and run for the whole duration again.
     /// Once the duration is over, the script will fire an event.
     /// </summary>
    [RequireComponent(typeof (MissileController))]
    public class Disperse : ActionBase
    {
        #region Properties
        /// <summary>
        /// Defines after how many seconds the script starts working. Note that changing this value while the script is running will pause it until the new delay is reached.
        /// This will also reset the duration. That means that changing this to a higher value, while the script is already running, will essentially restart the script at a later point.
        /// It will however not pick a new rotation.
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
        /// <summary>
        /// Defines how many seconds the script should run. If this value is changed to a value smaller than the time the script already ran, it will stop immediately. 
        /// </summary>
        public float Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
            }
        }
        /// <summary>
        /// Defines the maximum rotation speed of the missile. In degrees per second. Its actual rotation may vary, because it's generated randomly.
        /// </summary>
        public float RotationSpeed
        {
            get
            {
                return rotationSpeed;
            }
            set
            {
                rotationSpeed = value;
            }
        }
        /// <summary>
        /// Defines wether or not the script should be removed once it has run for its full duration. 
        /// Note that this is essentially the same thing as "Destroy when fired", however for this to work no other script has to be listening.
        /// </summary>
        public bool RemoveWhenDone
        {
            get
            {
                return removeWhenDone;
            }
            set
            {
                removeWhenDone = value;
            }
        }
        /// <summary>
        /// Defines wether or not the script should consider the maximum rotation speed of the attached Missile Controller.
        /// </summary>
        public bool ConsiderController
        {
            get
            {
                return considerController;
            }
            set
            {
                considerController = value;
            }
        }
        /// <summary>
        /// Defines wether or not the script is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("After how many seconds the script starts working.")]
        float delay = 0f;
        [SerializeField, Tooltip("How many seconds the script should run.")]
        float duration = 1;
        [SerializeField, Tooltip("The maximum rotation speed of the missile. In degrees per second. Its actual rotation may vary, because it's generated randomly.")]
        float rotationSpeed = 10;
        [SerializeField, Tooltip("Wether or not the script should be removed, once it has run for its full duration.")]
        bool removeWhenDone = true;
        [SerializeField, Tooltip("Wether or not the script should consider the maximum rotational speed of the attached Missile Controller.")]
        bool considerController = true;
        [SerializeField, Tooltip("Wether or not the script should be triggered as soon as the object is instantiated.")]
        bool triggerImmediately = true;
        #endregion

        #region Fields
        float elapsedTime;
        MissileController controller;
        bool isRunning;
        #endregion

        /// <summary>
        /// This method gets called as soon as one of the listed triggers fires. It then runs the actual script. If called again it will reset the script and run it again.
        /// </summary>
        internal override void OnTrigger(object sender, System.EventArgs e)
        {
            if (!wasTriggered && this.enabled)
            {
                wasTriggered = true;
                isRunning = true;
                elapsedTime = 0;
                // Rotate the missile on its forward axis.
                transform.Rotate(transform.forward * Random.Range(-180, 180), Space.World);
            }
        }

        void Start()
        {
            controller = GetComponent<MissileController>();
            if (triggerImmediately)
                OnTrigger(this, System.EventArgs.Empty);
        }

        void FixedUpdate()
        {
            // If the script isn't supposed to run, we simply return here.
            if (!isRunning)
                return;

            elapsedTime += Time.deltaTime;

            // If the life time of the object is greater than the set duration and the set delay combined then we know that the script should stop.
            // Therefor we fire the trigger and destroy the script if 'removeWhenDone' is true.
            if (elapsedTime > duration + delay)
            {
                isRunning = false;
                FireTrigger(this, System.EventArgs.Empty);
                if (removeWhenDone)
                    Destroy(this);
                return;
            }

            // If the life time of the object is greater than the set delay, we run the actual script. 
            // If we are considering the controllers maximum rotation speed, we rotate by using the controllers Rotate method, instead of using the transform directly.
            // Otherwise we just use the transform.
            if (elapsedTime > delay)
            {
                if (considerController)
                    controller.Rotate(transform.rotation * Quaternion.LookRotation(transform.up * Random.Range(-rotationSpeed, rotationSpeed) * Time.deltaTime, Vector3.up));
                else
                    transform.Rotate(transform.up, Random.Range(-rotationSpeed, rotationSpeed) * Time.deltaTime, Space.World);
            }
        }
    }
}