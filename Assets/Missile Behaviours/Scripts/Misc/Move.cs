using UnityEngine;

namespace MissileBehaviours.Misc
{
    // Moves the object forward without using a rigidbody.
    public class Move : MonoBehaviour
    {
        [Tooltip("The speed of the object, in units per second.")]
        public float speed = 10;

        void Update()
        {
            // Moves the object forward independent of the framerate.
            transform.position += Time.deltaTime * transform.forward * speed;
        }
    }
}
