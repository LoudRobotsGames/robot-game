using UnityEngine;
using System.Collections;

namespace CodeControl.Example
{
    public static class MathHelper
    {

        public static float EaseOutElastic(float t)
        {
            if (t >= 1)
            {
                return 1.0f;
            }

            float p = .3f;
            float s = p / 4.0f;

            return Mathf.Pow(2, -10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) + 1.0f;
        }

        public static float EaseInOutSin(float t)
        {
            return .5f - .5f * Mathf.Cos(t * Mathf.PI);
        }

    }
}