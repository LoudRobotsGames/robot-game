/// <copyright file="CodeControlMonitorWindow.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using CodeControl;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeControl.Editor {

    public class CodeControlMonitorWindow : EditorWindow {

        public const string MonitorHelpUrl = "http://unitycodecontrol.com/monitors/";

        public static Vector2 WindowOffset { get; private set; }
        public static Vector2 WindowPadding { get { return new Vector2(20, 20); } }
        public static Rect WindowRect { get; private set; }
        public static Rect MiniMapWindowRect { get; private set; }
        public static float MiniMapScaleFactor { get; private set; }
        public static Vector2 MiniMapOffset { get { return WindowPadding * MiniMapScaleFactor; } }
        public static Vector2 MousePosition { get; private set; }
        public static float DeltaTime { get; private set; }
        public static bool MiniMapEnabled { get; private set; }
        public static bool IsHoveringMiniMap { get; private set; }

        private const int PanButton = 0;
        private const float ScrollSpeed = 4.0f;
        private const float TopBarHeight = 30;
        private const float TopBarMargin = 5;
        private const float WindowMargin = 5;
        private const string EditorPrefsSelectedWindow = "UCC Selected Window";

        private static bool ShowPeformance = false;

        private static CodeControlMonitorWindow instance;
        
        private static bool hasInit = false;

        private static List<IWindowRenderer> windowRenderers;

        private static IWindowRenderer currentWindowRenderer;

        private static bool isPanning = false;
        private static bool isPanningMiniMap = false;
        private static Vector2 panDragStart = Vector2.zero;
        
        private static Texture2D backgroundTexture;

        private static double previousTimeSinceStartup;
        
        [MenuItem("Window/Code Control Monitors")]
        private static void Init() {
            if (instance) { Deinit(); }
            hasInit = true;
            instance = EditorWindow.GetWindow(typeof(CodeControlMonitorWindow)) as CodeControlMonitorWindow;
            
            instance.titleContent = new GUIContent("Code Monitors");
            instance.minSize = new Vector2(320, 200);

            previousTimeSinceStartup = EditorApplication.timeSinceStartup;

            windowRenderers = new List<IWindowRenderer>();
            windowRenderers.Add(new MessageFlowRenderer());
            windowRenderers.Add(new ModelRelationsRenderer());
            windowRenderers.Add(new ModelStructuresRenderer());

            WindowOffset = WindowPadding;

            if (!backgroundTexture) {
                backgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                backgroundTexture.hideFlags = HideFlags.DontSave;
                backgroundTexture.SetPixel(0, 1, new Color(0.05f, 0.05f, 0.05f));
                backgroundTexture.Apply();
            }

            string selectedWindow = EditorPrefs.GetString(EditorPrefsSelectedWindow);
            currentWindowRenderer = windowRenderers.Find(x => x.GetType().ToString() == selectedWindow);
            if (currentWindowRenderer == null) { 
                currentWindowRenderer = windowRenderers[0];
                EditorPrefs.SetString(EditorPrefsSelectedWindow, currentWindowRenderer.GetType().ToString());
            }
        }

        private static void InitIfNeeded() {
            if (hasInit) { return; }
            Init();
        }

        private static void Deinit() {
            instance = null;
            if (windowRenderers == null) { return; }
            foreach (IWindowRenderer windowRenderer in windowRenderers) {
                windowRenderer.Deinit();
            }
        }

        private void OnDestroy() {
            Deinit();
        }

        private void Update() {
            Repaint();
        }

        private void OnGUI() {
            InitIfNeeded();

            DeltaTime = (float)(EditorApplication.timeSinceStartup - previousTimeSinceStartup);
            previousTimeSinceStartup = EditorApplication.timeSinceStartup;

            Stopwatch updateWatch = Stopwatch.StartNew();

            UpdateWindowRects();

            MousePosition = Event.current.mousePosition;

            UpdateIsHoveringMiniMap();

            HandlePanning();

            foreach (IWindowRenderer windowRenderer in windowRenderers) {
                windowRenderer.Update();
            }

            updateWatch.Stop();
            Stopwatch renderWatch = Stopwatch.StartNew();

            DrawTopBar();

            GUI.DrawTexture(WindowRect, backgroundTexture);

            ButtonWidget.Tooltip = "";

            BeginWindows();
            GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GUI.Window(0, WindowRect, RenderRelationsWindow, "");
            GUI.color = Color.white;
            EndWindows();

            if (!String.IsNullOrEmpty(ButtonWidget.Tooltip) && !IsHoveringMiniMap) {
                GUI.TextArea(new Rect(MousePosition.x + 15, MousePosition.y + 10, 80, 16), ButtonWidget.Tooltip);
            }

            renderWatch.Stop();

            CodeControlEditorStyles.ResetLabelStyle();

            if (ShowPeformance) {
                GUI.Label(new Rect(10, position.height - 40, position.width - 10, 30), "Update: " + updateWatch.ElapsedMilliseconds.ToString() + " ms");
                GUI.Label(new Rect(10, position.height - 25, position.width - 10, 30), "Render: " + renderWatch.ElapsedMilliseconds.ToString() + " ms");
            }
        }

        private void HandlePanning() {
            if (Event.current.type == EventType.MouseDown && Event.current.button == PanButton && WindowRect.Contains(MousePosition) && !IsHoveringMiniMap) {
                isPanning = true;
                panDragStart = MousePosition;
            }

            if (Event.current.rawType == EventType.MouseUp) {
                isPanning = false;
            }

            if (!isPanning && !isPanningMiniMap && Event.current.type == EventType.ScrollWheel && WindowRect.Contains(MousePosition)) {
                WindowOffset -= Vector2.up * Event.current.delta.y * ScrollSpeed;
            }

            if (isPanning) {
                WindowOffset += MousePosition - panDragStart;
                panDragStart = MousePosition;
            }else if (isPanningMiniMap) {
                WindowOffset = -((Event.current.mousePosition - WindowRect.position - WindowPadding * MiniMapScaleFactor) - MiniMapWindowRect.position) / MiniMapScaleFactor + WindowRect.size * .5f;
                KeepWindowOffsetWithinBoundaries(false);
            }else{
                KeepWindowOffsetWithinBoundaries(true);
            }
        }

        private void KeepWindowOffsetWithinBoundaries(bool soft) {
            Rect windowRect = WindowRect;

            Vector2 newWindowOffset = WindowOffset;

            bool overLeft = WindowOffset.x < currentWindowRenderer.BoundingBox.x + WindowPadding.x;
            bool overRight = WindowOffset.x + currentWindowRenderer.BoundingBox.width > windowRect.width - WindowPadding.x;
            bool overTop = WindowOffset.y < currentWindowRenderer.BoundingBox.y + WindowPadding.y;
            bool overBottom = WindowOffset.y + currentWindowRenderer.BoundingBox.height > windowRect.height - WindowPadding.y;

            bool insideBoundingBoxHor = currentWindowRenderer.BoundingBox.width > windowRect.width;
            bool insideBoundingBoxVert = currentWindowRenderer.BoundingBox.height > windowRect.height;

            if (overLeft && !overRight) {
                float endValue = insideBoundingBoxHor ? WindowRect.width - currentWindowRenderer.BoundingBox.width - WindowPadding.x : currentWindowRenderer.BoundingBox.x + WindowPadding.x;
                newWindowOffset.x = Mathf.Lerp(WindowOffset.x, endValue, soft ? 0.1f : 1.0f);
            }
            if (overRight && !overLeft) {
                float endValue = insideBoundingBoxHor ? currentWindowRenderer.BoundingBox.x + WindowPadding.x : WindowRect.width - currentWindowRenderer.BoundingBox.width - WindowPadding.x;
                newWindowOffset.x = Mathf.Lerp(WindowOffset.x, endValue, soft ? 0.1f : 1.0f);
            }

            if (overTop && !overBottom) {
                float endValue = insideBoundingBoxVert ? WindowRect.height - currentWindowRenderer.BoundingBox.height - WindowPadding.y : currentWindowRenderer.BoundingBox.y + WindowPadding.y;
                newWindowOffset.y = Mathf.Lerp(WindowOffset.y, endValue, soft ? 0.1f : 1.0f);
            }
            if (overBottom && !overTop) {
                float endValue = insideBoundingBoxVert ? currentWindowRenderer.BoundingBox.y + WindowPadding.y : WindowRect.height - currentWindowRenderer.BoundingBox.height - WindowPadding.y;
                newWindowOffset.y = Mathf.Lerp(WindowOffset.y, endValue, soft ? 0.1f : 1.0f);
            }

            WindowOffset = newWindowOffset;
        }

        private void DrawTopBar() {
            GUILayout.BeginHorizontal();

            foreach (IWindowRenderer windowRenderer in windowRenderers) {
                DrawRelationWindowSelectionButton(windowRenderer.Title, windowRenderer);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawRelationWindowSelectionButton(string buttonText, IWindowRenderer windowRenderer) {
            if (currentWindowRenderer == windowRenderer) { 
                GUI.color = CodeControlEditorStyles.MainColor;
            } else {
                GUI.color = Color.white;
            }

            if (GUILayout.Button(buttonText, GUILayout.Height(TopBarHeight))) {
                if (Event.current.button == 1) {
                    windowRenderer.ShowContextMenu();
                } else {
                    WindowOffset = WindowPadding;
                    currentWindowRenderer = windowRenderer;
                    EditorPrefs.SetString(EditorPrefsSelectedWindow, currentWindowRenderer.GetType().ToString());
                }
            }
            GUI.color = Color.white;
        }

        private void RenderRelationsWindow(int id) {
            Rect windowRect = WindowRect;
            MiniMapEnabled = windowRect.size.x < currentWindowRenderer.BoundingBox.size.x || windowRect.size.y < currentWindowRenderer.BoundingBox.size.y;
            if (MiniMapEnabled) {
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                isPanningMiniMap = GUI.RepeatButton(MiniMapWindowRect, "");
                GUI.color = Color.white;
            }

            currentWindowRenderer.Render();

            if (MiniMapEnabled) {
                RenderingHelper.RenderRect(MiniMapWindowRect, new Color(.5f, .5f, .5f, .3f * (CodeControlMonitorWindow.IsHoveringMiniMap ? 1.0f : CodeControlEditorStyles.MiniMapMouseOutAlpha)));

                currentWindowRenderer.RenderMiniMap();

                RenderingHelper.RenderRect(new Rect(MiniMapWindowRect.x - (WindowOffset.x - WindowPadding.x) * MiniMapScaleFactor,
                                          MiniMapWindowRect.y - (WindowOffset.y - WindowPadding.y) * MiniMapScaleFactor, 
                                          windowRect.width * MiniMapScaleFactor,
                                          windowRect.height * MiniMapScaleFactor), new Color(1.0f, 1.0f, 1.0f, .2f * (CodeControlMonitorWindow.IsHoveringMiniMap ? 1.0f : CodeControlEditorStyles.MiniMapMouseOutAlpha)), MiniMapWindowRect);
            } else {
                isPanningMiniMap = false;
            }
        }

        private void UpdateWindowRects() {
            WindowRect = new Rect(WindowMargin, TopBarHeight + TopBarMargin, position.width - WindowMargin * 2, position.height - (TopBarHeight + TopBarMargin) - WindowMargin);

            Rect boundingBox = currentWindowRenderer.BoundingBox;
            const float maxSize = 150;
            Vector2 size = new Vector2();
            if (boundingBox.width > boundingBox.height) {
                size.x = maxSize;
                size.y = maxSize / boundingBox.width * boundingBox.height;
                MiniMapScaleFactor = maxSize / (boundingBox.width + WindowPadding.x * 2);
            } else {
                size.y = maxSize;
                size.x = maxSize / boundingBox.height * boundingBox.width;
                MiniMapScaleFactor = maxSize / (boundingBox.height + WindowPadding.y * 2);
            }
            MiniMapWindowRect = new Rect(WindowRect.width - WindowMargin - size.x, WindowRect.height - WindowMargin - size.y, size.x, size.y);
        }

        private void UpdateIsHoveringMiniMap() {
            IsHoveringMiniMap = MiniMapEnabled && MiniMapWindowRect.Contains(MousePosition - WindowRect.position);
        }

    }

}