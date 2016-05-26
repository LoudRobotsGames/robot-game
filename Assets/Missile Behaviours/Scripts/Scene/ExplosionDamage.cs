using UnityEngine;
using MissileBehaviours.Misc;

namespace MissileBehaviours.Scene
{
    /// <summary>
    /// A very specific script to apply damage to all targets when a missile explodes. You probably want to replace this with your own implementation.
    /// </summary>
    public class ExplosionDamage : MonoBehaviour
    {
        [Tooltip("The smoke prefab that's used for the debris.")]
        public GameObject smokePrefab;
        [Tooltip("The dust prefab that's used when debris hits the ground.")]
        public GameObject dustPrefab;
        [Tooltip("Layers that are affected by the explosion force.")]
        public LayerMask affectedLayers;
        [Tooltip("The range of the explosion force.")]
        public float range = 30;
        [Tooltip("The power of the explosion force.")]
        public float power = 3000;

        void Start()
        {
            // Get all the colliders one the affectedLayers in range.
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, affectedLayers);

            // If we find no colliders, there is nothing to do.
            if (colliders.Length > 0)
            {
                foreach (Collider c in colliders)
                {
                    GameObject parent = c.gameObject;

                    // Add a destruction timer, a rigidbody and both effects to every child of the found objects.
                    for (int i = 0; i < parent.transform.childCount; i++)
                    {
                        Transform child = parent.transform.GetChild(i);

                        // Remove the target indicator and skip the rest, because the target indicator doesn't need it.
                        if (child.name == "TargetIndicator")
                        {
                            Destroy(child.gameObject);
                            continue;
                        }

                        child.gameObject.AddComponent<DestroyAfterTime>().time = 30;

                        Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();

                        rb.velocity = parent.GetComponent<Move>().speed * parent.transform.forward;
                        rb.AddExplosionForce(power, transform.position, range);
                        rb.drag = 0.1f;

                        GameObject smoke = Instantiate(smokePrefab, child.position, Quaternion.identity) as GameObject;
                        smoke.transform.parent = child;

                        child.gameObject.AddComponent<ImpactDust>().dustPrefab = dustPrefab;
                    }

                    // Detach the children so that the rigidbodies can take over and then destroy the original object.
                    parent.transform.DetachChildren();
                    Destroy(parent);
                }
            }
        }
    }
}