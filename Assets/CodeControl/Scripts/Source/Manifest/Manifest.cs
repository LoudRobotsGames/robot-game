/// <copyright file="Manifest.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using CodeControl;

namespace CodeControl.Internal {

    [Serializable]
    public class Manifest {

        public List<ModelEntry> Entries;

        private delegate Model LoadModelDelegate(ModelEntry entry);

        private static Manifest instance;

        public static string Save(List<Model> models) {
            Manifest manifest = GetManifest(models);
            var serializer = new XmlSerializer(manifest.GetType());
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, manifest);
            return writer.ToString();
        }

        public static void Save(string dir, List<Model> models) {
            Directory.CreateDirectory(dir);

            DeleteOldSave(dir);

            Manifest manifest = GetManifest(models);
            var serializer = new XmlSerializer(manifest.GetType());
            var stream = new FileStream(dir + "/manifest.xml", FileMode.Create);
            serializer.Serialize(stream, manifest);
            stream.Close();
        }

        public static void LoadAndConstruct<T>(ModelBlobs modelBlobs, Action<float> onProgress, Action<T> onDone, Action<string> onError) where T : Model {
            if (!modelBlobs.ContainsKey("manifest")) {
                if (onError != null) { onError("Failed to load modelBlobs as it does not contain a manifest."); }
                return;
            }

            instance = modelBlobs["manifest"].XmlDeserializeFromString<Manifest>();
            if (instance == null) {
                if (onError != null) { onError("Failed to deserialize manifest."); }
                return;
            }

            LoadModelDelegate loadModelCallback = delegate(ModelEntry entry) {
                return modelBlobs[entry.Id].XmlDeserializeFromString(TypeHelper.GetGlobalType(entry.Type)) as Model;
            };

            Coroutiner.Start(ConstructAsync<T>(loadModelCallback, onProgress, onDone, onError));
        }

        public static void LoadAndConstruct<T>(string dir, Action<float> onProgress, Action<T> onDone, Action<string> onError) where T : Model {
            try {
                var serializer = new XmlSerializer(typeof(Manifest));
                var stream = new FileStream(dir + "/manifest.xml", FileMode.Open);
                instance = serializer.Deserialize(stream) as Manifest;
                stream.Close();
            } catch {
                if (onError != null) { onError("Failed to deserialize manifest."); }
                return;
            }

            LoadModelDelegate loadModelCallback = delegate(ModelEntry entry) {
                try {
                    Type type = TypeHelper.GetGlobalType(entry.Type);
                    var entrySerializer = new XmlSerializer(type);
                    var entryStream = new FileStream(dir + "/" + entry.Id + ".xml", FileMode.Open);
                    Model model = entrySerializer.Deserialize(entryStream) as Model;
                    entryStream.Close();
                    return model;
                } catch {
                    return null;
                }
            };

            Coroutiner.Start(ConstructAsync<T>(loadModelCallback, onProgress, onDone, onError));
        }

        private static IEnumerator ConstructAsync<T>(LoadModelDelegate loadModelCallback, Action<float> onProgress, Action<T> onDone, Action<string> onError) where T : Model {
            int i = 0;
            float startTime = Time.realtimeSinceStartup;
            T rootModel = null;
            while (i < instance.Entries.Count) {
                Model model = loadModelCallback(instance.Entries[i]);
                if (model == null) {
                    if (onError != null) { onError("Failed to deserialize type '" + instance.Entries[i].Type + "' with id '" + instance.Entries[i].Id + "'"); }
                    yield break;
                }
                if (model is T) {
                    rootModel = model as T;
                }
                i++;
                if (Time.realtimeSinceStartup - startTime > Time.smoothDeltaTime) {
                    if (onProgress != null) {
                        onProgress((float)i / (float)instance.Entries.Count);
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
            if (onDone != null) {
                onDone(rootModel);
            }
        }

        private static Manifest GetManifest(List<Model> models){
            Manifest manifest = GetInstance();
            manifest.Entries = new List<ModelEntry>();
            foreach (Model model in models) {
                ModelEntry entry = new ModelEntry(model.Id, model.GetType());
                manifest.Entries.Add(entry);
            }
            return manifest;
        }

        private static Manifest GetInstance() {
            if (instance == null) {
                instance = new Manifest();
            }
            return instance;
        }

        private static void DeleteOldSave(string dir) {
#if (!UNITY_WEBPLAYER)
            DirectoryInfo directory = new DirectoryInfo(dir);
            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files) {
                file.Delete();
            }
#endif
        }
    }

}