#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;

using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor {

    public class WelcomeWindow : EditorWindow {

        private static readonly Texture2D docsIcon = EditorGUIUtility.FindTexture( "cs Script Icon" );
        private static readonly Texture2D resourcesIcon = EditorGUIUtility.FindTexture("d_WelcomeScreen.AssetStoreLogo");
        private static readonly Texture2D communityIcon = EditorGUIUtility.FindTexture("AudioChorusFilter Icon");
        private static System.Type graphType;

        void OnEnable() {

            #if UNITY_5
            titleContent = new GUIContent("Welcome");
            #else
            title = "Welcome";
            #endif

            var size = new Vector2(630, 380);
            minSize = size;
            maxSize = size;
        }

        void OnGUI() {

            var att = graphType != null? (GraphInfoAttribute)graphType.GetCustomAttributes(typeof(GraphInfoAttribute), true).FirstOrDefault() : null;
            var packageName = att != null? att.packageName : "NodeCanvas";
            var docsURL = att != null? att.docsURL : "http://nodecanvas.com";
            var resourcesURL = att != null? att.resourcesURL : "http://nodecanvas.com/";
            var forumsURL = att != null? att.forumsURL : "http://nodecanvas.com/";

            GUI.skin.label.richText = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            GUILayout.Label(string.Format("<size=26><b>Welcome to {0}!</b></size>", packageName ));
            GUILayout.Label(string.Format("<i>Thanks for using {0}! Following are a few important links to get you started!</i>", packageName ) );
            GUILayout.Space(10);




            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(1,1,1,0f);
            if ( GUILayout.Button(docsIcon, GUILayout.Width(64), GUILayout.Height(64)) ) {
                UnityEditor.Help.BrowseURL(docsURL);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.Label("<size=14><b>Documentation</b></size>\nRead thorough documentation and API reference online.");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;





            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(1, 1, 1, 0f);
            if (GUILayout.Button(resourcesIcon, GUILayout.Width(64), GUILayout.Height(64)))
            {
                UnityEditor.Help.BrowseURL(resourcesURL);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.Label("<size=14><b>Resources</b></size>\nDownload samples, extensions and other resources.");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;





            GUILayout.BeginHorizontal("box");
            GUI.backgroundColor = new Color(1, 1, 1, 0f);
            if (GUILayout.Button(communityIcon, GUILayout.Width(64), GUILayout.Height(64)))
            {
                UnityEditor.Help.BrowseURL(forumsURL);
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.Label("<size=14><b>Community</b></size>\nJoin the online forums, give feedback and get support.");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = Color.white;



            GUILayout.FlexibleSpace();

            GUILayout.Label("Please consider leaving a ★★★★★ if you like! Thank you!");

            GUILayout.Space(5);

            NCPrefs.showWelcomeWindow = EditorGUILayout.ToggleLeft("Show On Startup", NCPrefs.showWelcomeWindow);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
        }

        
        public static void ShowWindow(){ ShowWindow(null); }
        public static void ShowWindow(System.Type t) {
            var window = CreateInstance<WelcomeWindow>();
            graphType = t;
            window.ShowUtility();
        }
    }
}

#endif