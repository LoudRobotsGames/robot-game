/// <copyright file="CodeControlEditorHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CodeControl;
using System.IO;
using UnityEditor;
using CodeControl.Internal;

namespace CodeControl.Editor {

    public static class CodeControlEditorHelper {

        public static bool IsDerived(Type derivedClass, Type baseClass) {
            Type it = derivedClass;
            while (it.BaseType != null) {
                if (it.BaseType == baseClass) { return true; }
                it = it.BaseType;
            }
            return false;
        }

        public static List<Type> GetAllModelTypes() {
            List<Type> types = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in asm.GetTypes()) {
                    if (CodeControlEditorHelper.IsDerived(type, typeof(Model)) && type != typeof(ModelProxy)) {
                        types.Add(type);
                    }
                }
            }
            return types;
        }

        public static string GetActualTypeName(Type rawType) {
            return GetActualTypeName(rawType.ToString());
        }

        public static string GetActualTypeName(string rawTypeName) {
            char[] chars = new char[] { '`', '+' };
            if (rawTypeName.IndexOfAny(chars) != -1) {
                return rawTypeName.Split(chars)[0];
            } else {
                return rawTypeName;
            }
        }

        public static bool OpenCodeOfType(Type type) {
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);

            string typeName = type.ToString();

            // Make sure we don't include namespaces
            int lastDotPos = typeName.LastIndexOf(".");
            if (lastDotPos >= 0) {
                typeName = typeName.Substring(lastDotPos + 1, typeName.Length - 1 - lastDotPos);
            }

            typeName = GetActualTypeName(typeName);

            FileInfo[] fields = directory.GetFiles(typeName + ".cs", SearchOption.AllDirectories);

            for (int i = 0; i < fields.Length; i++) {
                FileInfo field = fields[i];
                if (field == null) { continue; }

                string filePath = field.FullName;
                filePath = filePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object)) as UnityEngine.Object;
                if (asset == null) { continue; }

                AssetDatabase.OpenAsset(asset);

                return true;
            }

            Debug.LogError("Could not find file of type '" + type + "'. Make sure the filename matches its type.");

            return false;
        }

    }

}