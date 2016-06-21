/// <copyright file="ModelEntry.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;

namespace CodeControl.Internal {

    [Serializable]
    public class ModelEntry {

        public string Id { get; private set; }
        public string Type { get; private set; }

        public ModelEntry() { }

        public ModelEntry(string id, Type type) {
            Id = id;
            Type = type.ToString();
        }

    }

}