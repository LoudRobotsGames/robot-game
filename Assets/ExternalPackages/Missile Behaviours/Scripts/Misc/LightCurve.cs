using UnityEngine;
using System.Collections;
namespace MissileBehaviours.Misc
{
    [RequireComponent(typeof(Light))]
    public class LightCurve : MonoBehaviour
    {
        [Tooltip("The curve which defines the intensity of the light over time.")]
        public AnimationCurve lightCurve;

        float elapsedTime;

        void Update()
        {
            elapsedTime += Time.deltaTime;
            GetComponent<Light>().intensity = lightCurve.Evaluate(elapsedTime);
        }
    }
}
