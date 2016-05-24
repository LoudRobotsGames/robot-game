#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines{

	[Name("BehaviourTree")]
	[Category("Nested")]
	[Description("Execute a Behaviour Tree OnEnter. OnExit that Behavior Tree will be stoped or paused based on the relevant specified setting. You can optionaly specify a Success Event and a Failure Event which will be sent when the BT's root node status returns either of the two. If so, use alongside with a CheckEvent on a transition.")]
	public class NestedBTState : FSMState, IGraphAssignable{

		public enum BTExecutionMode
		{
			Once,
			Repeat
		}

		public enum BTExitMode
		{
			Stop_Restart,
			Pause_Resume
		}

		[SerializeField]
		protected BBParameter<BehaviourTree> _nestedBT;

		public BTExecutionMode executionMode = BTExecutionMode.Repeat;
		public BTExitMode exitMode = BTExitMode.Stop_Restart;
		public string successEvent;
		public string failureEvent;
	
		private readonly Dictionary<BehaviourTree, BehaviourTree> instances = new Dictionary<BehaviourTree, BehaviourTree>();
		private bool BTIsFinished;

		public BehaviourTree nestedBT{
			get {return _nestedBT.value;}
			set {_nestedBT.value = value;}
		}

		public Graph nestedGraph{
			get {return nestedBT;}
			set {nestedBT = (BehaviourTree)value;}
		}
/*
		protected override void OnInit(){
			if (nestedBT != null)
				CheckInstance();
		}
*/
		protected override void OnEnter(){

			if (nestedBT == null){
				Finish(false);
				return;
			}

			CheckInstance();

			BTIsFinished = false;
			nestedBT.repeat = (executionMode == BTExecutionMode.Repeat);
			nestedBT.updateInterval = 0;
			nestedBT.StartGraph(graphAgent, graphBlackboard, (success)=> {BTIsFinished = true;});
		}

		protected override void OnUpdate(){

			if (!string.IsNullOrEmpty(successEvent) && nestedBT.rootStatus == Status.Success)
				SendEvent(new EventData(successEvent));

			if (!string.IsNullOrEmpty(failureEvent) && nestedBT.rootStatus == Status.Failure)
				SendEvent(new EventData(failureEvent));
			
			if (BTIsFinished)
				Finish();
		}

		protected override void OnExit(){
			if (IsInstance(nestedBT) && nestedBT.isRunning){
				if (exitMode == BTExitMode.Stop_Restart){
					nestedBT.Stop();
				} else {
					nestedBT.Pause();
				}
			}
		}

		protected override void OnPause(){
			if (IsInstance(nestedBT) && nestedBT.isRunning){
				nestedBT.Pause();
			}
		}

		bool IsInstance(BehaviourTree bt){
			return instances.Values.Contains(bt);
		}

		void CheckInstance(){

			if (IsInstance(nestedBT)){
				return;
			}

			BehaviourTree instance = null;
			if (!instances.TryGetValue(nestedBT, out instance)){
				instance = Graph.Clone<BehaviourTree>(nestedBT);
				instances[nestedBT] = instance;
			}

            instance.agent = graphAgent;
		    instance.blackboard = graphBlackboard;
			nestedBT = instance;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			
			GUILayout.Label(string.Format("Nested BT\n{0}", _nestedBT) );
			if (nestedBT == null){
				if (!Application.isPlaying && GUILayout.Button("CREATE NEW"))
					Node.CreateNested<BehaviourTree>(this);
			}
		}

		protected override void OnNodeInspectorGUI(){

			ShowBaseFSMInspectorGUI();
			EditorUtils.BBParameterField("Behaviour Tree", _nestedBT);

			if (nestedBT == null){
				return;
			}

			executionMode = (BTExecutionMode)EditorGUILayout.EnumPopup("Execution Mode", executionMode);
			exitMode = (BTExitMode)EditorGUILayout.EnumPopup("Exit Mode", exitMode);

			var alpha1 = string.IsNullOrEmpty(successEvent)? 0.5f : 1;
			var alpha2 = string.IsNullOrEmpty(failureEvent)? 0.5f : 1;
			GUILayout.BeginVertical("box");
			GUI.color = new Color(1,1,1,alpha1);
			successEvent = EditorGUILayout.TextField("Success Status Event", successEvent);
			GUI.color = new Color(1,1,1,alpha2);
			failureEvent = EditorGUILayout.TextField("Failure Status Event", failureEvent);
			GUILayout.EndVertical();
			GUI.color = Color.white;

			nestedBT.name = name;


	    	var defParams = nestedBT.GetDefinedParameters();
	    	if (defParams.Length != 0){

		    	EditorUtils.TitledSeparator("Defined Nested BT Parameters");
		    	GUI.color = Color.yellow;
		    	EditorGUILayout.LabelField("Name", "Type");
				GUI.color = Color.white;
		    	var added = new List<string>();
		    	foreach(var bbVar in defParams){
		    		if (!added.Contains(bbVar.name)){
			    		EditorGUILayout.LabelField(bbVar.name, bbVar.varType.FriendlyName());
			    		added.Add(bbVar.name);
			    	}
		    	}
		    	if (GUILayout.Button("Check/Create Blackboard Variables")){
		    		nestedBT.CreateDefinedParameterVariables(graphBlackboard);
		    	}
		    }
		}

		#endif
	}
}