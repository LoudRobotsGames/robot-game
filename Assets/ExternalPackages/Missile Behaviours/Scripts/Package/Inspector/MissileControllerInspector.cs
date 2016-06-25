#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MissileBehaviours.Controller;
using System;

namespace MissileBehaviours.Inspector
{
    /// <summary>
    /// Creates a custom inspector window for the MissileController.
    /// </summary>
    [CustomEditor(typeof(MissileController))]
    public class MissileControllerInspector : Editor
    {
        MissileController controller;
        Rigidbody rb;
        bool showStats;

        public override void OnInspectorGUI()
        {
            controller = (MissileController)target;
            rb = controller.GetComponent<Rigidbody>();

            #region Movement
            controller.Force = EditorGUILayout.FloatField(new GUIContent("Force", "The acceleration force of the missile."), Mathf.Clamp(controller.Force, 0, float.MaxValue));
            controller.MaxRotation = EditorGUILayout.FloatField(new GUIContent("Max Rotation", "The maximum rotation of the missile. In degrees per second."), controller.MaxRotation);
            controller.InitialImpulse = EditorGUILayout.FloatField(new GUIContent("Initial Impulse", "This force is applied immediately when the missile is created."), controller.InitialImpulse);
            controller.Delay = EditorGUILayout.FloatField(new GUIContent("Delay", "Defines after how many seconds the acceleration sets in."), controller.Delay);
            controller.Throttle = EditorGUILayout.FloatField(new GUIContent("Throttle", "The throttle of the missile in percent, where 0 means 0% and 1 means 100%. Will affect acceleration and fuel consumption."), controller.Throttle);
            controller.Target = EditorGUILayout.ObjectField(new GUIContent("Target", "The target of the missile."), controller.Target, typeof(Transform), true) as Transform;
            #endregion

            EditorGUILayout.Space();

            #region Fuel
            controller.MaximumFuel = EditorGUILayout.FloatField(new GUIContent("Fuel", "How much fuel this missile can carry."), controller.MaximumFuel);
            controller.FuelConsumption = EditorGUILayout.FloatField(new GUIContent("Fuel Consumption", "How much fuel this missile consumes per second."), controller.FuelConsumption);
            EditorGUILayout.FloatField(new GUIContent("Fuel Remaining:", "How much fuel the missile has left."), controller.FuelRemaining);
            #endregion

            #region Stats
            showStats = EditorGUILayout.Foldout(showStats, "Stats");

            if (showStats)
            {
                #region Movement Stats
                if (rb)
                {
                    if (rb.drag == 0)
                    {
                        if (controller.Force != 0)
                            EditorGUILayout.HelpBox("Since the attached Rigidbody has no drag, this missile has no maximum speed. This will make it very hard for the missile to turn and it's therefore unlikely to hit its target.", MessageType.Info);
                    }
                    else
                    {
                        if (controller.MaxSpeed > 0)
                        {
                            EditorGUILayout.HelpBox("Assuming that one unit equals one meter we get the following stats:", MessageType.None);

                            InspectorHelper.RichHelpBox("Maximum Speed: <b>~" + Math.Round(controller.MaxSpeed, 2) + " m/s</b> or <b>~" + Math.Round(InspectorHelper.MsToKmh(controller.MaxSpeed), 2) + " km/h</b> or <b>~" + Math.Round(InspectorHelper.MsToMph(controller.MaxSpeed), 2) + " mph.</b>");
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("The specified force is not enough to overcome the specified drag. The missile will not move.", MessageType.Warning);
                        }
                    }
                    if (controller.MaxRotation > 0)
                    InspectorHelper.RichHelpBox("Turn Rate: The missile will need <b> ~" + Math.Round(controller.RotationRate, 2) + " seconds </b>for a full turn.");
                }
                else
                    EditorGUILayout.HelpBox("This missile doesn't have a Rigidbody attached.", MessageType.Error);
                #endregion

                #region Fuel Stats
                if (controller.Burntime != 0)
                    InspectorHelper.RichHelpBox("The fuel in this missile will last for <b>~" + Math.Round(controller.Burntime, 2) + " seconds</b>.");
                else
                    EditorGUILayout.HelpBox("This missile does not use the fuel feature.", MessageType.None);
                #endregion
            }
            #endregion

            EditorUtility.SetDirty(controller);
        }
    }
}
#endif