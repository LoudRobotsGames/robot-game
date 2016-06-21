/// <copyright file="ModelStructuresRenderer.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace CodeControl.Editor {

    public class ModelStructuresRenderer : IWindowRenderer {

        public Rect BoundingBox { get { return boundingBox; } }
        public string Title { get { return "Model Structures"; } }

        private const float marginVert = 20;

        private Rect boundingBox;
        private List<ModelTypeWidgetNode> rootNodes;

        public ModelStructuresRenderer() {
            rootNodes = new List<ModelTypeWidgetNode>();

            // Create a rootNode for all types
            List<Type> types = CodeControlEditorHelper.GetAllModelTypes();
            List<TypeAggregation> takenTypes = new List<TypeAggregation>();
            foreach (Type type in types) {
                ModelTypeWidgetNode node = new ModelTypeWidgetNode(type);
                rootNodes.Add(node);

                List<TypeAggregation> aggregatedTypes = node.GetAggregatedTypes();
                foreach (TypeAggregation aggregatedType in aggregatedTypes) {
                    takenTypes.Add(aggregatedType);
                }
            }
            
            // Remove rootNodes that are aggregated in other rootNodes
            while (true) {
                bool rootNodeRemoved = false;
                for (int i = rootNodes.Count - 1; i >= 0; i--) {
                    List<TypeAggregation> takenInTypes = takenTypes.FindAll(x => x.Child == rootNodes[i].TypeWidget.ModelType);
                    foreach (TypeAggregation aggregation in takenInTypes) {

                        // Check if the parent of this rootNode would remain in other rootNodes if this would be removed
                        bool parentWillRemain = rootNodes.Find(x => x.TypeWidget.ModelType == aggregation.Parent) != null;
                        if (!parentWillRemain) {
                            foreach (ModelTypeWidgetNode rootNode in rootNodes) {
                                if (rootNode == rootNodes[i]) { continue; }
                                List<TypeAggregation> aggregatedTypes = rootNode.GetAggregatedTypes();
                                foreach (TypeAggregation aggregatedType in aggregatedTypes) {
                                    if (aggregatedType.Child == aggregation.Parent) {
                                        parentWillRemain = true;
                                        break;
                                    }
                                }
                                if (parentWillRemain) {
                                    break;
                                }
                            }
                        }

                        if (parentWillRemain) {
                            rootNodes.RemoveAt(i);
                            i = rootNodes.Count - 1;
                            rootNodeRemoved = true;
                            break;
                        }
                    }
                }
                if (!rootNodeRemoved) {
                    break;
                }
            }

            // Position rootNodes
            float maxWidth = 0;
            float totalHeight = 0;
            foreach (ModelTypeWidgetNode rootNode in rootNodes) {
                rootNode.SetPosition(new Vector2(ModelTypeWidget.Width * .5f, ModelTypeWidget.Height * .5f + totalHeight));
                maxWidth = Mathf.Max(maxWidth, rootNode.Width);
                totalHeight += rootNode.Height + marginVert;
            }
            boundingBox = new Rect(0, 0, maxWidth, totalHeight);
        }

        public void Deinit() {

        }

        public void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("What is this?"), false, delegate() {
                Application.OpenURL(CodeControlMonitorWindow.MonitorHelpUrl);
            });

            menu.ShowAsContext();
        }

        public void Update() {
            
        }

        public void Render() {
            if (rootNodes.Count > 0) {
                foreach (ModelTypeWidgetNode rootNode in rootNodes) {
                    rootNode.Render();
                }
            } else {
                CodeControlEditorStyles.SetLabelStyle(CodeControlEditorStyles.LabelStyle.WarningMessage);
                GUI.color = CodeControlEditorStyles.NoContentColor;
                GUI.Label(new Rect(18, 15, 300, 100), "No Models Found.");
                CodeControlEditorStyles.ResetLabelStyle();
            }
        }

        public void RenderMiniMap() {
            foreach (ModelTypeWidgetNode rootNode in rootNodes) {
                rootNode.RenderMiniMap();
            }
        }

    }

}