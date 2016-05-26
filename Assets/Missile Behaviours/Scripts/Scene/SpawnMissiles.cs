using MissileBehaviours.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MissileBehaviours.Scene
{
    public class SpawnMissiles : MonoBehaviour
    {
        [Tooltip("The available missile types.")]
        public List<GameObject> missilePrefabs;
        [Tooltip("Where to spawn the missiles.")]
        public Transform spawnPosition;
        [Tooltip("The text to display which missile type is selected.")]
        public Text selectedMissileText;

        [Tooltip("The key to select the next missile type.")]
        public KeyCode nextMissile = KeyCode.E;
        [Tooltip("The key to select the previous missile type.")]
        public KeyCode previousMissile = KeyCode.Q;
        [Tooltip("The key to fire a missile.")]
        public KeyCode fireMissile = KeyCode.Space;
        [Tooltip("The key to show/hide the trail renderers.")]
        public KeyCode toggleTrails = KeyCode.L;
        [Tooltip("The layer on which the script should look for targets.")]
        public LayerMask targetLayer;
        [Tooltip("How far away a target can be before it is detected.")]
        public float targetFinderRange = 5000;
        // Wether or not the trails are visible.
        public static bool trailsVisible;
        int selectedIndex;

        void Update()
        {
            if (Input.GetKeyDown(toggleTrails))
                trailsVisible = !trailsVisible;

            if (Input.GetKeyDown(nextMissile))
            {
                if (selectedIndex == missilePrefabs.Count - 1)
                    selectedIndex = 0;
                else
                    selectedIndex++;
            }

            if (Input.GetKeyDown(previousMissile))
            {
                if (selectedIndex == 0)
                    selectedIndex = missilePrefabs.Count - 1;
                else
                    selectedIndex--;
            }

            if (Input.GetKeyDown(fireMissile))
            {
                // Find target
                Transform target = null;
                Collider[] foundColliders = Physics.OverlapSphere(transform.position, targetFinderRange, targetLayer);

                if (foundColliders.Length != 0)
                {
                    target = foundColliders[Random.Range(0, foundColliders.Length - 1)].transform;
                }

                GameObject missile = Instantiate(missilePrefabs[selectedIndex], spawnPosition.position, spawnPosition.rotation) as GameObject;

                missile.GetComponent<MissileController>().Target = target;

            }

            selectedMissileText.text = "Selected Missile: " + missilePrefabs[selectedIndex].name;
        }
    }
}