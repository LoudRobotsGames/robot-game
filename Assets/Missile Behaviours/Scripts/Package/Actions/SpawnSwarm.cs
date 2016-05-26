using MissileBehaviours.Controller;
using System;
using UnityEngine;

namespace MissileBehaviours.Actions
{
    [RequireComponent(typeof(MissileController))]
    public class SpawnSwarm : ActionBase
    {
        #region Properties
        /// <summary>
        /// The prefab this script should spawn.
        /// </summary>
        public GameObject PrefabToSpawn
        {
            get { return prefabToSpawn; }
            set { prefabToSpawn = value; }
        }
        /// <summary>
        /// The amount of prefabs this script should spawn.
        /// </summary>
        public int SpawnCount
        {
            get { return spawnCount; }
            set { spawnCount = value; }
        }
        /// <summary>
        /// The rotation of the newly spawned prefabs. -90 will launch them backwards, 0 will launch them radially away and 90 will launch them forward.
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }
        /// <summary>
        /// The amount of force with which the newly spawned prefabs get launched.
        /// </summary>
        public float Force
        {
            get { return force; }
            set { force = value; }
        }
        /// <summary>
        /// How much of the velocity of the spawner the newly spawned prefabs should inherit. 0 means none, 1 means all of it.
        /// </summary>
        public float InterhitedVelocity
        {
            get { return inheritedVelocity; }
            set { inheritedVelocity = value; }
        }
        #endregion

        #region Serialized Fields
        [SerializeField, Tooltip("The prefab to spawn.")]
        GameObject prefabToSpawn;
        [SerializeField, Tooltip("The number of prefabs to spawn.")]
        int spawnCount;
        [SerializeField, Tooltip("The angle at which the prefabs are spawned. Use -90 to launch them backwards, 0 to launch them radially away and 90 to launch them forward.")]
        float angle = 90;
        [SerializeField, Tooltip("The amount of force with which the prefab is launched.")]
        float force = 0;
        [SerializeField, Tooltip("Wether or not the prefab should inherit its target from this gameobject. Both objects requiere a MissileController for this to work.")]
        bool inheritTarget = true;
        [SerializeField, Tooltip("The amount of velocity the prefab should inherit from this gameobject. Both objects requiere a rigidbody for this to work.")]
        float inheritedVelocity = 1;
        [SerializeField, Tooltip("Wether or not the prefabs should be rotated when spawned.")]
        bool rotateToSwarmDirection = true;
        #endregion

        #region Fields
        MissileController controller;
        Rigidbody rigidBody;
        #endregion

        internal override void Awake()
        {
            base.Awake();
            controller = GetComponent<MissileController>();
            rigidBody = GetComponent<Rigidbody>();
        }

        internal override void OnTrigger(object sender, System.EventArgs e)
        {
            if (!wasTriggered && this.enabled)
            {
                wasTriggered = true;

                // Figure out how far along the circle we need to go for each spawn.
                float step = 360 / (spawnCount * 1.0f);
                Quaternion rotation = Quaternion.identity;

                // Spawn the prefabs.
                for (int i = 0; i < spawnCount; i++)
                {
                    // Create the rotation for the prefabs.
                    rotation = Quaternion.AngleAxis(i * step, transform.forward) * transform.rotation * Quaternion.LookRotation(Vector3.right) * Quaternion.AngleAxis(-angle, Vector3.up);

                    // Spawn a prefab.
                    GameObject spawnedObject = Instantiate(prefabToSpawn, transform.position, transform.rotation) as GameObject;

                    MissileController spawnedController = spawnedObject.GetComponent<MissileController>();
                    Rigidbody spawnedRb = spawnedObject.GetComponent<Rigidbody>();

                    // Set the target for the spawned prefab.
                    if (inheritTarget && spawnedController != null)
                        spawnedController.Target = controller.Target;

                    // Set the rotation for the spawned prefab.
                    if (rotateToSwarmDirection)
                    {
                        spawnedObject.transform.rotation = rotation;
                    }

                    // Set the velocity and force of the prefab.
                    if (spawnedRb != null)
                    {
                        spawnedRb.velocity = rigidBody.velocity * inheritedVelocity;
                        spawnedRb.AddForce(rotation * spawnedObject.transform.forward * force, ForceMode.VelocityChange);
                    }
                }
                FireTrigger(this, EventArgs.Empty);
            }
        }
    }
}
