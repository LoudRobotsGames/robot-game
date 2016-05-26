using UnityEngine;

namespace MissileBehaviours.Scene
{
    public class SpawnTargets : MonoBehaviour
    {
        [Tooltip("The prefab to spawn.")]
        public GameObject targetPrefab;
        [Tooltip("How far away targets are spawned from the center of the scene.")]
        public float spawnDistance = 3000;
        [Tooltip("How long the script will wait to spawn a new target. In seconds.")]
        public float spawnInterval = 3;
        [Tooltip("The minimum height at which a target will spawn.")]
        public float minHeight = 1000;
        [Tooltip("The maximum height at which a target will spawn.")]
        public float maxHeight = 2000;
        [Tooltip("The key to manually spawn a target.")]
        public KeyCode spawnKey = KeyCode.Return;
        [Tooltip("The key to toggle automatic spawning.")]
        public KeyCode spawnToggleKey = KeyCode.T;

        float elapsedTime;
        bool spawnEnabled = true;

        void Start()
        {

        }

        void Update()
        {
            if (Input.GetKeyDown(spawnToggleKey))
                spawnEnabled = !spawnEnabled;

            if (Input.GetKeyDown(spawnKey))
                SpawnTarget();

            if (spawnEnabled)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= spawnInterval)
                {
                    elapsedTime = 0;

                    SpawnTarget();
                }
            }
        }

        void SpawnTarget ()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 position = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized * spawnDistance;

            float spawnHeight = Random.Range(minHeight, maxHeight);

            GameObject target = Instantiate(targetPrefab, new Vector3(position.x, spawnHeight, position.y), Quaternion.identity) as GameObject;
            target.transform.LookAt(Random.insideUnitSphere * spawnDistance * 0.5f);
        }
    }
}