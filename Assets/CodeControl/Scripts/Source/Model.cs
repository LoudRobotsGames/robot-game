/// <copyright file="Model.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;
using CodeControl.Internal;
using System.Diagnostics;

namespace CodeControl {

    /// <copyright file="Model.cs">Copyright (c) 2015 All Rights Reserved</copyright>
    /// <author>Joris van Leeuwen</author>
    /// <date>01/27/2014</date>
    /// <summary>A Model is a representation of data, which can be saved and loaded into/from different data types. It will delete itself automatically when there is no ModelRef(s) left referencing it. Extend the Model into a custom class to add data fields that can be saved/loaded. Use Models in combination with Controllers for max use.</summary>
    [Serializable]
    public abstract class Model : ModelReferencer {

        /// <summary>
        /// Called when the NotifyChange is called on a Model. Only works in Editor.
        /// </summary>
        /// <param name="affecterType">The type that called the NotifyChange on the Model.</param>
        /// <param name="modelType">The type of Model on which the NotifyChange is called.</param>
        public delegate void OnModelAffectDelegate(Type affecterType, Type modelType);

        /// <summary>Called when a model change is notified. Only works when in the Unity editor.</summary>
        public static OnModelAffectDelegate OnModelChangeNotified;

        /// <summary>Called when a model change is notified. Only works when in the Unity editor.</summary>
        public static OnModelAffectDelegate OnModelChangeHandled;

        /// <summary>Called when a model is deleted. Only works when in the Unity editor.</summary>
        public static OnModelAffectDelegate OnModelDeleted;

        /// <summary>Called when a model deletion is handled. Only works when in the Unity editor.</summary>
        public static OnModelAffectDelegate OnModelDeleteHandled;

        /// <summary>
        /// The id of this model, which can be used to find it later on.
        /// </summary>
        public string Id {
            get {
                return id;
            }
            private set {
                Unregister();
                id = value;
                Register();
            }
        }

        private static Dictionary<string, Model> sortedInstances = new Dictionary<string, Model>();
        private static Dictionary<Type, List<Model>> typeSortedInstances = new Dictionary<Type, List<Model>>();
        private static List<Model> instances = new List<Model>();

        private static bool isSerializing;

        private string id;
        private bool isRegistered;
        private bool referencesCollected;
        private int refCount;
        private List<Delegate> onChangeHandlers;
        private List<Delegate> onDeleteHandlers;

        /// <summary>
        /// Saves all Models into ModelBlobs.
        /// </summary>
        /// <returns>All Models in form of ModelBlobs, xml formatted strings order by id.</returns>
        public static ModelBlobs SaveAll() {
            return Save(instances, false);
        }

        /// <summary>
        /// Saves the given model and all Models referenced by the given model into ModelBlobs.
        /// </summary>
        /// <param name="rootModel">The model that will be saved along with the models it has references to.</param>
        /// <returns>All models in form of modelBlobs, xml formatted strings order by id.</returns>
        public static ModelBlobs Save(Model rootModel) {
            if (rootModel == null) {
                UnityEngine.Debug.LogError("Can't save Model if the given value is null!");
                return null;
            }
            List<Model> models = new List<Model>();
            if (rootModel != null) {
                models.Add(rootModel);
                models.AddList<Model>(rootModel.GetReferences());
            }
            return Save(models, false);
        }

        /// <summary>
        /// Saves the given models and potentially all models referenced by the given models into modelBlobs.
        /// </summary>
        /// <param name="models">The models that will be saved, potentially along with the models they have references to.</param>
        /// <param name="saveReferenced">Whether the given models should also save their referenced models.</param>
        /// <returns>All models in form of modelBlobs, xml formatted strings order by id.</returns>
        public static ModelBlobs Save(List<Model> models, bool saveReferenced) {
            if (models == null) {
                UnityEngine.Debug.LogError("Can't save Models if the given list is null");
                return null;
            }
            models.Distinct<Model>();
            if (saveReferenced) {
                foreach (Model model in models) {
                    models.AddList<Model>(model.GetReferences());
                }
            }
            models.Distinct<Model>();
            ModelBlobs modelBlobs = new ModelBlobs();
            modelBlobs.Add("manifest", Manifest.Save(models));
            isSerializing = true;
            foreach (Model model in models) {
                modelBlobs.Add(model.Id, model.XmlSerializeToString());
            }
            isSerializing = false;
            return modelBlobs;
        }

