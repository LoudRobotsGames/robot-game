/// <copyright file="ModelTypeWidget.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using CodeControl;
using CodeControl.Internal;
using UnityEditor;

namespace CodeControl.Editor {

    public class AggregatedModelType {

        public Type ModelType;

        /// <summary>-1 means variable count</summary>
        public int AggregationCount;

    }

    public class ModelTypeWidget : ButtonWidget {

        public Type ModelType { get; private set; }
        public List<AggregatedModelType> AggregatedTypes { get; private set; }

        public ModelTypeWidget(Type modelType) : base() {
            ModelType = modelType;
            AggregatedTypes = FindAggregatedTypes();
        }

        protected override void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open " + ModelType.ToString() + ".cs"), false, delegate() {
                CodeControlEditorHelper.OpenCodeOfType(ModelType);
            });

            menu.ShowAsContext();
        }

        protected override string GetText() {
            return ModelType.Name;
        }

        /// <summary>
        /// Checks this model for aggregated model references in form of ModelRef or ModelRefs
        /// </summary>
        /// <returns></returns>
        private List<AggregatedModelType> FindAggregatedTypes() {
            List<AggregatedModelType> aggregatedTypes = new List<AggregatedModelType>();
            FieldInfo[] fields = ModelType.GetFields();
            foreach (FieldInfo field in fields) {
                if (field.FieldType.IsSubclassOf(typeof(ModelReferencer))) {
                    int count = -1;

                    // Check if single ref
                    if (field.FieldType.GetGenericTypeDefinition() == typeof(ModelRef<>)) {
                        AggregatedModelType aggregatedType = aggregatedTypes.Find(x => x.ModelType == field.FieldType.GetGenericArguments()[0] && x.AggregationCount > 0);
                        if (aggregatedType != null) {
                            aggregatedType.AggregationCount++;
                            continue;
                        } else {
                            count = 1;
                        }
                    }

                    aggregatedTypes.Add(new AggregatedModelType() {
                        ModelType = field.FieldType.GetGenericArguments()[0],
                        AggregationCount = count
                    });
                }                
            }
            return aggregatedTypes;
        }

    }

}