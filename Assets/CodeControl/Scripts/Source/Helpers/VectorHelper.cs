/// <copyright file="VectorHelper.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl.Internal {

    public static class VectorHelper {

        public static Vector2 GetPerpendicular(this Vector2 vector) {
            Vector3 v3 = new Vector3(vector.x, 0.0f, vector.y);
            Vector3 perp = Vector3.Cross(v3, Vector3.up);
            return new Vector2(perp.x, perp.z);
        }

    }

}