        /// <summary>
        /// Saves all models into separate xml files in the given directory.
        /// </summary>
        /// <param name="dir">Target save directory.</param>
        public static void SaveAll(string dir) {
            Save(dir, instances, false);
        }

        /// <summary>
        /// Saves the given model and all models referenced by the given model into separate xml files in the given directory.
        /// </summary>
        /// <param name="dir">Target save directory.</param>
        /// <param name="model">The model that will be saved along with the models it has references to.</param>
        public static void Save(string dir, Model model) {
            if (model == null) {
                UnityEngine.Debug.LogError("Can't save Model if the given value is null!");
                return;
            }
            List<Model> models = model.GetReferences();
            models.Add(model);
            Save(dir, models, false);
        }

        /// <summary>
        /// Saves the given models and potentially all models referenced by the given models into separate xml files in the given directory.
        /// </summary>
        /// <param name="dir">Target save directory.</param>
        /// <param name="models">The models that will be saved, potentially along with the models they have references to.</param>
        /// <param name="saveReferenced">Whether the given models should also save their referenced models.</param>
        public static void Save(string dir, List<Model> models, bool saveReferenced) {
            if (string.IsNullOrEmpty(dir)) {
                UnityEngine.Debug.LogError("Can't save Models if the given directory name is empty!");
                return;
            }
            if (models == null) {
                UnityEngine.Debug.LogError("Can't save Models if the given list is null");
                return;
            }
            models.Distinct<Model>();
            if (saveReferenced) {
                foreach (Model model in models) {
                    models.AddList<Model>(model.GetReferences());
                }
            }
            models.Distinct<Model>();
            Manifest.Save(dir, models);
            isSerializing = true;
            foreach (Model model in models) {
                var serializer = new XmlSerializer(model.GetType());
                var stream = new FileStream(dir + "/" + model.Id + ".xml", FileMode.Create);
                serializer.Serialize(stream, model);
                stream.Close();
            }
            isSerializing = false;
        }

        /// <summary>
        /// Reads the given ModelBlobs and constructs models accordingly.
        /// </summary>
        /// <param name="data">The modelBlobs that will be read.</param>
        /// <param name="onStart">Called when the loading process starts.</param>
        /// <param name="onProgress">Called every frame during the loading process. Outputs the progression on a scale of 0.0f to 1.0f.</param>
        /// <param name="onDone">Called when the loading process is done.</param>
        /// <param name="onError">Called when the an error occurs during the loading process. Outputs the reason of the error.</param>
        public static void Load(ModelBlobs data, Action onStart, Action<float> onProgress, Action onDone, Action<string> onError) {
            Load<Model>(data, onStart, onProgress, delegate(Model model) { if (onDone != null) { onDone(); } }, onError);
        }

        /// <summary>
        /// Reads the given ModelBlobs and constructs models accordingly. Returns the loaded root Model of type T in the onDone callback.
        /// </summary>
        /// <typeparam name="T">The type of instance that will be outputted in the onDone callback.</typeparam>
        /// <param name="data">The modelBlobs that will be read.</param>
        /// <param name="onStart">Called when the loading process starts.</param>
        /// <param name="onProgress">Called every frame during the loading process. Outputs the progression on a scale of 0.0f to 1.0f.</param>
        /// <param name="onDone">Called when the loading process is done, outputting the loaded instance of the given type.</param>
        /// <param name="onError">Called when the an error occurs during the loading process. Outputs the reason of the error.</param>
        public static void Load<T>(ModelBlobs data, Action onStart, Action<float> onProgress, Action<T> onDone, Action<string> onError) where T : Model {
            if (onStart != null) { onStart(); }
            if (data == null) {
                if (onError != null) { onError("Failed to load modelBlobs because it is null."); }
                return;
            }
            Manifest.LoadAndConstruct<T>(data, onProgress, delegate(T rootModel) {
                foreach (Model model in instances) {
                    model.CollectReferences();
                }
                if (onDone != null) { onDone(rootModel); }
            }, onError);
        }

