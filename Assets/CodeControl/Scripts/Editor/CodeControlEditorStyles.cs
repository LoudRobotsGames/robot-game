/// <copyright file="CodeControlEditorStyles.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;

namespace CodeControl.Editor {

    public static class CodeControlEditorStyles {

        public enum LabelStyle {
            WarningMessage,
            ModelRelationsTitle,
            AggregationCountType
        }

        public static Color MainColor = new Color(89 / 255.0f, 
                                                  247 / 255.0f, 
                                                  92 / 255.0f, 
                                                  1.0f);

        public static Color MessageLineColor = new Color(63 / 255.0f,
                                                         182 / 255.0f,
                                                         64 / 255.0f,
                                                         1.0f);

        public static Color LineMessageColor = new Color(89 / 255.0f,
                                                          247 / 255.0f,
                                                          92 / 255.0f, 
                                                         .8f);

        public static Color NoContentColor = Color.white;

        public static float MiniMapMouseOutAlpha = .2f;

        public static Color LineMessageColorTypeless = new Color(.8f, .8f, .8f, .8f);

        public static void SetLabelStyle(LabelStyle style) {
            ResetLabelStyle();
            switch (style) {
                case LabelStyle.WarningMessage:
                    GUI.skin.label.fontSize = 20;
                    GUI.skin.label.normal.textColor = new Color(.8f, .8f, .8f, 1.0f);
                    break;
                case LabelStyle.ModelRelationsTitle:
                    GUI.skin.label.fontSize = 20;
                    GUI.skin.label.alignment = TextAnchor.UpperCenter;
                    GUI.skin.label.normal.textColor = new Color(.8f, .8f, .8f, 1.0f);
                    break;
                case LabelStyle.AggregationCountType:
                    GUI.skin.label.alignment = TextAnchor.UpperRight;
                    GUI.skin.label.normal.textColor = new Color(.8f, .8f, .8f, 1.0f);
                    break;
            }
        }

        public static void ResetLabelStyle() {
            GUI.skin.label.fontSize = 11;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.label.normal.textColor = Color.black;
        }

    }

}