/// <copyright file="ModelRefs.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using CodeControl.Internal;

namespace CodeControl {

    /// <summary>
    /// A list of references to models. Automatically removes models from the list when they are manually deleted.
    /// </summary>
    /// <typeparam name="T">The Model type that will be referenced</typeparam>
    [Serializable]
    public sealed class ModelRefs<T> : ModelReferencer, IEnumerable<T>, IXmlSerializable
        where T : Model {

        public T this[int i] {
            get { return models[i]; }
            set { models[i] = value; }
        }

        /// <summary>
        /// The number of instances in the list.
        /// </summary>
        public int Count { get { return models.Count; } }

        /// <summary>
        /// The last model instance in the list. Returns null if the list is empty. 
        /// </summary>
        public T Last { get { return models.Count == 0 ? null : models[models.Count - 1]; } }


        private List<T> models;
        private List<string> ids;

        public ModelRefs() {
            models = new List<T>();
        }

        /// <summary>
        /// Clears and removes the references to the models, potentially deleting models if they have no ModelRef(s) left referencing them.
        /// </summary>
        public void Clear() {
            for (int i = models.Count - 1; i >= 0; i--) {
                models[i].RemoveDeleteListener(OnModelDelete);
                models[i].DecreaseRefCount();
            }
            models.Clear();
        }

        /// <summary>
        /// Checks if the list of references contains a reference to the given model.
        /// </summary>
        /// <param name="model">The model that will be checked.</param>
        /// <returns>True if a reference to the given model is found.</returns>
        public bool Contains(T model) { return models.Contains(model); }

        /// <summary>
        /// Adds a model to the list of references.
        /// </summary>
        /// <param name="model">The model that is added to the list of references.</param>
        public void Add(T model) {
            models.Add(model);
            model.IncreaseRefCount();
            model.AddDeleteListener(OnModelDelete);
        }

        /// <summary>
        /// Removes the reference to the given model from the list of references.
        /// </summary>
        /// <param name="model">The model which reference will be removed from the list.</param>
        public void Remove(T model) {
            model.RemoveDeleteListener(OnModelDelete);
            model.DecreaseRefCount();
            models.Remove(model);
        }

        /// <summary>
        /// Removes the reference to the model at the given index, potentially deleting the model if it has no ModelRef(s) left referencing it.
        /// </summary>
        /// <param name="index">The index at which the model reference will be removed.</param>
        public void RemoveAt(int index) {
            if (index >= models.Count) {
                Debug.LogError("Can't remove because '" + index + "' is out of index");
                return;
            }
            models[index].RemoveDeleteListener(OnModelDelete);
            models[index].DecreaseRefCount();
            models.RemoveAt(index);
        }

        /// <summary>
        /// Clears and removes the references to the models, potentially deleting models if they have no ModelRef(s) left referencing them.
        /// </summary>
        public override void Delete() {
            Clear();
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < models.Count; i++) {
                yield return models[i];
            }
        }

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            ids = new List<string>();

            reader.MoveToContent();
            if (reader.IsEmptyElement) { return; }

            reader.ReadStartElement();

            while (reader.NodeType != XmlNodeType.EndElement) {
                string id = reader.ReadInnerXml();
                ids.Add(id);
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer) {
            foreach (Model model in models) {
                writer.WriteElementString("Id", model.Id);
            }
        }

        internal override void CollectReferences() {
            Clear();
            if (ids == null) { return; }
            foreach (string id in ids) {
                T model = (T)Model.Find(id);
                if (model == null) {
                    Debug.LogError("Could not collect reference of '" + typeof(T) + "' with id '" + id + "'");
                    continue;
                }
                Add(model);
            }
        }

        internal override List<Model> GetReferences() {
            List<Model> references = new List<Model>();
            foreach (T model in models) {
                references.Add(model);
                references.AddList<Model>(model.GetReferences());
            }
            references.Distinct<Model>();
            return references;
        }

        private void OnModelDelete(Model model) {
            models.Remove((T)model);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

}