using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using UnityEngine;


namespace NodeCanvas.Framework{

    /// <summary>
    /// A component that is used to control a Graph in respects to the gameobject attached to
    /// </summary>
    [ParadoxNotion.Design.SpoofAOT]
	abstract public class GraphOwner : MonoBehaviour {

		public enum EnableAction{
			EnableBehaviour,
			DoNothing
		}

		public enum DisableAction{
			DisableBehaviour,
			PauseBehaviour,
			DoNothing
		}

		///What will happen OnEnable
		[HideInInspector]
		public EnableAction enableAction = EnableAction.EnableBehaviour;
		///What will happen OnDisable
		[HideInInspector]
		public DisableAction disableAction = DisableAction.DisableBehaviour;

		private Dictionary<Graph, Graph> instances = new Dictionary<Graph, Graph>();
		private bool startCalled = false;

		private static bool isQuiting;

		abstract public Graph graph{get;set;}
		abstract public IBlackboard blackboard{get;set;}
		abstract public System.Type graphType{get;}

		abstract public void StartBehaviour();
		abstract public void StartBehaviour(System.Action<bool> callback);
		abstract public void PauseBehaviour();
		abstract public void StopBehaviour();
		abstract public void UpdateBehaviour();

		///Is the graph local?
		public bool graphIsLocal{
			get
			{
				var sc = GetComponents(typeof(IScriptableComponent)).Cast<IScriptableComponent>().ToList();
				return sc.Contains(this.graph);
			}
		}

		///Is the assigned graph currently running?
		public bool isRunning{
			get {return graph != null? graph.isRunning : false;}
		}

		///Is the assigned graph currently paused?
		public bool isPaused{
			get {return graph != null? graph.isPaused : false;}
		}

		///The time is seconds the graph is running
		public float elapsedTime{
			get {return graph != null? graph.elapsedTime : 0;}
		}


		//Gets the instance graph for this owner of the provided graph
		protected Graph GetInstance(Graph originalGraph){

			if (originalGraph == null)
				return null;

			//in editor the instance is always the original
			if (!Application.isPlaying)
				return originalGraph;

			//if its already an instance, return the instance
			if (instances.Values.Contains(originalGraph))
				return originalGraph;

			Graph instance = null;

			//if it's not an instance but rather an asset reference which has been instantiated before, return the instance stored
			if (!instances.TryGetValue(originalGraph, out instance)){
				instance = Graph.Clone<Graph>(originalGraph);
				instances[originalGraph] = instance;
			}

			instance.agent = this;
			instance.blackboard = this.blackboard;
			return instance;
		}



		///Send an event through the graph (To be used with CheckEvent for example). Same as .graph.SendEvent
		public void SendEvent(string eventName){ SendEvent(new EventData(eventName));}
		public void SendEvent<T>(string eventName, T eventValue) {SendEvent(new EventData<T>(eventName, eventValue)); }
		public void SendEvent(EventData eventData){
			if (graph != null){
				graph.SendEvent(eventData);
			}
		}

		///Thats the same as calling the static Graph.SendGlobalEvent
		public static void SendGlobalEvent(string eventName){
			Graph.SendGlobalEvent( new EventData(eventName) );
		}
		///Thats the same as calling the static Graph.SendGlobalEvent
		public static void SendGlobalEvent<T>(string eventName, T eventValue){
			Graph.SendGlobalEvent( new EventData<T>(eventName, eventValue) );
		}


		
		//set the quite flag
		protected void OnApplicationQuit(){
			isQuiting = true;
		}

		//instantiate the graph reference if not local
		protected void Awake(){
			if (graphIsLocal){
				instances[graph] = graph;
			} else {
				graph = GetInstance(graph);
			}
		}

		//mark as startCalled and handle enable behaviour setting
		protected void Start(){
			startCalled = true;
			if (enableAction == EnableAction.EnableBehaviour){
				StartBehaviour();
			}
		}

		//handle enable behaviour setting
		protected void OnEnable(){
			if (startCalled && enableAction == EnableAction.EnableBehaviour){
				StartBehaviour();
			}
		}

		//handle disable behaviour setting
		protected void OnDisable(){

			if (isQuiting)
				return;

			if (disableAction == DisableAction.DisableBehaviour){
				StopBehaviour();
			}

			if (disableAction == DisableAction.PauseBehaviour){
				PauseBehaviour();
			}
		}

