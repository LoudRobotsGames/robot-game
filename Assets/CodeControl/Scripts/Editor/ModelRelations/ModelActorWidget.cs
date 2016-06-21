/// <copyright file="ModelActorWidget.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using CodeControl.Internal;

namespace CodeControl.Editor {

    public class ModelActorWidget : ButtonWidget, IComparable<ModelActorWidget> {

        public bool HasChangeRelation;
        public bool HasDeleteRelation;

        public readonly Type ModelType;
        public readonly string TypeName;

        private const float dotMaxAge = 2.0f;
        private const float dotSpeed = 50.0f;

        private readonly ModelRelationsWidget modelRelationsWidget;

        private float age = 0.0f;
        private float lastChangeAge;
        private float lastDeleteAge;

        public ModelActorWidget(Type modelType, ModelRelationsWidget modelRelationsWidget) : base() {
            ModelType = modelType;
            this.modelRelationsWidget = modelRelationsWidget;
            TypeName = CodeControlEditorHelper.GetActualTypeName(modelType);
        }

        public override void Update() {
            age += CodeControlMonitorWindow.DeltaTime;
            base.Update();
        }

        public void RenderLine(Vector2 startPos, Vector2 endPos, bool thickLine) {
            Vector2 offset = Vector2.zero;
            if (HasChangeRelation && HasDeleteRelation) {
                offset = (endPos - startPos).GetPerpendicular().normalized * 2.0f;
            }

            int lineWidth = thickLine ? 2 : 1;

            if (HasChangeRelation) {
                RenderLineWithDots(startPos, endPos, -offset, new Color(0.0f, 1.0f, 1.0f), age - lastChangeAge, lineWidth);
            }
            if (HasDeleteRelation) {
                RenderLineWithDots(startPos, endPos, offset, new Color(1.0f, 0.0f, 0.0f), age - lastDeleteAge, lineWidth);
            }
        }

        public void RenderLineInMiniMap(Vector2 startPos, Vector2 endPos) {
            Vector2 offset = Vector2.zero;
            if (HasChangeRelation && HasDeleteRelation) {
                offset = (endPos - startPos).GetPerpendicular().normalized * 2.0f;
            }

            if (HasChangeRelation) {
                RenderingHelper.RenderLineInMiniMap(startPos - offset, endPos - offset, new Color(1.0f, 1.0f, 1.0f, .5f), 1);
            }
            if (HasDeleteRelation) {
                RenderingHelper.RenderLineInMiniMap(startPos + offset, endPos + offset, new Color(1.0f, 1.0f, 1.0f, .5f), 1);
            }
        }

        public void TriggerChangeRelation() {
            HasChangeRelation = true;
            lastChangeAge = age;
        }

        public void TriggerDeleteRelation() {
            HasDeleteRelation = true;
            lastDeleteAge = age;
        }

        public bool IsActualType(Type type) {
            return CodeControlEditorHelper.GetActualTypeName(type) == TypeName;
        }

        public int CompareTo(ModelActorWidget other) {
            if (!HasDeleteRelation && other.HasDeleteRelation) { return -1; }
            if (!HasChangeRelation && other.HasChangeRelation) { return 1; }
            return other.age > age ? 1 : -1;
        }

        protected override string GetText() {
            return TypeName;
        }

        protected override void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open " + TypeName + ".cs"), false, delegate() {
                CodeControlEditorHelper.OpenCodeOfType(ModelType);
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Hide"), false, delegate() {
                modelRelationsWidget.RemoveModelActorWidget(this);
            });

            menu.ShowAsContext();
        }

        private void RenderLineWithDots(Vector2 startPos, Vector2 endPos, Vector2 offset, Color color, float dotAge, int width) {
            Vector2 startWithOffset = startPos + offset;
            Vector2 endWithOffset = endPos + offset;
            RenderingHelper.RenderLineInMonitorWindow(startWithOffset, endWithOffset, color, width);
            RenderDots(startWithOffset, endWithOffset, color, dotAge);
        }

        private void RenderDots(Vector2 startPos, Vector2 endPos, Color color, float dotAge) {
            float distance = Vector2.Distance(startPos, endPos);
            if (dotAge <= dotMaxAge) {
                const int dotCount = 2;
                for (int i = 0; i < dotCount; i++) {
                    color.a = 1.0f - dotAge / dotMaxAge;
                    float lerp = ((age / (distance / dotSpeed) + (float)i / dotCount)) % 1;
                    color.a *= Mathf.Sin(lerp * Mathf.PI);
                    Vector2 position = Vector2.Lerp(startPos, endPos, lerp);
                    RenderingHelper.RenderDotInMonitorWindow(position, color, 6, 6);
                }
            }
        }
    }

}