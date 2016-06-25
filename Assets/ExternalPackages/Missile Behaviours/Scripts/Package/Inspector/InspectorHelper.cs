#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MissileBehaviours.Inspector
{
    /// <summary>
    /// A little helper class for creating inspector windows.
    /// </summary>
    public class InspectorHelper : MonoBehaviour
    {
        /// <summary>
        /// Draws a help box with rich text enabled.
        /// </summary>
        public static string RichHelpBox(string text)
        {
            GUIStyle richTextStyle = GUI.skin.GetStyle("HelpBox");
            richTextStyle.richText = true;
            return EditorGUILayout.TextArea(text, richTextStyle);
        }

        /// <summary>
        /// Converts meters per second to kilometers per hour.
        /// </summary>
        public static float MsToKmh(float ms)
        {
            return ms * 3.6f;
        }
        /// <summary>
        /// Converts kilometers per hour to meters per second.
        /// </summary>
        public static float KmhToMs(float kmh)
        {
            return kmh * 0.277f;
        }
        /// <summary>
        /// Converts meters per seconds to miles per hour.
        /// </summary>
        public static float MsToMph(float ms)
        {
            return ms * 2.236f;
        }
        /// <summary>
        /// Convert miles per hour to meters per second.
        /// </summary>
        public static float MphToMs(float mph)
        {
            return mph * 0.447f;
        }
    }
}
#endif