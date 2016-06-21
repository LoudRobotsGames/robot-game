/// <copyright file="Controller.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl {

    /// <summary>
    /// A controller is responsible for controlling its model based on user input. It will get destroyed automatically on deletion of its model.
    /// </summary>
    public static class Controller {

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by an empty gameobject.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(Model model) where T : AbstractController {
            return Instantiate<T>(model, null);
        }

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by an empty gameobject.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <param name="parent">The transform that the instantiated controller will be parented to.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(Model model, Transform parent) where T : AbstractController {
            if (model == null) {
                Debug.LogError("Can't instantiate controller '" + typeof(T) + "' because the given model is null.");
                return null;
            }

            GameObject gameObject = new GameObject(typeof(T).ToString());
            gameObject.transform.parent = parent;
            gameObject.transform.localPosition = Vector3.zero;

            T controller = gameObject.AddComponent<T>();
            controller.Initialize(model);

            return controller;
        }

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by a prefab located at the given resource path.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="resourcePath">The path in resources to the prefab that will embody the controller.</param>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(string resourcePath, Model model) where T : AbstractController {
            return Instantiate<T>(resourcePath, model, null);
        }

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by a prefab located at the given resource path.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="resourcePath">The path in resources to the prefab that will embody the controller.</param>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <param name="parent">The transform that the instantiated controller will be parented to.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(string resourcePath, Model model, Transform parent) where T : AbstractController {
            Object resource = Resources.Load(resourcePath);
            if (resource == null) {
                Debug.LogError("Can't instantiate controller '" + typeof(T) + "' because resource at '" + resourcePath + "' could not be found.");
                return null;
            }
            return Instantiate<T>(resource, model, parent);
        }

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by the given resource as gameobject.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="resource">The path in resources to the prefab that will embody the controller.</param>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(Object resource, Model model) where T : AbstractController {
            return Instantiate<T>(resource, model, null);
        }

        /// <summary>
        /// Instantiates a new instance of the given controller type, embodied by the given resource as gameobject.
        /// </summary>
        /// <typeparam name="T">The controller type that will be instantiated.</typeparam>
        /// <param name="resource">The path in resources to the prefab that will embody the controller.</param>
        /// <param name="model">The model that will be linked to the controller. The controller will be destroyed on the deletion of this model.</param>
        /// <param name="parent">The transform that the instantiated controller will be parented to.</param>
        /// <returns>A new instance of the given controller type.</returns>
        public static T Instantiate<T>(Object resource, Model model, Transform parent) where T : AbstractController {
            if (resource == null) {
                Debug.LogError("Can't instantiate controller '" + typeof(T) + "' because the given resource is null.");
                return null;
            }

            if (model == null) {
                Debug.LogError("Can't instantiate controller '" + typeof(T) + "' because the given model is null.");
                return null;
            }

            GameObject gameObject = GameObject.Instantiate(resource) as GameObject;
            T controller = gameObject.GetComponent<T>();
            if (controller == null) {
                Debug.LogError("Can't instantiate controller '" + typeof(T) + "' because the controller component on the given object is missing.");
                return null;
            }

            controller.transform.parent = parent;
            controller.transform.localPosition = Vector3.zero;

            controller.Initialize(model);
            return controller;
        }
    }

    /// <summary>
    /// An abstraction of the Controller, used as parameter type in instantiation methods.
    /// </summary>
    public abstract class AbstractController : MonoBehaviour {
        internal abstract void Initialize(Model model);
    }

    /// <summary>
    /// A controller is responsible for handeling its model based on user input. It will get destroyed on the deletion of its model.
    /// </summary>
    /// <typeparam name="T">The type of model that this controller will control.</typeparam>
    public abstract class Controller<T> : AbstractController
        where T : Model {

        /// <summary>
        /// The Model assigned to the Controller. A Controller will be destroyed automatically on the deletion of this Model.
        /// </summary>
        protected T model { get; private set; }

        internal override void Initialize(Model model) {
            if (model.GetType() != typeof(T)) {
                Debug.LogError("Failed to initialize controller '" + GetType() + "' with model type '" + model.GetType() + "'");
                return;
            }
            this.model = model as T;
            model.AddChangeListener(OnModelChange);
            model.AddDeleteListener(OnModelDelete);
            OnInitialize();
        }

        /// <summary>
        /// Called after the model has been set.
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Called on the model's OnChange callback.
        /// </summary>
        protected virtual void OnModelChanged() { }

        protected virtual void OnDestroy() {
            if (model != null) {
                model.RemoveChangeListener(OnModelChange);
                model.RemoveDeleteListener(OnModelDelete);
            }
        }

        private void OnModelChange() {
            OnModelChanged();
        }

        private void OnModelDelete() {
            model.RemoveDeleteListener(OnModelDelete);
            GameObject.DestroyImmediate(gameObject);
        }
    }

}