        /// <summary>
        /// Loads the xml files from the given directory and constructs models accordingly.
        /// </summary>
        /// <param name="dir">Target load directory.</param>
        /// <param name="onStart">Called when the loading process starts.</param>
        /// <param name="onProgress">Called every frame during the loading process. Outputs the progression on a scale of 0.0f to 1.0f.</param>
        /// <param name="onDone">Called when the loading process is done.</param>
        /// <param name="onError">Called when the an error occurs during the loading process. Outputs the reason of the error.</param>
        public static void Load(string dir, Action onStart, Action<float> onProgress, Action onDone, Action<string> onError) {
            Load<Model>(dir, onStart, onProgress, delegate(Model model) { if (onDone != null) { onDone(); } }, onError);
        }

        /// <summary>
        /// Loads the xml files from the given directory and constructs models accordingly.
        /// </summary>
        /// <typeparam name="T">The type of instance that will be outputted in the onDone callback.</typeparam>
        /// <param name="dir">Target load directory.</param>
        /// <param name="onStart">Called when the loading process starts.</param>
        /// <param name="onProgress">Called every frame during the loading process. Outputs the progression on a scale of 0.0f to 1.0f.</param>
        /// <param name="onDone">Called when the loading process is done, outputting the loaded instance of the given type.</param>
        /// <param name="onError">Called when the an error occurs during the loading process. Outputs the reason of the error.</param>
        public static void Load<T>(string dir, Action onStart, Action<float> onProgress, Action<T> onDone, Action<string> onError) where T : Model {
            if (onStart != null) { onStart(); }
            if (string.IsNullOrEmpty(dir)) {
                if (onError != null) { onError("Failed to load models because the given directory is null or empty."); }
                return;
            }
            if (!Directory.Exists(dir)) {
                if (onError != null) { onError("Failed to load models because the given directory does not exist."); }
                return;
            }
            Manifest.LoadAndConstruct<T>(dir, onProgress, delegate(T rootModel) {
                foreach (Model model in instances) {
                    model.CollectReferences();
                }
                if (onDone != null) { onDone(rootModel); }
            }, onError);
        }

        /// <summary>
        /// Finds and returns the model with the given id.
        /// </summary>
        /// <param name="id">The id used to find the model.</param>
        /// <returns>The model found with the given id.</returns>
        public static Model Find(string id) {
            if (!sortedInstances.ContainsKey(id)) {
                UnityEngine.Debug.LogError("Could not find model with id '" + id + "'");
                return null;
            }
            return sortedInstances[id];
        }

        /// <summary>
        /// Finds and returns the model of given type with the given id.
        /// </summary>
        /// <typeparam name="T">The type used to find the model.</typeparam>
        /// <param name="id">The id used to find the model.</param>
        /// <returns>The model found with the given type and id.</returns>
        public static Model Find<T>(string id) where T : Model {
            Model model = Find(id);
            if (model.GetType() != typeof(T)) {
                UnityEngine.Debug.LogError("Could not find model with id '" + id + "' and type '" + typeof(T) + "'");
                return null;
            }
            return model;
        }

        /// <summary>
        /// Finds and returns an instance of the given model type.
        /// </summary>
        /// <typeparam name="T">The type used to find the instance.</typeparam>
        /// <returns>The model found with the given type.</returns>
        public static T First<T>() where T : Model {
            Type type = typeof(T);
            if (!typeSortedInstances.ContainsKey(type)) { return null; }
            if (typeSortedInstances[type].Count == 0) { return null; }
            return typeSortedInstances[type][0] as T;
        }

        /// <summary>
        /// Returns all models.
        /// </summary>
        /// <returns>All models.</returns>
        public static List<Model> GetAll() {
            return instances;
        }

        /// <summary>
        /// Finds and returns all models of the given type.
        /// </summary>
        /// <typeparam name="T">The type used to find the models.</typeparam>
        /// <returns>All models of given type.</returns>
        public static List<T> GetAll<T>() where T : Model {
            List<T> models = new List<T>();
            Type type = typeof(T);
            if (!typeSortedInstances.ContainsKey(type)) { return models; }
            foreach (Model model in typeSortedInstances[type]) {
                models.Add((T)model);
            }
            return models;
        }