		//Destroy instanced graphs as well
		protected void OnDestroy(){

			if (isQuiting){
				return;
			}

			if (disableAction == DisableAction.DisableBehaviour){
				StopBehaviour();
			}

			foreach (var graph in instances.Values){
				Destroy(graph);
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR		

		[SerializeField] [HideInInspector]
		private bool hasUpdated2_1;

		//hide local IScriptableObject graph
		protected void OnValidate(){
			
			if (!hasUpdated2_1){
				hasUpdated2_1 = true;
				if (graph != null && !UnityEditor.EditorUtility.IsPersistent(graph)){
					var newLocal = (Graph)ParadoxNotion.Design.EditorUtils.AddScriptableComponent(this.gameObject, graphType);
					UnityEditor.EditorUtility.CopySerialized(graph, newLocal);
					DestroyImmediate(graph);
					graph = newLocal;
				}
			}


			if (graphIsLocal){
				graph.hideFlags = HideFlags.HideInInspector;
			}			
		}
		
		//...
		protected void Reset(){
			hasUpdated2_1 = true;
			blackboard = gameObject.GetComponent<Blackboard>();
			if (blackboard == null)
				blackboard = gameObject.AddComponent<Blackboard>();		
		}

		//forward the call
		protected void OnDrawGizmos(){
			Gizmos.DrawIcon(transform.position, "GraphOwnerGizmo.png", true);
			DoGizmos(graph);
		}

		void DoGizmos(Graph targetGraph){
			if (targetGraph != null){
				foreach (var node in targetGraph.allNodes){
					node.OnDrawGizmos();
					if (Graph.currentSelection == node){
						node.OnDrawGizmosSelected();
					}
					var graphAssignable = node as IGraphAssignable;
					if (graphAssignable != null && graphAssignable.nestedGraph != null){
						DoGizmos(graphAssignable.nestedGraph);
					}
				}
			}			
		}

		#endif
	}






	///The class where GraphOwners derive from
	abstract public class GraphOwner<T> : GraphOwner where T:Graph{

		[SerializeField] [HideInInspector]
		private T _graph;
		[SerializeField] [HideInInspector]
		private Blackboard _blackboard;
		
		///The current behaviour Graph assigned
		sealed public override Graph graph{
			get {return behaviour;}
			set {behaviour = (T)value;}
		}

		///The blackboard that the assigned behaviour will be Started with
		sealed public override IBlackboard blackboard{
			get
			{
				if (_blackboard == null){
					_blackboard = GetComponent<Blackboard>();
				}
				return _blackboard;
			}
			set
			{
				if (_blackboard != value){
					_blackboard = (Blackboard)(object)value;
					if (graph != null){
						graph.blackboard = value;
					}
				}
			}
		}
		

		///The Graph type this Owner can be assigned
		sealed public override Type graphType{ get {return typeof(T);} }

		///The current behaviour Graph assigned
		public T behaviour{
			get {return _graph;}
			set
			{
				if (_graph != value){
					_graph = (T)GetInstance(value);
				}
			}
		}


		///Start the behaviour assigned
		sealed public override void StartBehaviour(){
			behaviour = (T)GetInstance(behaviour);
			if (behaviour != null){
				behaviour.StartGraph(this, blackboard);
			}
		}

		///Start the behaviour assigned providing a callback for when it's finished if at all
		sealed public override void StartBehaviour(Action<bool> callback){
			behaviour = (T)GetInstance(behaviour);
			if (behaviour != null){
				behaviour.StartGraph(this, blackboard, callback);
			}
		}

		///Pause the current running Behaviour
		sealed public override void PauseBehaviour(){
			if (behaviour != null){
				behaviour.Pause();
			}
		}

		///Stop the current running behaviour
		sealed public override void StopBehaviour(){
			if (behaviour != null){
				behaviour.Stop();
			}
		}

		///Manually update the assigned behaviour graph
		sealed public override void UpdateBehaviour(){
			if (behaviour != null){
				behaviour.UpdateGraph();
			}
		}

		///Start a new behaviour on this owner
		public void StartBehaviour(T newGraph){
			SwitchBehaviour(newGraph);
		}

		///Start a new behaviour on this owner and get a call back for when it's finished if at all
		public void StartBehaviour(T newGraph, Action<bool> callback){
			SwitchBehaviour(newGraph, callback);
		}

		///Use to switch the behaviour dynamicaly at runtime
		public void SwitchBehaviour(T newGraph){
			SwitchBehaviour(newGraph, null);
		}

		///Use to switch or set graphs at runtime and optionaly get a callback when it's finished if at all
		public void SwitchBehaviour(T newGraph, Action<bool> callback){
			StopBehaviour();
			behaviour = newGraph;
			StartBehaviour(callback);
		}
	}
}