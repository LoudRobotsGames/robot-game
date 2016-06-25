#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor{

	/// <summary>
	/// Editor for IBlackboards
	/// </summary>
    public static class BlackboardEditor {

		class ReorderingState{
			public List<Variable> list;
			public bool isReordering = false;
			public ReorderingState(List<Variable> list){
				this.list = list;
			}
		}

		private static readonly Dictionary<IBlackboard, ReorderingState> tempStates = new Dictionary<IBlackboard, ReorderingState>();

		public static Variable pickedVariable{get;set;}
		public static IBlackboard pickedVariableBlackboard{get;set;}
		public static void ShowVariables(IBlackboard bb, UnityEngine.Object contextParent){

			var layoutOptions = new GUILayoutOption[]{GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(16)};

			//Begin undo check
			UndoManager.CheckUndo(contextParent, "Blackboard Inspector");

			//Add variable button
			GUI.backgroundColor = new Color(0.8f,0.8f,1);
			if (GUILayout.Button("Add Variable")){
				System.Action<System.Type> SelectVar = (t)=>
				{
					var name = "my" + t.FriendlyName();
					while (bb.GetVariable(name) != null){
						name += ".";
					}
					bb.AddVariable(name, t);
				};

				System.Action<PropertyInfo> SelectBoundProp = (p) =>
				{
					var newVar = bb.AddVariable(p.Name, p.PropertyType);
					newVar.BindProperty(p);
				};

				System.Action<FieldInfo> SelectBoundField = (f) =>
				{
					var newVar = bb.AddVariable(f.Name, f.FieldType);
					newVar.BindProperty(f);
				};

				var menu = new GenericMenu();
				menu = EditorUtils.GetPreferedTypesSelectionMenu(typeof(object), SelectVar, true, menu, "New");

				if (bb.propertiesBindTarget != null){
					foreach (var comp in bb.propertiesBindTarget.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ){
						menu = EditorUtils.GetPropertySelectionMenu(comp.GetType(), typeof(object), SelectBoundProp, false, false, menu, "Bound Property");
						menu = EditorUtils.GetFieldSelectionMenu(comp.GetType(), typeof(object), SelectBoundField, menu, "Bound Field");
					}
				}

				menu.AddSeparator("/");
				menu.AddItem(new GUIContent("Separator"), false, ()=>{ SelectVar(typeof(VariableSeperator)); } );

				menu.ShowAsContext();
				Event.current.Use();
			}


			GUI.backgroundColor = Color.white;

			//Simple column header info
			if (bb.variables.Keys.Count != 0){
				GUILayout.BeginHorizontal();
				GUI.color = Color.yellow;
				GUILayout.Label("Name", layoutOptions);
				GUILayout.Label("Value", layoutOptions);
				GUI.color = Color.white;
				GUILayout.EndHorizontal();
			} else {
				EditorGUILayout.HelpBox("Blackboard has no variables", MessageType.Info);
			}

			if (!tempStates.ContainsKey(bb)){
				tempStates.Add(bb, new ReorderingState(bb.variables.Values.ToList()));
			}

			//Make temporary list for editing variables
			if (!tempStates[bb].isReordering){
				tempStates[bb].list = bb.variables.Values.ToList();
			}

			//The actual variables reorderable list
			EditorUtils.ReorderableList(tempStates[bb].list, delegate(int i){

				var data = tempStates[bb].list[i];
				if (data == null){
					GUILayout.Label("NULL Variable!");
					return;
				}

				GUILayout.BeginHorizontal();

				//Name of the variable GUI control
				if (!Application.isPlaying){

					//The small box on the left to re-order variables
					GUI.backgroundColor = new Color(1,1,1,0.8f);
					GUILayout.Box("", GUILayout.Width(6));
					GUI.backgroundColor = new Color(0.7f,0.7f,0.7f, 0.3f);

					if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) ){
						tempStates[bb].isReordering = true;
						if (data.varType != typeof(VariableSeperator)){
							pickedVariable = data;
							pickedVariableBlackboard = bb;
						}
					}
					
					//Make name field red if same name exists
					if (tempStates[bb].list.Where(v => v != data).Select(v => v.name).Contains(data.name)){
						GUI.backgroundColor = Color.red;
					}

					GUI.enabled = !data.isProtected;
					if (data.varType != typeof(VariableSeperator)){
						data.name = EditorGUILayout.TextField(data.name, layoutOptions);
					} else {
						GUILayout.Box("------------", layoutOptions);
					}
					GUI.enabled = true;
					GUI.backgroundColor = Color.white;

				} else {

					//Don't allow name edits in play mode. Instead show just a label
					if (data.varType != typeof(VariableSeperator)){
						GUI.backgroundColor = new Color(0.7f,0.7f,0.7f);
						GUI.color = new Color(0.8f,0.8f,1f);
						GUILayout.Label(data.name, layoutOptions);
					} else {
						GUI.color = Color.black;
						GUILayout.Box("------------", layoutOptions);
						GUI.color = Color.white;						
					}
				}
				
				//reset coloring
				GUI.color = Color.white;
				GUI.backgroundColor = Color.white;

				//Show the respective data GUI
				if (data.varType != typeof(VariableSeperator)){
					ShowDataGUI(data, bb, contextParent, layoutOptions);
				} else {
					GUILayout.Space(0);
					GUILayout.Space(0);
				}

				//reset coloring
				GUI.color = Color.white;
				GUI.backgroundColor = Color.white;
				
				//'X' to delete data
				if (!Application.isPlaying && GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(16))){
					if (EditorUtility.DisplayDialog("Delete Variable '" + data.name + "'", "Are you sure?", "Yes", "No!")){
						tempStates[bb].list.Remove(data);
					}
				}

				GUILayout.EndHorizontal();

			}, contextParent);

			//reset coloring
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;

			if ( (GUI.changed && !tempStates[bb].isReordering) || Event.current.rawType == EventType.MouseUp){
				tempStates[bb].isReordering = false;
				EditorApplication.delayCall += ()=>{ pickedVariable = null; pickedVariableBlackboard = null; };
				//reconstruct the dictionary
				try { bb.variables = tempStates[bb].list.ToDictionary(d => d.name, d => d); }
				catch { Debug.LogError("Blackboard has duplicate names!"); }
		    }

			//Check dirty
			UndoManager.CheckDirty(contextParent);
		}

		static void ShowDataGUI(Variable data, IBlackboard bb, UnityEngine.Object contextParent, GUILayoutOption[] layoutOptions){

			//Bind info or value GUI control
			if (data.hasBinding){
				var arr = data.propertyPath.Split('.');
				GUI.color = new Color(0.8f,0.8f,1);
				GUILayout.Label(string.Format(".{0} ({1})", arr[1], arr[0]), layoutOptions);
				GUI.color = Color.white;
			} else {
				GUI.enabled = !data.isProtected;
				data.value = VariableField(data, contextParent, layoutOptions);
				GUI.enabled = true;
				GUI.backgroundColor = Color.white;
			}

			//Variable options menu
			if ( !Application.isPlaying && GUILayout.Button(" ", GUILayout.Width(8), GUILayout.Height(16))){

				System.Action<PropertyInfo> SelectProp = (p) => {
					data.BindProperty(p);
				};

				System.Action<FieldInfo> SelectField = (f) => {
					data.BindProperty(f);
				};

				var menu = new GenericMenu();

				if (bb.propertiesBindTarget != null){
					foreach (var comp in bb.propertiesBindTarget.GetComponents(typeof(Component)).Where(c => c.hideFlags == 0) ){
						menu = EditorUtils.GetPropertySelectionMenu(comp.GetType(), data.varType, SelectProp, false, false, menu, "Bind Property");
						menu = EditorUtils.GetFieldSelectionMenu(comp.GetType(), data.varType, SelectField, menu, "Bind Field");
					}
				}

				menu.AddItem(new GUIContent("Protected"), data.isProtected, ()=>{ data.isProtected = !data.isProtected; });

				if (bb.propertiesBindTarget != null){
					menu.AddSeparator("/");
					if (data.hasBinding){
						menu.AddItem(new GUIContent("UnBind"), false, ()=> {data.UnBindProperty();});
					} else {
						menu.AddDisabledItem(new GUIContent("UnBind"));
					}
				}
				
				menu.ShowAsContext();
				Event.current.Use();
			}
		}

		//While there is a similar method in EditorUtils, due to layouting and especialy no prefix name, this has to be redone a bit differently
		static object VariableField(Variable data, UnityEngine.Object context, GUILayoutOption[] layoutOptions){

			var o = data.value;
			var t = data.varType;

			//Check scene object type for UnityObjects. Consider Interfaces as scene object type. Assumpt that user uses interfaces with UnityObjects
			var isSceneObjectType = (typeof(Component).IsAssignableFrom(t) || typeof(IScriptableComponent).IsAssignableFrom(t) || t == typeof(GameObject) || t.IsInterface);
			if (typeof(UnityEngine.Object).IsAssignableFrom(t) || t.IsInterface){
				return UnityEditor.EditorGUILayout.ObjectField((UnityEngine.Object)o, t, isSceneObjectType, layoutOptions);
			}

		    //Check Type second
			if (t == typeof(System.Type)){
				return EditorUtils.Popup<System.Type>(null, (System.Type)o, UserTypePrefs.GetPreferedTypesList(typeof(object), false), layoutOptions );
			}

			t = o != null? o.GetType() : t;
			if (t.IsAbstract){
				GUILayout.Label( string.Format("({0})", t.FriendlyName()), layoutOptions );
				return o;
			}

			if (o == null && !t.IsAbstract && !t.IsInterface && (t.GetConstructor(System.Type.EmptyTypes) != null || t.IsArray ) ){
				if (GUILayout.Button("(null) Create", layoutOptions)){
					if (t.IsArray)
						return System.Array.CreateInstance(t.GetElementType(), 0);
					return System.Activator.CreateInstance(t);
				}
				return o;
			}			

			if (t == typeof(bool))
				return UnityEditor.EditorGUILayout.Toggle((bool)o, layoutOptions);
			if (t == typeof(Color))
				return UnityEditor.EditorGUILayout.ColorField((Color)o, layoutOptions);
			if (t == typeof(AnimationCurve))
				return UnityEditor.EditorGUILayout.CurveField((AnimationCurve)o, layoutOptions);
			if (t.IsSubclassOf(typeof(System.Enum) ))
				return UnityEditor.EditorGUILayout.EnumPopup((System.Enum)o, layoutOptions);
			if (t == typeof(float)){
				GUI.backgroundColor = UserTypePrefs.GetTypeColor(t);
				return UnityEditor.EditorGUILayout.FloatField((float)o, layoutOptions);
			}
			if (t == typeof(int)){
				GUI.backgroundColor = UserTypePrefs.GetTypeColor(t);
				return UnityEditor.EditorGUILayout.IntField((int)o, layoutOptions);
			}
			if (t == typeof(string)){
				GUI.backgroundColor = UserTypePrefs.GetTypeColor(t);
				return UnityEditor.EditorGUILayout.TextField((string)o, layoutOptions);
			}

			if (t == typeof(Vector2))
				return UnityEditor.EditorGUILayout.Vector2Field("", (Vector2)o, layoutOptions);
			if (t == typeof(Vector3))
				return UnityEditor.EditorGUILayout.Vector3Field("", (Vector3)o, layoutOptions);
			if (t == typeof(Vector4))
				return UnityEditor.EditorGUILayout.Vector4Field("", (Vector4)o, layoutOptions);

			if (t == typeof(Quaternion)){
				var q = (Quaternion)o;
				var v = new Vector4(q.x, q.y, q.z, q.w);
				v = UnityEditor.EditorGUILayout.Vector4Field("", v, layoutOptions);
				return new Quaternion(v.x, v.y, v.z, v.w);
			}

			if (t == typeof(LayerMask))
				return EditorUtils.LayerMaskField(null, (LayerMask)o, layoutOptions);



			//If some other type, show it in the generic object editor window
			if (GUILayout.Button( string.Format("{0} {1}", t.FriendlyName(), (o is IList)? ((IList)o).Count.ToString() : "" ), layoutOptions)){
				GenericInspectorWindow.Show(data.name, o, t, context);
			}

			//if we are externaly inspecting value and it's this one, get value from the external editor. This is basicaly done for structs
			if (GenericInspectorWindow.current != null && GenericInspectorWindow.current.inspectedID == data.name){
				return GenericInspectorWindow.current.value;
			}

			return o;
		}
	}
}

#endif