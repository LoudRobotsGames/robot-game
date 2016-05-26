using UnityEngine;

namespace MissileBehaviours.Misc
{
    /// <summary>
    /// Destroys the gameobject after the set amount of seconds.
    /// </summary>
    public class DestroyAfterTime : MonoBehaviour
    {
        [Tooltip("The time, in seconds, after which this Gameobject is destroyed.")]
        public float time = 10;
        void Start()
        {
            Destroy(gameObject, time);
        }
    }
}
