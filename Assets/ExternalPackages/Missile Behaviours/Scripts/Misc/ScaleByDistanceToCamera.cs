using UnityEngine;
using System.Collections;
namespace MissileBehaviours.Misc
{
    /// <summary>
    /// Scales the gameobject depending on the distance to the camera. If no camera is specified it will use the camera with the 'MainCamera' tag.
    /// If there is no camera with the 'MainCamera' tag either, this script will do nothing.
    /// </summary>
    public class ScaleByDistanceToCamera : MonoBehaviour
    {
        [Tooltip("The apparent size of this object.")]
        public float scaleMultiplier = 1;
        [Tooltip("The camera the scale should be based on. If this is empty the script will attempt to use the camera with the 'MainCamera' tag.")]
        public Camera activeCamera;
        void Update()
        {
            if (activeCamera)
            {
                float scale = Vector3.Distance(transform.position, activeCamera.transform.position);
                scale = Mathf.Sqrt(scale) * scaleMultiplier;
                transform.localScale = new Vector3(scale, scale, scale);
            }
            else if (Camera.main)
            {
                float scale = Vector3.Distance(transform.position, Camera.main.transform.position);
                scale = Mathf.Sqrt(scale) * scaleMultiplier;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}