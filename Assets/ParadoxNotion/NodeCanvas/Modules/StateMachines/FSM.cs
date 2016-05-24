using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines{

	/// <summary>
	/// Use FSMs to create state like behaviours
	/// </summary>
	[GraphInfo(
		packageName = "NodeCanvas",
		docsURL = "http://nodecanvas.paradoxnotion.com/documentation/",
		resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/",
		forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/"
		)]
	public class FSM : Graph{

		private IUpdatable[] updatableNodes;

		private event System.Action<IState> CallbackEnter;
		private event System.Action<IState> CallbackStay;
		private event System.Action<IState> CallbackExit;

		public FSMState currentState{get; private set;}
		public FSMState previousState{get; private set;}

		///The current state name. Null if none
		public string currentStateName{
			get {return currentState != null? currentState.name : null; }
		}

		///The previous state name. Null if none
		public string previousStateName{
			get	{return previousState != null? previousState.name : null; }
		}

		public override System.Type baseNodeType{ get {return typeof(FSMState);} }
		public override bool requiresAgent{	get {return true;} }
		public override bool requiresPrimeNode { get {return true;} }
		public override bool autoSort{ get {return false;} }
		public override bool useLocalBlackboard{get {return false;}}


		protected override void OnGraphStarted(){

			GatherDelegates();

			//collect AnyStates and ConcurentStates
			updatableNodes = allNodes.OfType<IUpdatable>().ToArray();
			//enable AnyStates
			foreach(var anyState in allNodes.OfType<AnyState>()){
				anyState.Execute(agent, blackboard);
			}
			//enable ConcurentStates
			foreach(var conc in allNodes.OfType<ConcurrentState>()){
				conc.Execute(agent, blackboard);
			}

			//enter the last or start state
			EnterState(previousState == null? (FSMState)primeNode : previousState);
		}

		protected override void OnGraphUpdate(){

			if (currentState == null){
				//Debug.LogError("Current FSM State is or became null. Stopping FSM...");
				Stop(false);
				return;
			}

			//do this first. This automaticaly stops the graph if the current state is finished and has no transitions
			if (currentState.status != Status.Running && currentState.outConnections.Count == 0){
				Stop(true);
				return;
			}

			//Update AnyStates and ConcurentStates
			for (var i = 0; i < updatableNodes.Length; i++)
				updatableNodes[i].Update();

			//Update current state
			currentState.Update();
			
			if (CallbackStay != null && currentState != null && currentState.status == Status.Running){
				CallbackStay(currentState);
			}
		}

		protected override void OnGraphStoped(){
			if (currentState != null){
				if (CallbackExit != null){
					CallbackExit(currentState);
				}
				currentState.Finish();
				currentState.Reset();
			}

			previousState = null;
			currentState = null;
		}

		protected override void OnGraphPaused(){
			previousState = currentState;
			currentState = null;
		}

		///Enter a state providing the state itself
		public bool EnterState(FSMState newState){

			if (!isRunning){
				Debug.LogWarning("Tried to EnterState on an FSM that was not running", this);
				return false;
			}

			if (newState == null){
				Debug.LogWarning("Tried to Enter Null State");
				return false;
			}

			if (currentState != null){	

				if (CallbackExit != null){
					CallbackExit(currentState);
				}

				currentState.Finish();
				currentState.Reset();

				#if UNITY_EDITOR //Done for visualizing in editor
				for (var i = 0; i < currentState.inConnections.Count; i++)
					currentState.inConnections[i].connectionStatus = Status.Resting;
				#endif
			}

			previousState = currentState;
			currentState = newState;

			if (CallbackEnter != null){
				CallbackEnter(currentState);
			}

			currentState.Execute(agent, blackboard);
			return true;
		}

		///Trigger a state to enter by it's name. Returns the state found and entered if any
		public FSMState TriggerState(string stateName){

			var state = GetStateWithName(stateName);
			if (state != null){
				EnterState(state);
				return state;
			}

			Debug.LogWarning("No State with name '" + stateName + "' found on FSM '" + name + "'");
			return null;
		}

		///Get all State Names
		public string[] GetStateNames(){
			return allNodes.Where(n => n.allowAsPrime).Select(n => n.name).ToArray();
		}

		///Get a state by it's name
		public FSMState GetStateWithName(string name){
			return (FSMState)allNodes.Find(n => n.allowAsPrime && n.name == name);
		}

		//Gather and creates delegates from MonoBehaviours on agents that implement required methods
		void GatherDelegates(){

			foreach (var _mono in agent.gameObject.GetComponents<MonoBehaviour>()){
                
				var mono = _mono;
				var enterMethod = mono.GetType().RTGetMethod("OnStateEnter");
				var stayMethod = mono.GetType().RTGetMethod("OnStateUpdate");
				var exitMethod = mono.GetType().RTGetMethod("OnStateExit");

				if (enterMethod != null){
					try { CallbackEnter += enterMethod.RTCreateDelegate<System.Action<IState>>(mono); } //JIT
					catch { CallbackEnter += (m)=>{ enterMethod.Invoke(mono, new object[]{m}); }; } //AOT
				}

				if (stayMethod != null){
					try { CallbackStay += stayMethod.RTCreateDelegate<System.Action<IState>>(mono); } //JIT
					catch { CallbackStay += (m)=>{ stayMethod.Invoke(mono, new object[]{m}); }; } //AOT
				}

				if (exitMethod != null){
					try { CallbackExit += exitMethod.RTCreateDelegate<System.Action<IState>>(mono); } //JIT
					catch { CallbackExit += (m)=>{ exitMethod.Invoke(mono, new object[]{m}); }; } //AOT
				}
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		[UnityEditor.MenuItem("Tools/ParadoxNotion/NodeCanvas/Create/State Machine", false, 0)]
		public static void Editor_CreateGraph(){
			var newGraph = EditorUtils.CreateAsset<FSM>(true);
			UnityEditor.Selection.activeObject = newGraph;
		}

		[UnityEditor.MenuItem("Assets/Create/ParadoxNotion/NodeCanvas/State Machine")]
		public static void Editor_CreateGraphFix(){
			var path = EditorUtils.GetAssetUniquePath("FSM.asset");
			var newGraph = EditorUtils.CreateAsset<FSM>(path);
			UnityEditor.Selection.activeObject = newGraph;
		}
		
		#endif
	}
}