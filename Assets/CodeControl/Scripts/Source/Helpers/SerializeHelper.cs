/// <copyright file="SerializeHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System;

namespace CodeControl.Internal {

    public static class SerializeHelper {

        public static string XmlSerializeToString(this object objectInstance) {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb)) {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(this string objectData) {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(this string objectData, Type type) {
            try {
                var serializer = new XmlSerializer(type);
                object result;

                using (TextReader reader = new StringReader(objectData)) {
                    result = serializer.Deserialize(reader);
                }

                return result;
            } catch {
                return null;
            }
        }

    }

}