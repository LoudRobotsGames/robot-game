/// <copyright file="ModelTypeWidgetNode.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace CodeControl.Editor {

    public class ModelTypeWidgetNode {

        public float Height {
            get {
                float height = ModelTypeWidget.Height + marginVert;
                foreach (ModelTypeWidgetNode node in nodes) {
                    height += node.Height;
                }
                return height;
            }
        }

        public float Width {
            get {
                return marginHor * InnerLevelCount + ModelTypeWidget.Width;
            }
        }

        public string AggregationCountType {
            get {
                if (aggregationCount < 0) {
                    return "0.*";
                } else if (aggregationCount == 0) {
                    return "";
                } else {
                    return aggregationCount.ToString();
                }
            }
        }

        public int InnerLevelCount { 
            get {
                int maxLevelCount = 0;
                foreach (ModelTypeWidgetNode node in nodes) {
                    maxLevelCount = Mathf.Max(maxLevelCount, node.InnerLevelCount);
                }
                if (HasNodes) {
                    maxLevelCount++;
                }
                return maxLevelCount;
            } 
        }

        public int InnerNodeCount {
            get {
                int nodeCount = 1;
                foreach (ModelTypeWidgetNode node in nodes) {
                    nodeCount += node.InnerNodeCount;
                }
                return nodeCount;
            }
        }

        public bool HasNodes {
            get {
                return nodes.Count > 0;
            }
        }

        public ModelTypeWidget TypeWidget;
        private int aggregationCount;

        private const float marginVert = 5;
        private const float marginHor = 50;

        private List<ModelTypeWidgetNode> nodes;

        public ModelTypeWidgetNode(Type modelType) : this(modelType, 0, .5f * new Vector2(ModelTypeWidget.Width, ModelTypeWidget.Height), new List<Type>() { modelType }) { }

        private ModelTypeWidgetNode(Type modelType, int aggregationCount, Vector2 position, List<Type> previouslyReferencedTypes, bool stopRecursion = false) {
            TypeWidget = new ModelTypeWidget(modelType);
            this.aggregationCount = aggregationCount;
            TypeWidget.Position = position;
            nodes = new List<ModelTypeWidgetNode>();

            if (stopRecursion) { return; }

            // Make a list with all referenced types including aggregated types of this type
            List<Type> afterReferencedTypes = new List<Type>(previouslyReferencedTypes);
            foreach (AggregatedModelType aggregatedType in TypeWidget.AggregatedTypes) {
                if (afterReferencedTypes.Contains(aggregatedType.ModelType)) { continue; }
                afterReferencedTypes.Add(aggregatedType.ModelType);
            }

            // Construct ModelTypeWidgetNodes for each aggregated type
            foreach (AggregatedModelType aggregatedType in TypeWidget.AggregatedTypes) {
                bool stop = previouslyReferencedTypes.Contains(aggregatedType.ModelType);
                previouslyReferencedTypes.Add(aggregatedType.ModelType);
                ModelTypeWidgetNode node = new ModelTypeWidgetNode(aggregatedType.ModelType, aggregatedType.AggregationCount, position + new Vector2(marginHor, Height), afterReferencedTypes, stop);
                nodes.Add(node);
            }
        }

        public void SetPosition(Vector2 position) {
            TypeWidget.Position = position;
            float totalHeight = ModelTypeWidget.Height + marginVert;
            foreach (ModelTypeWidgetNode node in nodes) {
                node.SetPosition(position + new Vector2(marginHor, totalHeight));
                totalHeight += node.Height;
            }
        }

        public void Render() {
            Vector2 start = TypeWidget.Position - .25f * ModelTypeWidget.Width * Vector2.right + .5f * ModelTypeWidget.Height * Vector2.up;
            foreach (ModelTypeWidgetNode node in nodes) {
                Vector2 end = node.TypeWidget.Position - .5f * ModelTypeWidget.Width * Vector2.right;
                Vector2 mid = new Vector2(start.x, end.y);
                RenderingHelper.RenderLineInMonitorWindow(start, mid, Color.white, 1);
                RenderingHelper.RenderLineInMonitorWindow(mid, end, Color.white, 1);
                start = new Vector2(TypeWidget.Position.x - .25f * ModelTypeWidget.Width, node.TypeWidget.Position.y);
            }

            TypeWidget.Render();

            foreach (ModelTypeWidgetNode node in nodes) {
                Vector2 aggregationCountPosition = node.TypeWidget.Position - .70f * ModelTypeWidget.Width * Vector2.right + CodeControlMonitorWindow.WindowOffset;
                CodeControlEditorStyles.SetLabelStyle(CodeControlEditorStyles.LabelStyle.AggregationCountType);
                GUI.Label(new Rect(aggregationCountPosition.x - 100, aggregationCountPosition.y - 8, 100, 30), node.AggregationCountType);
                CodeControlEditorStyles.ResetLabelStyle();
                node.Render();
            }
        }

        public void RenderMiniMap() {
            Vector2 start = TypeWidget.Position - .25f * ModelTypeWidget.Width * Vector2.right + .5f * ModelTypeWidget.Height * Vector2.up;
            foreach (ModelTypeWidgetNode node in nodes) {
                Vector2 end = node.TypeWidget.Position - .5f * ModelTypeWidget.Width * Vector2.right;
                Vector2 mid = new Vector2(start.x, end.y);
                RenderingHelper.RenderLineInMiniMap(start, mid, new Color(1.0f, 1.0f, 1.0f, .3f), 1);
                RenderingHelper.RenderLineInMiniMap(mid, end, new Color(1.0f, 1.0f, 1.0f, .3f), 1);
                start = new Vector2(TypeWidget.Position.x - .25f * ModelTypeWidget.Width, node.TypeWidget.Position.y);
            }

            TypeWidget.RenderMiniMap();
            foreach (ModelTypeWidgetNode node in nodes) {
                node.RenderMiniMap();
            }
        }

        public List<TypeAggregation> GetAggregatedTypes() {
            List<TypeAggregation> types = new List<TypeAggregation>();
            foreach (ModelTypeWidgetNode node in nodes) {
                types.Add(new TypeAggregation() { Parent = TypeWidget.ModelType, Child = node.TypeWidget.ModelType });
                List<TypeAggregation> aggregatedTypes = node.GetAggregatedTypes();
                foreach (TypeAggregation aggregatedType in aggregatedTypes) {
                    types.Add(aggregatedType);
                }
            }
            return types;
        }

    }
}