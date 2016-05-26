using UnityEngine;
using System.Collections;

namespace MissileBehaviours.Scene
{
    /// <summary>
    /// Switches the skybox and light settings on the press of a button.
    /// </summary>
    public class DayNightSwitch : MonoBehaviour
    {
        [Tooltip("The skybox at day.")]
        public Material daySkybox;
        [Tooltip("The skybox at night.")]
        public Material nightSkybox;
        [Tooltip("The main light of the scene.")]
        public Light sun;

        // The intensity of the light at day and night.
        public float dayLight = 0.5f;
        public float nightLight = 0;

        // The key to switch.
        public KeyCode switchKey = KeyCode.N;

        bool isNight;

        void Update()
        {
            if (Input.GetKeyDown(switchKey))
            {
                isNight = !isNight;

                if (isNight)
                {
                    RenderSettings.skybox = nightSkybox;
                    if (sun)
                        sun.intensity = nightLight;
                }
                else
                {
                    RenderSettings.skybox = daySkybox;
                    if (sun)
                        sun.intensity = dayLight;
                }
            }
        }
    }
}