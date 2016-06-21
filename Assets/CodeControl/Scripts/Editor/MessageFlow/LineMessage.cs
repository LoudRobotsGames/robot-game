/// <copyright file="LineMessage.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using CodeControl;

namespace CodeControl.Editor {

    public class LineMessage {

        public const float MessageButtonWidth = 150;
        public const float MessageButtonHeight = 15;

        public const float MessageButtonWidthSmall = 15;
        public const float MessageButtonHeightSmall = 15;

        public bool IsDone {
            get {
                return age > maxMessageAge;
            }
        }

        public float AgeFactor {
            get {
                return age / maxMessageAge;
            }
        }

        public bool IsReversed { get; private set; }

        private const float maxMessageAge = 4.0f;

        private string name;
        private float age;
        private bool isTypeless;

        public LineMessage(Type messageType, string name, bool reversed) {
            this.name = name;
            IsReversed = reversed;
            isTypeless = messageType == typeof(Message);
        }

        public void Update() {
            age += CodeControlMonitorWindow.DeltaTime;
        }

        public void Render(Vector2 position, bool renderName) {
            Color messageColor = isTypeless ? CodeControlEditorStyles.LineMessageColorTypeless : CodeControlEditorStyles.LineMessageColor;
            messageColor.a *= Mathf.Min(1.0f, 4.0f - 4.0f * age / maxMessageAge);
            GUI.color = messageColor;
            GUIStyle messageButtonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            messageButtonStyle.fontSize = 10;

            if (renderName) {
                GUI.Button(new Rect(position.x - .5f * MessageButtonWidth, position.y - .5f * MessageButtonHeight, MessageButtonWidth, MessageButtonHeight), name, messageButtonStyle);
            } else {
                GUI.Button(new Rect(position.x - .5f * MessageButtonWidthSmall, position.y - .5f * MessageButtonHeightSmall, MessageButtonWidthSmall, MessageButtonHeightSmall), "", messageButtonStyle);
            }

            GUI.color = Color.white;
        }
    }

}