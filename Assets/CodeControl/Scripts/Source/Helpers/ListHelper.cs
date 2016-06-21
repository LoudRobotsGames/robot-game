/// <copyright file="ListHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CodeControl.Internal {

    public static class ListHelper {

        public static void Distinct<T>(this List<T> list) {
            for (int i = list.Count - 1; i >= 0; i--) {
                T item = list[i];
                list.RemoveAll(x => EqualityComparer<T>.Default.Equals(x, item));
                list.Add(item);
                i = Mathf.Min(i, list.Count);
            }
        }

        public static void AddList<T>(this List<T> original, List<T> addition) {
            foreach (T item in addition) {
                original.Add(item);
            }
        }

    }

}