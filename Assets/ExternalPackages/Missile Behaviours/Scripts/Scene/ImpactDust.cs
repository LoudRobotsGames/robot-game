using UnityEngine;
using System.Collections;
namespace MissileBehaviours.Scene
{
    /// <summary>
    /// Creates dust particles when debris of the targets hits the terrain.
    /// </summary>
    public class ImpactDust : MonoBehaviour
    {
        [Tooltip("The prefab that's used on debris when it hits the ground.")]
        public GameObject dustPrefab;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                return;


            GameObject dust = Instantiate(dustPrefab, transform.position, Quaternion.identity) as GameObject;
            dust.transform.parent = transform;

        }

    }
}
