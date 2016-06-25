using System;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Script Control/Common")]
	[Description("Will subscribe to a public event of Action type and return true when the event is raised.\n(eg public event System.Action [name])")]
	public class CheckCSharpEvent : ConditionTask {

		[SerializeField]
		private System.Type targetType;
		[SerializeField]
		private string eventName;

		public override Type agentType{
			get {return targetType ?? typeof(Transform);}
		}
		
		protected override string info{
			get
			{
				if (string.IsNullOrEmpty(eventName))
					return "No Event Selected";
				return string.Format("'{0}' Raised", eventName);
			}
		}


		protected override string OnInit(){
			
			if (eventName == null)
				return "No Event Selected";

			var eventInfo = agentType.RTGetEvent(eventName);
			if (eventInfo == null)
				return "Event was not found";

			System.Action pointer = ()=> { Raised(); };
			eventInfo.AddEventHandler( agent, pointer );
			return null;
		}

		public void Raised(){
			YieldReturn(true);
		}

		protected override bool OnCheck(){
			return false;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnTaskInspectorGUI(){

			if (!Application.isPlaying && GUILayout.Button("Select Event")){
				Action<EventInfo> Selected = (e)=> {
					targetType = e.DeclaringType;
					eventName = e.Name;
				};

				if (agent != null){
					EditorUtils.ShowGameObjectEventSelectionMenu(agent.gameObject, null, Selected);
				} else {
					var menu = new UnityEditor.GenericMenu();
					foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(Component), true))
						menu = EditorUtils.GetEventSelectionMenu(t, null, Selected, menu);
					menu.ShowAsContext();
					Event.current.Use();
				}
			}

			if (targetType != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Selected Type", agentType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Selected Event", eventName);
				GUILayout.EndVertical();
			}
		}
		
		#endif
	}



	[Category("✫ Script Control/Common")]
	[Description("Will subscribe to a public event of Action<T> type and return true when the event is raised.\n(eg public event System.Action<T> [name])")]
	public class CheckCSharpEvent<T> : ConditionTask {

		[SerializeField]
		private System.Type targetType;
		[SerializeField]
		private string eventName;
		[SerializeField]
		protected BBParameter<T> saveAs;

		public override Type agentType{
			get {return targetType ?? typeof(Transform);}
		}
		
		protected override string info{
			get
			{
				if (string.IsNullOrEmpty(eventName))
					return "No Event Selected";
				return string.Format("'{0}' Raised", eventName);
			}
		}


		protected override string OnInit(){

			if (eventName == null)
				return "No Event Selected";			

			var eventInfo = agentType.RTGetEvent(eventName);
			if (eventInfo == null)
				return "Event was not found";

			System.Action<T> pointer = (v)=> { Raised(v); };
			eventInfo.AddEventHandler( agent, pointer );
			return null;
		}

		public void Raised(T eventValue){
			saveAs.value = eventValue;
			YieldReturn(true);
		}

		protected override bool OnCheck(){
			return false;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnTaskInspectorGUI(){

			if (!Application.isPlaying && GUILayout.Button("Select Event")){
				Action<EventInfo> Selected = (e)=> {
					targetType = e.DeclaringType;
					eventName = e.Name;
				};

				if (agent != null){
					EditorUtils.ShowGameObjectEventSelectionMenu(agent.gameObject, typeof(T), Selected);
				} else {
					var menu = new UnityEditor.GenericMenu();
					foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(Component), true))
						menu = EditorUtils.GetEventSelectionMenu(t, typeof(T), Selected, menu);
					menu.ShowAsContext();
					Event.current.Use();
				}
			}

			if (targetType != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Selected Type", agentType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Selected Event", eventName);
				GUILayout.EndVertical();

				EditorUtils.BBParameterField("Save Value As", saveAs, true);
			}
		}
		
		#endif
	}
}