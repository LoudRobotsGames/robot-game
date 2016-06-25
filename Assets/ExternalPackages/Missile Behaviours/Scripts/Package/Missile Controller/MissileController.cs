using UnityEngine;

namespace MissileBehaviours.Controller
{
    /// <summary>
    /// This class contains all information about a missile, such as its speed, rotational velocity and its target.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MissileController : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// Gets or sets the amount of force with which the missile will accelerate, ignoring mass.
        /// </summary>
        public float Force
        {
            get
            {
                return force;
            }
            set
            {
                force = value;
            }
        }
        /// <summary>
        /// Gets or sets the maximum rotation this missile can achieve. In degrees per second.
        /// </summary>
        public float MaxRotation
        {
            get
            {
                return maxRotation;
            }
            set
            {
                maxRotation = Mathf.Abs(value);
            }
        }
        /// <summary>
        /// Gets or sets the initial force applied to this missile when it's created.
        /// </summary>
        public float InitialImpulse
        {
            get
            {
                return initialImpulse;
            }
            set
            {
                initialImpulse = value;
            }
        }
        /// <summary>
        /// The time, in seconds, after which the acceleration starts.
        /// </summary>
        public float Delay
        {
            get { return delay; }
            set { delay = value; }
        }
        /// <summary>
        /// The throttle of the missile. In percent, where 0 means 0% and 1 means 100%. This affects acceleration and fuel consumption.
        /// </summary>
        public float Throttle
        {
            get { return throttle; }
            set { throttle = value; }
        }
        /// <summary>
        /// Gets the current velocity of this missile.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return rb.velocity;
            }
        }
        /// <summary>
        /// Gets or sets the current position of the target.
        /// </summary>
        public Transform Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }
        /// <summary>
        /// Gets the approximate maximum speed the missile can reach when flying on a horizontal course.
        /// The actual maximum speed will be lower when ascending and higher when descending.
        /// This value is recalculated each time it is requested, so you may want to cache it if you need it more than once.
        /// </summary>
        public float MaxSpeed
        {
           get
            {
                // Make sure the rigidbody is setup to avoid null reference errors.
                if (!rb)
                    rb = GetComponent<Rigidbody>();

                return (Force / rb.drag) - (Time.fixedDeltaTime * Force);
            }
        }
        /// <summary>
        /// Gets the approximate time the missile will need to do a 360° turn.
        /// This value is recalculated each time it is requested, so you may want to cache it if you need it more than once.
        /// </summary>
        public float RotationRate
        {
            get
            {
                return 360 / MaxRotation;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of fuel this missile can carry. Can't be negative. If this is set to a lower value than the remaining fuel that value will also decrease.
        /// </summary>
        public float MaximumFuel
        {
            get
            {
                return maximumFuel;
            }
            set
            {
                maximumFuel = Mathf.Clamp(value, 0, float.MaxValue);
                if (fuelRemaining > maximumFuel)
                    fuelRemaining = maximumFuel;
            }
        }
        /// <summary>
        /// Gets or sets the fuel consumption of this missile. Use 0 to disable the fuel feature completely.
        /// </summary>
        public float FuelConsumption
        {
            get
            {
                return fuelConsumption;
            }
            set
            {
                fuelConsumption = value;
            }
        }
        /// <summary>
        /// Gets or sets the amount of fuel the missile has left. If this is set to a higher value than the missiles maximum fuel that value will also increase.
        /// </summary>
        public float FuelRemaining
        {
            get
            {
                if (fuelRemaining < 0)
                    fuelRemaining = 0;

                return fuelRemaining;
            }
            set
            {
                if (value > maximumFuel)
                    maximumFuel = value;
                fuelRemaining = value;
            }
        }
        /// <summary>
        /// Gets the amount of seconds before this missile runs out of fuel when its tanks are full. This value is calculated each time it is requested.
        /// If you use it several times, consider cacheing it.
        /// </summary>
        public float Burntime
        {
            get
            {
                if (fuelConsumption > 0 && maximumFuel > 0)
                    return maximumFuel / (fuelConsumption * throttle);
                else
                    return 0;
            }
        }
        /// <summary>
        /// Gets the amount of seconds before this missile runs out of the fuel it has left. This value is calculated each time it is requested.
        /// If you use it several times, consider cacheing it.
        /// </summary>
        public float RemainingBurnTime
        {
            get
            {
                if (fuelConsumption > 0 && fuelRemaining > 0)
                    return fuelRemaining / (fuelConsumption * throttle);
                else
                    return 0;
            }
        }
        /// <summary>
        /// If the throttle is not 0 and the missile already lives longer than the set delay the missile is accelerating. If the fuel feature is used this will only return 
        /// true if the missile has fuel left.
        /// </summary>
        public bool IsAccelerating
        {
            get
            {
                // If the throttle is zero or the delay isn't over yet the missile will not accelerate.
                if (throttle != 0 && elapsedTime > delay)
                {
                    // Find out wether or not the missile uses the fuel feature.
                    if (FuelConsumption > 0)
                    {
                        // If the missiles uses the fuel feature we still need to make sure that it has fuel
                        if (fuelRemaining > 0)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return false;
            }
        }
        #endregion
        #region Serialized Fields
        // See MissileControllerInspector for the tooltips.
        [SerializeField]
        float force = 50;
        [SerializeField]
        float maxRotation = 90;
        [SerializeField]
        float initialImpulse = 0;
        [SerializeField]
        float delay;
        [SerializeField]
        Transform target;
        [SerializeField]
        float maximumFuel;
        [SerializeField]
        float fuelConsumption;
        [SerializeField]
        float throttle = 1;
        #endregion

        #region Fields
        Rigidbody rb;
        float fuelRemaining;
        float elapsedTime;
        #endregion

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            fuelRemaining = maximumFuel;
        }

        void Start()
        {
            // Apply impulse
            rb.AddForce(transform.forward * initialImpulse, ForceMode.Acceleration);
        }

        void FixedUpdate()
        {
            elapsedTime += Time.deltaTime;

            // Only accelerate when the life time of the missile is greater than the set delay.
            if (elapsedTime < delay)
                return;

            // Only calculate the fuel consumption if the missile does actually consume fuel and has fuel left. Otherwise just accelerate normally.
            if (fuelConsumption > 0)
            {
                if (fuelRemaining > 0)
                {
                    rb.AddForce(transform.forward * force * throttle, ForceMode.Acceleration);
                    fuelRemaining -= fuelConsumption * throttle * Time.fixedDeltaTime;
                }
            }
            else
                rb.AddForce(transform.forward * force * throttle, ForceMode.Acceleration);
        }

        /// <summary>
        /// Rotates the missile towards the specefied rotation while considering its rotation rate.
        /// </summary>
        /// <param name="desiredRotation">The rotation to rotate towards.</param>
        public void Rotate(Quaternion desiredRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, Time.deltaTime * maxRotation);
        }
    }
}