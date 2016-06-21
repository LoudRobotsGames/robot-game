/// <copyright file="ModelBlobs.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeControl.Internal;
using System;

namespace CodeControl {

    /// <summary>
    /// A xml formatted string dictionary ordered by id. Used to store and load a collection of Models.
    /// </summary>
    public class ModelBlobs : Dictionary<string, string> {

        /// <summary>
        /// Converts a single string into a ModelBlobs object, using the '~' char as a seperator
        /// </summary>
        /// <param name="data">The string to convert into a ModelBlobs object.</param>
        /// <returns>A ModelBlobs object based on the given string.</returns>
        public static ModelBlobs FromString(string data) {
            return FromString(data, '~');
        }

        /// <summary>
        /// Converts a single string into a ModelBlobs object.
        /// </summary>
        /// <param name="data">The string to convert into a ModelBlobs object.</param>
        /// <param name="seperator">The seperator to split the string.</param>
        /// <returns>A ModelBlobs object based on the given string.</returns>
        public static ModelBlobs FromString(string data, char seperator) {
            if (string.IsNullOrEmpty(data)) {
                Debug.LogError("Can't split string into ModelBlobs if the given string is empty or null!");
                return null;
            }
            return FromStringArray(data.Split(seperator));
        }

        /// <summary>
        /// Converts a single string into a ModelBlobs object. Use a char as seperator for higher performance.
        /// </summary>
        /// <param name="data">The string to convert into a ModelBlobs object.</param>
        /// <param name="seperator">The seperator to split the string.</param>
        /// <returns>A ModelBlobs object based on the given string.</returns>
        public static ModelBlobs FromString(string data, string seperator) {
            if (string.IsNullOrEmpty(data)) {
                Debug.LogError("Can't split string into ModelBlobs if the given string is empty or null!");
                return null;
            }
            if (string.IsNullOrEmpty(seperator)) {
                Debug.LogError("Can't split string into ModelBlobs if the given seperator is empty or null!");
                return null;
            }
            return FromStringArray(Regex.Split(data, seperator));
        }

        /// <summary>
        /// Converts the ModelBlobs object into a single string, using the '~' char as a seperator
        /// </summary>
        /// <returns>A single string containing the ModelBlobs.</returns>
        public override string ToString() {
            return ToString('~');
        }

        /// <summary>
        /// Converts the ModelBlobs object into a single string.
        /// </summary>
        /// <param name="seperator">The seperator used to glue the ModelBlobs into a single string.</param>
        /// <returns>A single string containing the ModelBlobs.</returns>
        public string ToString(char seperator) {
            return ToString(seperator.ToString());
        }

        /// <summary>
        /// Converts the ModelBlobs object into a single string. Use a char as seperator for higher performance.
        /// </summary>
        /// <param name="seperator">The seperator used to glue the ModelBlobs into a single string.</param>
        /// <returns>A single string containing the ModelBlobs.</returns>
        public string ToString(string seperator) {
            if (string.IsNullOrEmpty(seperator)) {
                Debug.LogError("Can't join ModelBlobs if the given seperator is empty or null!");
                return null;
            }
            if (this.Count == 0) {
                Debug.LogError("Can't join ModelBlobs because there are none!");
                return null;
            }
            List<string> dataList = new List<string>();
            foreach (KeyValuePair<string, string> pair in this) {
                dataList.Add(pair.Key);
                dataList.Add(pair.Value);
            }
            string[] dataArray = dataList.ToArray();
            return string.Join(seperator, dataArray);
        }

        private static ModelBlobs FromStringArray(string[] splittedData) {
            ModelBlobs modelBlobs = new ModelBlobs();
            string id = null;
            foreach (string blob in splittedData) {
                if (string.IsNullOrEmpty(id)) {
                    id = blob;
                } else {
                    modelBlobs.Add(id, blob);
                    id = null;
                }
            }
            return modelBlobs;
        }

    }

}