        /// <summary>
        /// Deletes all models.
        /// </summary>
        public static void DeleteAll() {
            while (instances.Count > 0) {
                instances[0].Delete();
            }
        }

        /// <summary>
        /// Deletes all models of given type.
        /// </summary>
        /// <typeparam name="T">The type used to find and delete the models.</typeparam>
        public static void DeleteAll<T>() where T : Model {
            List<T> models = GetAll<T>();
            while (models.Count > 0) {
                if (sortedInstances.ContainsKey(models[0].Id)) {
                    models[0].Delete();
                }
                models.RemoveAt(0);
            }
        }

        public Model() {
            Id = Guid.NewGuid().ToString();
            refCount = 0;
            onChangeHandlers = new List<Delegate>();
            onDeleteHandlers = new List<Delegate>();
        }

        /// <summary>
        /// Adds a listener that triggers the given callback when the NotifyChange is called on this Model.
        /// </summary>
        /// <param name="callback">The callback that will be triggered when NotifyChange is called.</param>
        public void AddChangeListener(Action callback) {
            if (callback == null) {
                UnityEngine.Debug.LogError("Failed to add ChangeListener on Model but the given callback is null!");
                return;
            }
            onChangeHandlers.Add(callback);
        }

        /// <summary>
        /// Adds a listener that triggers the given callback when the NotifyChange is called on this Model.
        /// </summary>
        /// <param name="callback">The callback that will be triggered when NotifyChange is called.</param>
        public void AddChangeListener(Action<Model> callback) {
            if (callback == null) {
                UnityEngine.Debug.LogError("Failed to add ChangeListener on Model but the given callback is null!");
                return;
            }
            onChangeHandlers.Add(callback);
        }

        /// <summary>
        /// Removes a listener that would trigger the given callback when the NotifyChange is called on this Model.
        /// </summary>
        /// <param name="callback">The callback that is triggered when the NotifyChange is called.</param>
        public void RemoveChangeListener(Action callback) {
            onChangeHandlers.Remove(callback);
        }

        /// <summary>
        /// Removes a listener that triggers the given callback when the NotifyChange is called on this Model
        /// </summary>
        /// <param name="callback">The callback that is triggered when the NotifyChange is called.</param>
        public void RemoveChangeListener(Action<Model> callback) {
            onChangeHandlers.Remove(callback);
        }

        /// <summary>
        /// Adds a listener that triggers the given callback when this Model is deleted.
        /// </summary>
        /// <param name="callback">The callback that will be triggered when NotifyChange is called.</param>
        public void AddDeleteListener(Action callback) {
            if (callback == null) {
                UnityEngine.Debug.LogError("Failed to add DeleteListener on Model but the given callback is null!");
                return;
            }
            onDeleteHandlers.Add(callback);
        }

        /// <summary>
        /// Adds a listener that triggers the given callback when this Model is deleted.
        /// </summary>
        /// <param name="callback">The callback that will be triggered when NotifyChange is called.</param>
        public void AddDeleteListener(Action<Model> callback) {
            if (callback == null) {
                UnityEngine.Debug.LogError("Failed to add DeleteListener on Model but the given callback is null!");
                return;
            }
            onDeleteHandlers.Add(callback);
        }

        /// <summary>
        /// Removes a listener that triggers the given callback when this Model is deleted.
        /// </summary>
        /// <param name="callback">The callback that triggers when NotifyChange is called.</param>
        public void RemoveDeleteListener(Action callback) {
            onDeleteHandlers.Remove(callback);
        }

        /// <summary>
        /// Removes a listener that triggers the given callback when this Model is deleted.
        /// </summary>
        /// <param name="callback">The callback that triggers when NotifyChange is called.</param>
        public void RemoveDeleteListener(Action<Model> callback) {
            onDeleteHandlers.Remove(callback);
        }

