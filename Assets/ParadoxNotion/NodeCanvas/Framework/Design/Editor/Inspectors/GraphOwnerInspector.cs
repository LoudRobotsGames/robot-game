#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor{


    [InitializeOnLoad]
    public class HierarchyIcons {

        static HierarchyIcons() {
            EditorApplication.hierarchyWindowItemOnGUI += ShowIcon;
        }

        static void ShowIcon(int ID, Rect r) {
            var go = EditorUtility.InstanceIDToObject(ID) as GameObject;
            if ( go == null || go.GetComponent<GraphOwner>() == null ) return;
            r.x = r.xMax - 18;
            r.width = 18;
            GUI.Label(r, "♟");
        }
    }



	[CustomEditor(typeof(GraphOwner), true)]
	public class GraphOwnerInspector : UnityEditor.Editor {

		private GraphOwner owner{
			get{return target as GraphOwner;}
		}

		
		//destroy local graph when owner gets destroyed
		void OnDestroy(){
			if (owner == null && owner.graph != null){
				//1st check is for prefab owner with local graph
				//2nd check is for non prefab with local graph
				if (AssetDatabase.IsSubAsset(owner.graph) || !EditorUtility.IsPersistent(owner.graph))
					DestroyImmediate(owner.graph, true);
			}
		}

		//create new graph asset and assign it to owner
		Graph NewAsAsset(){
			var newGraph = (Graph)EditorUtils.CreateAsset(owner.graphType, true);
			if (newGraph != null){
				Undo.RecordObject(owner, "Export To Asset");
				owner.graph = newGraph;
				EditorUtility.SetDirty(owner);
				EditorUtility.SetDirty(newGraph);
				AssetDatabase.SaveAssets();
			}
			return newGraph;			
		}

		//create new local graph and assign it to owner
		Graph NewAsLocal(){
			var newGraph = (Graph)EditorUtils.AddScriptableComponent(owner.gameObject, owner.graphType);
			newGraph.name = owner.graphType.Name;
			newGraph.hideFlags = HideFlags.HideInInspector;
			Undo.RegisterCreatedObjectUndo(newGraph, "New Bound Graph");
			Undo.RecordObject(owner, "New Bound Graph");
			owner.graph = newGraph;
			EditorUtility.SetDirty(owner);
			EditorUtility.SetDirty(newGraph);
			return newGraph;
		}

		//Bind graph to owner
		void AssetToLocal(){
			var newGraph = (Graph)EditorUtils.AddScriptableComponent(owner.gameObject, owner.graphType);
			newGraph.hideFlags = HideFlags.HideInInspector;
			
			EditorUtility.CopySerialized(owner.graph, newGraph);
			newGraph.Validate();

			Undo.RegisterCreatedObjectUndo(newGraph, "New Bound Graph");
			Undo.RecordObject(owner, "New Bound Graph");
			owner.graph = newGraph;
			EditorUtility.SetDirty(owner);
			EditorUtility.SetDirty(newGraph);
		}

		//Save graph to asset
		Graph LocalToAsset(){
			var newGraph = (Graph)EditorUtils.CreateAsset(owner.graphType, true);
			if (newGraph != null){
				
				EditorUtility.CopySerialized(owner.graph, newGraph);
				newGraph.Validate();
				
				EditorUtility.SetDirty(owner);
				EditorUtility.SetDirty(newGraph);
				AssetDatabase.SaveAssets();
			}
			return newGraph;			
		}

		
		public override void OnInspectorGUI(){

			UndoManager.CheckUndo(owner, "Graph Owner Inspector");

			var ownerPeristant = EditorUtility.IsPersistent(owner);
			var label = owner.graphType.Name.SplitCamelCase();

			if (owner.graph == null){
				EditorGUILayout.HelpBox(owner.GetType().Name + " needs a " + label + ".\nAssign or Create a new one...", MessageType.Info);
				if (!Application.isPlaying && GUILayout.Button("CREATE NEW")){
					
					Graph newGraph = null;
					if (EditorUtility.DisplayDialog("Create Graph", "Create a Bound or an Asset Graph?\n\n"+
                        "Bound Graph is saved with the GraphOwner and you can use direct scene references within it.\n\n"+
                        "Asset Graph is an asset file and can be reused amongst any number of GraphOwners.\n\n"+
                        "You can convert from one type to the other at any time.",
                        "Bound", "Asset")){

						newGraph = NewAsLocal();

					} else {

						newGraph = NewAsAsset();
					}

					if (newGraph != null){
						GraphEditor.OpenWindow(owner);
					}
				}

				owner.graph = (Graph)EditorGUILayout.ObjectField(label, owner.graph, owner.graphType, false);
				return;
			}

			GUILayout.Space(10);

			//Graph comments ONLY if Bound graph
			if (owner.graphIsLocal){
				owner.graph.graphComments = GUILayout.TextArea(owner.graph.graphComments, GUILayout.Height(45));
				EditorUtils.TextFieldComment(owner.graph.graphComments, "Graph comments...");
			}

			//Open behaviour
			GUI.backgroundColor = EditorUtils.lightBlue;
			if (GUILayout.Button( ("Edit " + owner.graphType.Name.SplitCamelCase()).ToUpper() )){
				GraphEditor.OpenWindow(owner);
			}
			GUI.backgroundColor = Color.white;
		
			if (!Application.isPlaying){

				if (!owner.graphIsLocal && GUILayout.Button("Bind Graph")){
					if (EditorUtility.DisplayDialog("Bind Graph", "This will make a local copy of the graph, bound to the owner.\n\nThis allows you to make local changes and assign scene object references directly.\n\nNote that you can also use scene references through the Blackboard Variables\n\nBind Graph?", "YES", "NO"))
						AssetToLocal();
				}

				//Reference graph
				if (!owner.graphIsLocal){

					owner.graph = (Graph)EditorGUILayout.ObjectField(label, owner.graph, owner.graphType, true);

				} else {

					if (GUILayout.Button("Delete Bound Graph")){
						if (EditorUtility.DisplayDialog("Delete Bound Graph", "Are you sure?", "YES", "NO")){
							Undo.DestroyObjectImmediate(owner.graph);
							Undo.RecordObject(owner, "Delete Bound");
							owner.graph = null;
							EditorUtility.SetDirty(owner);
						}
					}
				}
			}



			//basic options
//			owner.blackboard = (Blackboard)EditorGUILayout.ObjectField("Blackboard", owner.blackboard as Blackboard, typeof(Blackboard), true);
			owner.enableAction = (GraphOwner.EnableAction)EditorGUILayout.EnumPopup("On Enable", owner.enableAction);
			owner.disableAction = (GraphOwner.DisableAction)EditorGUILayout.EnumPopup("On Disable", owner.disableAction);


			EditorUtils.Separator();

			//derived GUI
			OnExtraOptions();

			//execution debug controls
			if (Application.isPlaying && owner.graph != null && !ownerPeristant){
				var pressed = new GUIStyle(GUI.skin.GetStyle("button"));
				pressed.normal.background = GUI.skin.GetStyle("button").active.background;

				GUILayout.BeginHorizontal("box");
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(EditorUtils.playIcon, owner.isRunning || owner.isPaused? pressed : (GUIStyle)"button")){
					if (owner.isRunning || owner.isPaused) owner.StopBehaviour();
					else owner.StartBehaviour();
				}

				if (GUILayout.Button(EditorUtils.pauseIcon, owner.isPaused? pressed : (GUIStyle)"button")){	
					if (owner.isPaused) owner.StartBehaviour();
					else owner.PauseBehaviour();
				}

				OnGrapOwnerControls();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			EditorUtils.ShowAutoEditorGUI(owner);
			EditorUtils.EndOfInspector();

			UndoManager.CheckDirty(owner);

			if (GUI.changed){
				EditorUtility.SetDirty(owner);
				if (owner.graph != null){
					EditorUtility.SetDirty(owner.graph);
				}
			}
		}

		virtual protected void OnExtraOptions(){}
		virtual protected void OnGrapOwnerControls(){}
	}
}

#endif