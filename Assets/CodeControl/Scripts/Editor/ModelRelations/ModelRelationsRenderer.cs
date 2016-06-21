/// <copyright file="ModelRelationsRenderer.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace CodeControl.Editor {

    public class ModelRelationsRenderer : IWindowRenderer {

        public bool ShowChanges { get { return EditorPrefs.HasKey(showChangesPrefKey) ? EditorPrefs.GetBool(showChangesPrefKey) : true; } }
        public bool ShowDeletions { get { return EditorPrefs.HasKey(showDeletionsPrefKey) ? EditorPrefs.GetBool(showDeletionsPrefKey) : true; } }

        public Rect BoundingBox {
            get {
                return new Rect(0.0f, 0.0f, cachedWidth, cachedHeight);
            }
        }

        public string Title {
            get {
                return "Model Relations";
            }
        }

        private const float labelHeight = 45.0f;
        private const float widgetMarginVert = 30.0f;

        private const string showChangesPrefKey = "UCC Model Relations Show Changes";
        private const string showDeletionsPrefKey = "UCC Model Relations Show Deletions";

        private float cachedWidth = 0.0f;
        private float cachedHeight = 0.0f;
        private List<ModelRelationsWidget> modelRelationsWidgets = new List<ModelRelationsWidget>();

        public ModelRelationsRenderer() {
            Model.OnModelChangeNotified += OnModelChangeNotified;
            Model.OnModelChangeHandled += OnModelChangeHandled;
            Model.OnModelDeleted += OnModelDeleted;
            Model.OnModelDeleteHandled += OnModelDeleteHandled;
        }

        public void Deinit() {
            Model.OnModelChangeNotified -= OnModelChangeNotified;
            Model.OnModelChangeHandled -= OnModelChangeHandled;
            Model.OnModelDeleted -= OnModelDeleted;
            Model.OnModelDeleteHandled -= OnModelDeleteHandled;
        }

        public void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("What is this?"), false, delegate() {
                Application.OpenURL(CodeControlMonitorWindow.MonitorHelpUrl);
            });

            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Show Changes"), ShowChanges, delegate() {
                EditorPrefs.SetBool(showChangesPrefKey, !ShowChanges);
                if (!ShowChanges) { EditorPrefs.SetBool(showDeletionsPrefKey, true); }
                HideRelationWidgetsBasedOnSettings();
            });

            menu.AddItem(new GUIContent("Show Deletions"), ShowDeletions, delegate() {
                EditorPrefs.SetBool(showDeletionsPrefKey, !ShowDeletions);
                if (!ShowDeletions) { EditorPrefs.SetBool(showChangesPrefKey, true); }
                HideRelationWidgetsBasedOnSettings();
            });

            menu.ShowAsContext();
        }

        public void Update() {
            foreach (ModelRelationsWidget widget in modelRelationsWidgets) {
                widget.Update();
            }
        }

        public void Render() {
            if (modelRelationsWidgets.Count > 0) {
                CodeControlEditorStyles.SetLabelStyle(CodeControlEditorStyles.LabelStyle.ModelRelationsTitle);
                GUI.Label(new Rect(CodeControlMonitorWindow.WindowOffset.x, CodeControlMonitorWindow.WindowOffset.y, ModelRelationsWidget.Width, 30), "Actor");
                GUI.Label(new Rect(CodeControlMonitorWindow.WindowOffset.x + ModelRelationsWidget.WidgetMarginHor, CodeControlMonitorWindow.WindowOffset.y, ModelRelationsWidget.Width, 30), "Model");
                GUI.Label(new Rect(CodeControlMonitorWindow.WindowOffset.x + 2.0f * ModelRelationsWidget.WidgetMarginHor, CodeControlMonitorWindow.WindowOffset.y, ModelRelationsWidget.Width, 30), "Handler");
                CodeControlEditorStyles.ResetLabelStyle();

                foreach (ModelRelationsWidget widget in modelRelationsWidgets) {
                    widget.Render();
                }
            } else {
                CodeControlEditorStyles.SetLabelStyle(CodeControlEditorStyles.LabelStyle.WarningMessage);
                GUI.color = CodeControlEditorStyles.NoContentColor;
                GUI.Label(new Rect(18, 15, 300, 100), "No Model Changes Notified.");
                CodeControlEditorStyles.ResetLabelStyle();
            }
        }

        public void RenderMiniMap() {
            foreach (ModelRelationsWidget widget in modelRelationsWidgets) {
                widget.RenderMiniMap();
            }
        }

        public void RemoveModelRelationsWidget(ModelRelationsWidget widget) {
            modelRelationsWidgets.Remove(widget);
            RepositionModelRelationsWidgets();
        }

        public void RepositionModelRelationsWidgets() {
            float totalHeight = labelHeight;
            foreach (ModelRelationsWidget widget in modelRelationsWidgets) {
                cachedWidth = Mathf.Max(cachedWidth, widget.RelationsWidth);
                totalHeight += widget.RelationsHeight * .5f;
                widget.SetPosition(Vector2.up * totalHeight);
                totalHeight += widget.RelationsHeight * .5f + widgetMarginVert;
            }
            cachedHeight = totalHeight - widgetMarginVert;
        }

        private void OnModelChangeNotified(Type notifierType, Type modelType) {
            if (!ShowChanges) { return; }
            ModelRelationsWidget widget = GetOrCreateModelRelationWidget(modelType);
            widget.AddChanger(notifierType);
            RepositionModelRelationsWidgets();
        }

        private void OnModelChangeHandled(Type handlerType, Type modelType) {
            if (!ShowChanges) { return; }
            ModelRelationsWidget widget = GetOrCreateModelRelationWidget(modelType);
            widget.AddChangeHandler(handlerType);
            RepositionModelRelationsWidgets();
        }

        private void OnModelDeleted(Type deleterType, Type modelType) {
            if (!ShowDeletions) { return; }
            ModelRelationsWidget widget = GetOrCreateModelRelationWidget(modelType);
            widget.AddDeleter(deleterType);
            RepositionModelRelationsWidgets();
        }

        private void OnModelDeleteHandled(Type handlerType, Type modelType) {
            if (!ShowDeletions) { return; }
            ModelRelationsWidget widget = GetOrCreateModelRelationWidget(modelType);
            widget.AddDeleteHandler(handlerType);
            RepositionModelRelationsWidgets();
        }

        private ModelRelationsWidget GetOrCreateModelRelationWidget(Type type) {
            ModelRelationsWidget widget = modelRelationsWidgets.Find(x => x.ModelType == type);
            if (widget != null) {
                return widget;
            }
            widget = new ModelRelationsWidget(type, this);
            modelRelationsWidgets.Add(widget);
            return widget;
        }

        private void HideRelationWidgetsBasedOnSettings() {
            for (int i = modelRelationsWidgets.Count - 1; i >= 0; i--) {
                modelRelationsWidgets[i].HideBasedOnSettings();
            }
        }

    }

}