        /// <summary>
        /// Sends out callbacks to this Model's change listeners.
        /// </summary>
        public void NotifyChange() {
            if (Application.isEditor && OnModelChangeNotified != null) {
                StackTrace stackTrace = new StackTrace();
                Type notifierType = stackTrace.GetFrame(1).GetMethod().DeclaringType;
                OnModelChangeNotified(notifierType, GetType());
            }

            List<Delegate> callbacks = new List<Delegate>(onChangeHandlers);
            while (callbacks.Count > 0) {
                Delegate callback = callbacks[0];
                if (Application.isEditor && OnModelChangeHandled != null) {
                    OnModelChangeHandled(callback.Target.GetType(), GetType());
                }
                CallbackModelDelegate(callback);
                callbacks.Remove(callback);
            }
        }

        /// <summary>
        /// Deletes this Model, removing it from ModelRefs lists and destroying its linked Controllers.
        /// </summary>
        public override void Delete() {
            if (!sortedInstances.ContainsKey(id)) {
                return;
            }

            if (Application.isEditor && OnModelDeleted != null) {
                StackTrace stackTrace = new StackTrace();
                Type deleterType = stackTrace.GetFrame(1).GetMethod().DeclaringType;
                OnModelDeleted(deleterType, GetType());
            }

            while (onDeleteHandlers.Count > 0) {
                Delegate callback = onDeleteHandlers[0];
                if (Application.isEditor && OnModelDeleteHandled != null) {
                    OnModelDeleteHandled(callback.Target.GetType(), GetType());
                }
                CallbackModelDelegate(callback);
                onDeleteHandlers.Remove(callback);
            }

            Unregister();

            List<ModelReferencer> modelReferencers = GetModelReferencersInFields();
            foreach (ModelReferencer referencer in modelReferencers) {
                if (referencer == null) { continue; }
                referencer.Delete();
            }
        }

        internal override List<Model> GetReferences() {
            List<Model> references = new List<Model>();
            List<ModelReferencer> referencers = GetModelReferencersInFields();
            foreach (ModelReferencer referencer in referencers) {
                references.AddList<Model>(referencer.GetReferences());
            }
            references.Distinct<Model>();
            return references;
        }

        internal override void CollectReferences() {
            if (referencesCollected) { return; }
            referencesCollected = true;
            List<ModelReferencer> referencers = GetModelReferencersInFields();
            foreach (ModelReferencer referencer in referencers) {
                referencer.CollectReferences();
            }
        }

        internal void IncreaseRefCount() {
            refCount++;
        }

        internal void DecreaseRefCount() {
            refCount--;
            if (refCount <= 0) {
                Delete();
            }
        }

        private List<ModelReferencer> GetModelReferencersInFields() {
            FieldInfo[] fields = GetType().GetFields();
            List<ModelReferencer> modelReferencers = new List<ModelReferencer>();
            foreach (FieldInfo field in fields) {
                if (field.GetValue(this) is ModelReferencer) {
                    modelReferencers.Add(field.GetValue(this) as ModelReferencer);
                }
            }
            return modelReferencers;
        }

        private void Register() {
            if (isSerializing) { return; }
            if (isRegistered) { return; }
            isRegistered = true;

            if (!sortedInstances.ContainsKey(id)) {
                sortedInstances.Add(id, this);
            }

            if (!typeSortedInstances.ContainsKey(GetType())) {
                typeSortedInstances.Add(GetType(), new List<Model>());
            }
            typeSortedInstances[GetType()].Add(this);

            instances.Add(this);
        }

        private void Unregister() {
            if (!isRegistered) { return; }
            isRegistered = false;
            if (sortedInstances.ContainsValue(this)) {
                foreach (KeyValuePair<string, Model> pair in sortedInstances) {
                    if (pair.Value == this) {
                        sortedInstances.Remove(pair.Key);
                        break;
                    }
                }
            }
            if (typeSortedInstances.ContainsKey(GetType())) {
                typeSortedInstances[GetType()].Remove(this);
            }
            instances.Remove(this);
            if (!string.IsNullOrEmpty(id)) {
                sortedInstances.Remove(id);
            }
            instances.Remove(this);
        }

        private void CallbackModelDelegate(Delegate callback) {
            if (callback is Action<Model>) {
                Action<Model> action = callback as Action<Model>;
                action(this);
            } else {
                Action action = callback as Action;
                action();
            }
        }

    }

}