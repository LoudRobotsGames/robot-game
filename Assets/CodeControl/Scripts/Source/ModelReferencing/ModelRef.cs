/// <copyright file="ModelRef.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using CodeControl.Internal;

namespace CodeControl {

    /// <summary>
    /// A reference to a single model.
    /// </summary>
    /// <typeparam name="T">The model type that will be referenced</typeparam>
    [Serializable]
    public sealed class ModelRef<T> : ModelReferencer, IXmlSerializable
        where T : Model {

        /// <summary>
        /// The referenced model. Changing this will potentially delete the old model if it has no ModelRef(s) left referencing it.
        /// </summary>
        public T Model {
            get {
                return model;
            }
            set {
                if (model == value) { return; }
                if (model != null) {
                    model.RemoveDeleteListener(OnModelDelete);
                    model.DecreaseRefCount();
                }
                model = value;
                if (model != null) {
                    model.IncreaseRefCount();
                    model.AddDeleteListener(OnModelDelete);
                }
            }
        }

        private T model;
        private string id;

        public ModelRef() { }

        /// <summary>
        /// Creates a new instance of ModelRef, referencing the given model.
        /// </summary>
        /// <param name="model">The model that will be referenced.</param>
        public ModelRef(T model) {
            Model = model;
        }

        /// <summary>
        /// Removes the reference to the model, potentially deleting the model if it has no ModelRef(s) left referencing it.
        /// </summary>
        public override void Delete() {
            Model = null;
        }

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            if (reader.IsEmptyElement) { return; }
            reader.Read();
            id = reader.ReadElementString("Id");
        }

        public void WriteXml(XmlWriter writer) {
            if (model != null) {
                writer.WriteElementString("Id", model.Id);
            }
        }

        internal override void CollectReferences() {
            if (string.IsNullOrEmpty(id)) { return; }
            Model = ModelProxy.Find(id) as T;
        }

        internal override List<Model> GetReferences() {
            if (model != null) {
                List<Model> references = model.GetReferences();
                if (!references.Contains(model)) {
                    references.Add(model);
                }
                return references;
            } else {
                return new List<Model>();
            }
        }

        private void OnModelDelete() {
            model = null;
        }

    }

}