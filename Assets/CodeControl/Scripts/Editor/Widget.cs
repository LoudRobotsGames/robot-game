/// <copyright file="Widget.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl.Editor {

    public class Widget {

        public virtual Vector2 Position { get; set; }
        public Vector2 TargetPosition {
            get {
                return targetPosition;
            }
            set {
                targetPosition = value;
                if (!isPositioned) {
                    isPositioned = true;
                    Position = targetPosition;
                }
            }
        }

        private Vector2 targetPosition;
        private bool isPositioned;

        public virtual void Update() {
            Position = Vector2.Lerp(Position, TargetPosition, .05f);
        }

    }

}