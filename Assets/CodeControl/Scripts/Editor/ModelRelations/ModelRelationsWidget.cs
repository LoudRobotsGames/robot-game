/// <copyright file="ModelRelationsWidget.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using CodeControl.Internal;

namespace CodeControl.Editor {

    public class ModelRelationsWidget : ButtonWidget {

        public const float WidgetMarginHor = 150.0f;
        public const float WidgetMarginVert = 35.0f;

        public readonly Type ModelType;

        public float RelationsWidth {
            get {
                return ButtonWidget.Width + WidgetMarginHor * 2;
            }
        }

        public float RelationsHeight {
            get {
                int count = Mathf.Max(affectors.Count, handlers.Count);
                return (count-1) * WidgetMarginVert + ButtonWidget.Height;
            }
        }
        private const float lineInTake = 3.0f;

        private readonly string typeName;
        private readonly ModelRelationsRenderer modelRelationsRenderer;

        private List<ModelActorWidget> affectors = new List<ModelActorWidget>();
        private List<ModelActorWidget> handlers = new List<ModelActorWidget>();

        public ModelRelationsWidget(Type modelType, ModelRelationsRenderer modelRelationsRenderer) : base() {
            ModelType = modelType;
            this.modelRelationsRenderer = modelRelationsRenderer;
            typeName = CodeControlEditorHelper.GetActualTypeName(modelType);
        }

        public void SetPosition(Vector2 position) {
            TargetPosition = position + Vector2.right * (ButtonWidget.Width * .5f + WidgetMarginHor);
            RepositionActors();
        }

        public void HideBasedOnSettings(){
            HideActorsBasedOnSettings(affectors);
            HideActorsBasedOnSettings(handlers);
            if (affectors.Count == 0 && handlers.Count == 0) {
                modelRelationsRenderer.RemoveModelRelationsWidget(this);
            } else {
                RepositionActors();
            }
        }

        public void AddChanger(Type type) {
            GetOrCreateActor(type, affectors).TriggerChangeRelation();
        }

        public void AddDeleter(Type type) {
            GetOrCreateActor(type, affectors).TriggerDeleteRelation();
        }

        public void AddChangeHandler(Type type) {
            GetOrCreateActor(type, handlers).TriggerChangeRelation();
        }

        public void AddDeleteHandler(Type type) {
            GetOrCreateActor(type, handlers).TriggerDeleteRelation();
        }

        public void RemoveModelActorWidget(ModelActorWidget actor) {
            affectors.Remove(actor);
            handlers.Remove(actor);
            RepositionActors();
        }

        public override void Update() {
            base.Update();
            foreach (ModelActorWidget actor in affectors) {
                actor.Update();
            }
            foreach (ModelActorWidget actor in handlers) {
                actor.Update();
            }
        }

        public override void Render() {
            bool thisIsHovered = IsHovered;
            foreach (ModelActorWidget actor in affectors) {
                actor.RenderLine(GetAffectorLineStart(actor), GetAffectorLineEnd(), actor.IsHovered || thisIsHovered);
                actor.Render();
            }
            foreach (ModelActorWidget actor in handlers) {
                actor.RenderLine(GetHandlerLineStart(), GetHandlerLineEnd(actor), actor.IsHovered || thisIsHovered);
                actor.Render();
            }

            base.Render();
        }

        public override void RenderMiniMap() {
            foreach (ModelActorWidget actor in affectors) {
                actor.RenderLineInMiniMap(GetAffectorLineStart(actor), GetAffectorLineEnd());
                actor.RenderMiniMap();
            }
            foreach (ModelActorWidget actor in handlers) {
                actor.RenderLineInMiniMap(GetHandlerLineStart(), GetHandlerLineEnd(actor));
                actor.RenderMiniMap();
            }

            base.RenderMiniMap();
        }

        protected override string GetText() {
            return typeName;
        }

        protected override void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open " + typeName + ".cs"), false, delegate() {
                CodeControlEditorHelper.OpenCodeOfType(ModelType);
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Hide"), false, delegate() {
                modelRelationsRenderer.RemoveModelRelationsWidget(this);
            });

            menu.ShowAsContext();
        }

        private void RepositionActors() {
            affectors.Sort();
            handlers.Sort();
            for (int i = 0; i < affectors.Count; i++) {
                ModelActorWidget actor = affectors[i];
                actor.TargetPosition = TargetPosition + Vector2.right * -WidgetMarginHor + Vector2.up * ((-(affectors.Count - 1) * .5f + i) * WidgetMarginVert);
            }
            for (int i = 0; i < handlers.Count; i++) {
                ModelActorWidget actor = handlers[i];
                actor.TargetPosition = TargetPosition + Vector2.right * WidgetMarginHor + Vector2.up * ((-(handlers.Count - 1) * .5f + i) * WidgetMarginVert);
            }
        }

        private ModelActorWidget GetOrCreateActor(Type type, List<ModelActorWidget> actorList) {
            ModelActorWidget actor = actorList.Find(x => x.IsActualType(type));
            if (actor != null) {
                return actor;
            }
            actor = new ModelActorWidget(type, this);
            actorList.Add(actor);
            return actor;
        }

        private void HideActorsBasedOnSettings(List<ModelActorWidget> actorList) {
            for (int i = actorList.Count - 1; i >= 0; i--) {
                if ((!modelRelationsRenderer.ShowChanges && !actorList[i].HasDeleteRelation) ||
                    (!modelRelationsRenderer.ShowDeletions && !actorList[i].HasChangeRelation)) {
                    actorList.RemoveAt(i);
                    continue;
                }
                if (modelRelationsRenderer.ShowChanges == false) {
                    actorList[i].HasChangeRelation = false;
                }
                if (modelRelationsRenderer.ShowDeletions == false) {
                    actorList[i].HasDeleteRelation = false;
                }
            }
        }

        private Vector2 GetAffectorLineStart(ModelActorWidget affector) {
            return affector.Position + Vector2.right * (ModelActorWidget.Width * .5f - lineInTake);
        }

        private Vector2 GetAffectorLineEnd() {
            return Position - Vector2.right * (ModelActorWidget.Width * .5f - lineInTake);
        }

        private Vector2 GetHandlerLineStart() {
            return Position + Vector2.right * (ModelActorWidget.Width * .5f - lineInTake);
        }

        private Vector2 GetHandlerLineEnd(ModelActorWidget handler) {
            return handler.Position - Vector2.right * (ModelActorWidget.Width * .5f - lineInTake);
        }

    }

}