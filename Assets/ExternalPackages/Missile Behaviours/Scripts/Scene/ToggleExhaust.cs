using MissileBehaviours.Controller;
using UnityEngine;

namespace MissileBehaviours.Scene
{
    /// <summary>
    /// Disables the particle system on this gameobject if the missile controller of the parent isn't accelerating. Otherwise its emission rate is set to the throttle of the missile controller.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ToggleExhaust : MonoBehaviour
    {
        ParticleSystem exhaust;
        MissileController controller;

        void Awake ()
        {
            exhaust = GetComponent<ParticleSystem>();
            controller = GetComponentInParent<MissileController>();
        }

        void Update()
        {
            if (controller)
            {
                if (controller.IsAccelerating)
                    exhaust.emissionRate = controller.Throttle;
                else
                    exhaust.emissionRate = 0;
            }
        }
    }
}