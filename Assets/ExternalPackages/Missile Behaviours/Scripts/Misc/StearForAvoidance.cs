using UnityEngine;

namespace MissileBehaviours.Misc
{
    /// <summary>
    /// A simple script which helps to avoid a collision with the terrain. If the terrain is detected within a certain distance in front of the object,
    /// it will rotate on the x-axis to gain altitude again.
    /// </summary>
    public class StearForAvoidance : MonoBehaviour
    {
        [Tooltip("How far away an object can be before it is detected.")]
        public float distance = 200;
        [Tooltip("How fast this gameobject can rotate in order to avoid another object.")]
        public float maxRotation = 50;
        [Tooltip("Defines on what layers the script should look for obstacles.")]
        public LayerMask layersToAvoid;

        RaycastHit hitInfo;

        void Update()
        {
            if (Physics.Raycast(transform.position, transform.forward - new Vector3(0, 0.5f, 0), out hitInfo, distance, layersToAvoid))
            {
                transform.Rotate(new Vector3(-hitInfo.normal.y * maxRotation * Time.deltaTime * (1 - hitInfo.distance / distance), 0, 0));
            }
        }
    }
}