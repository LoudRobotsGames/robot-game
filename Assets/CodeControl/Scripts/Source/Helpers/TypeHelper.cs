/// <copyright file="TypeHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;


namespace CodeControl.Internal {

    public static class TypeHelper {

        private static Dictionary<string, Type> types = new Dictionary<string,Type>();

        public static Type GetGlobalType(string typeName) {
            if (types.ContainsKey(typeName)) { return types[typeName]; }
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                Type type = Type.GetType(typeName + "," + assembly.GetName());
                if (type != null) {
                    types.Add(typeName, type);
                    return type;
                }
            }
            return null;
        }

    }

}