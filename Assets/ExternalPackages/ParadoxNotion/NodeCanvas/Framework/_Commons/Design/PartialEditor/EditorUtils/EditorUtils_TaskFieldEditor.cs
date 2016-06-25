#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;
using ParadoxNotion;
using UnityObject = UnityEngine.Object;

namespace ParadoxNotion.Design{

    /// <summary>
    /// Task specific Editor field
    /// </summary>

	partial class EditorUtils {

		static string search = string.Empty;

		public static void TaskField<T>(T task, ITaskSystem ownerSystem, Action<T> callback) where T : NodeCanvas.Framework.Task{
			TaskField(task, ownerSystem, typeof(T), (Task t)=> { callback((T)t); });
		}

		public static void TaskField(Task task, ITaskSystem ownerSystem, Type baseType, Action<Task> callback){
			if (task == null){
				TaskSelectionButton(ownerSystem, baseType, callback);
			} else {
				Task.ShowTaskInspectorGUI(task, callback);
			}
		}

        public static void TaskSelectionButton<T>(ITaskSystem ownerSystem, Action<T> callback) where T : NodeCanvas.Framework.Task
        {
			TaskSelectionButton(ownerSystem, typeof(T), (Task t)=> { callback((T)t); });
		}

		//Shows a button that when clicked, pops a context menu with a list of tasks deriving the base type specified. When something is selected the callback is called
		//On top of that it also shows a search field for Tasks
		public static void TaskSelectionButton(ITaskSystem ownerSystem, Type baseType, Action<Task> callback){

			Action<Type> TaskTypeSelected = (t)=> {
				var newTask = Task.Create(t, ownerSystem);
				Undo.RecordObject(ownerSystem.contextObject, "New Task");
				callback(newTask);
			};

			Func<GenericMenu> GetMenu = ()=> {
				var menu = GetTypeSelectionMenu(baseType, TaskTypeSelected);
				if (Task.copiedTask != null && baseType.IsAssignableFrom( Task.copiedTask.GetType()) )
					menu.AddItem(new GUIContent(string.Format("Paste ({0})", Task.copiedTask.name) ), false, ()=> { callback( Task.copiedTask.Duplicate(ownerSystem) ); });
				return menu;				
			};


			GUI.backgroundColor = lightBlue;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add " + baseType.Name.SplitCamelCase() )){
				GetMenu().ShowAsContext();
				Event.current.Use();
			}
			if (GUILayout.Button("...", GUILayout.Width(22))){
				CompleteContextMenu.Show(GetMenu(), Event.current.mousePosition, "Add Task");
				Event.current.Use();				
			}
			GUILayout.EndHorizontal();


			GUI.backgroundColor = Color.white;
			GUILayout.BeginHorizontal();
			search = EditorGUILayout.TextField(search, (GUIStyle)"ToolbarSeachTextField");
			if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton")){
				search = string.Empty;
				GUIUtility.keyboardControl = 0;
			}
			GUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(search)){
				GUILayout.BeginVertical("TextField");
				foreach (var taskInfo in GetScriptInfosOfType(baseType)){
					if (taskInfo.name.Replace(" ", "").ToUpper().Contains( search.Replace(" ", "").ToUpper() )){
						if (GUILayout.Button(taskInfo.name)){
							search = string.Empty;
							GUIUtility.keyboardControl = 0;
							TaskTypeSelected(taskInfo.type);
						}
					}
				}
				GUILayout.EndVertical();
			}
		}

	}
}

#endif