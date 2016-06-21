/// <copyright file="RenderingHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using System;
using UnityEngine;
using UnityEditor;

namespace CodeControl.Editor {

    public class RenderingHelper {

        private static Texture2D dotTex = null;

        public static void RenderDotInMonitorWindow(Vector2 point, Color color, float width, float height) {
            RenderDot(point + CodeControlMonitorWindow.WindowOffset, color, width, height);
        }

        public static void RenderDot(Vector2 point, Color color, float width, float height) {
            InitializeDotTexture();

            GUI.color = color;
            GUI.DrawTexture(new Rect(point.x - width * .5f, point.y - height * .5f, width, height), dotTex);
            GUI.color = Color.white;
        }

        public static void RenderRectInMonitorWindow(Rect rect, Color color) {
            RenderRect(new Rect(rect.x + CodeControlMonitorWindow.WindowOffset.x, rect.y + CodeControlMonitorWindow.WindowOffset.y, rect.width, rect.height), color);
        }

        public static void RenderRect(Rect rect, Color color) {
            InitializeDotTexture();
            GUI.color = color;
            GUI.DrawTexture(rect, dotTex);
            GUI.color = Color.white;
        }

        public static void RenderRect(Rect rect, Color color, Rect mask) {
            InitializeDotTexture();

            if (rect.xMax < mask.xMin) { return; }
            if (rect.yMax < mask.yMin) { return; }
            if (rect.xMin > mask.xMax) { return; }
            if (rect.yMin > mask.yMax) { return; }

            if (rect.xMin < mask.xMin) {
                rect.width -= Mathf.Max(0.0f, mask.xMin - rect.xMin);
                rect.x = mask.xMin;
            }
            if (rect.yMin < mask.yMin) {
                rect.height -= Mathf.Max(0.0f, mask.yMin - rect.yMin);
                rect.y = mask.yMin;
            }
            if (rect.xMax > mask.xMax) {
                rect.width -= rect.xMax - mask.xMax;
            }
            if (rect.yMax > mask.yMax) {
                rect.height -= rect.yMax - mask.yMax;
            }

            GUI.color = color;
            GUI.DrawTexture(rect, dotTex);
            GUI.color = Color.white;
        }

        public static void RenderLineInMiniMap(Vector2 pointA, Vector2 pointB, Color color, int width) {
            color.a *= (CodeControlMonitorWindow.IsHoveringMiniMap ? 1.0f : CodeControlEditorStyles.MiniMapMouseOutAlpha);
            Vector2 miniMapPos = CodeControlMonitorWindow.MiniMapWindowRect.position + CodeControlMonitorWindow.WindowPadding * CodeControlMonitorWindow.MiniMapScaleFactor;
            float miniMapScale = CodeControlMonitorWindow.MiniMapScaleFactor;
            RenderLine(pointA * miniMapScale + miniMapPos, pointB * miniMapScale + miniMapPos, color, width);
        }

        public static void RenderLineInMonitorWindow(Vector2 pointA, Vector2 pointB, Color color, int width) {
            RenderLine(pointA + CodeControlMonitorWindow.WindowOffset, pointB + CodeControlMonitorWindow.WindowOffset, color, width);
        }

        public static void RenderLine(Vector2 pointA, Vector2 pointB, Color color, int width) {
            Handles.BeginGUI();
            Color savedColor = Handles.color;
            Handles.color = color;

            Vector3 startPos = new Vector3(pointA.x, pointA.y);
            Vector3 endPos = new Vector3(pointB.x, pointB.y);

            if (width == 1) {
                Handles.DrawLine(startPos, endPos);
            }else{
                Vector3 direction = (pointB - pointA).normalized;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward);

                Vector3 offsetStart = perpendicular * width * .5f;
                Vector3 offsetEnd = -perpendicular * width * .5f;

                for (float i = 0; i < width; i += .5f) {
                    Vector3 offset = Vector3.Lerp(offsetStart, offsetEnd, i / (float)width);
                    Vector2 totalStartPos = startPos + offset;
                    Vector2 totalEndPos = endPos + offset;
                    totalStartPos = new Vector2(Mathf.Round(totalStartPos.x), Mathf.Round(totalStartPos.y));
                    totalEndPos = new Vector2(Mathf.Round(totalEndPos.x), Mathf.Round(totalEndPos.y));
                    Handles.DrawLine(totalStartPos, totalEndPos);
                }
            }

            Handles.EndGUI();
            Handles.color = savedColor;
        }

        private static void InitializeDotTexture() {
            if (!dotTex) {
                dotTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                dotTex.hideFlags = HideFlags.DontSave;
                dotTex.SetPixel(0, 1, Color.white);
                dotTex.Apply();
            }
        }

        
    }

}