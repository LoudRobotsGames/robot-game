using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees{

	[Name("SubTree")]
	[Category("Nested")]
	[Description("SubTree Node can be assigned an entire Sub BehaviorTree. The root node of that behaviour will be considered child node of this node and will return whatever it returns.\nThe SubTree can also be parametrized using Blackboard variables as normal.")]
	[Icon("BT")]
	public class SubTree : BTNode, IGraphAssignable{

		[SerializeField]
		protected BBParameter<BehaviourTree> _subTree;

		private Dictionary<BehaviourTree, BehaviourTree> instances = new Dictionary<BehaviourTree, BehaviourTree>();

		public BehaviourTree subTree{
			get {return _subTree.value;}
			set { _subTree.value = value;}
		}

		public Graph nestedGraph{
			get {return subTree;}
			set {subTree = (BehaviourTree)value;}
		}

		public override string name{
			get {return base.name.ToUpper();}
		}

		/////////
		/////////

		protected override Status OnExecute(Component agent, IBlackboard blackboard){

			if (subTree == null || subTree.primeNode == null){
				return Status.Failure;
			}

			if (status == Status.Resting){
				CheckInstance();
			}

			return subTree.Tick(agent, blackboard);
		}

		protected override void OnReset(){
			if (IsInstance(subTree) && subTree.primeNode != null){
				subTree.primeNode.Reset();
			}
		}
/*
		public override void OnGraphStarted(){
			if (subTree != null){
				CheckInstance();
				for (var i = 0; i < subTree.allNodes.Count; i++){
					subTree.allNodes[i].OnGraphStarted();
				}			
			}
		}
*/
		public override void OnGraphStoped(){
			if (IsInstance(subTree)){
				for (var i = 0; i < subTree.allNodes.Count; i++){
					subTree.allNodes[i].OnGraphStoped();
				}
			}			
		}

		public override void OnGraphPaused(){
			if (IsInstance(subTree)){
				for (var i = 0; i < subTree.allNodes.Count; i++){
					subTree.allNodes[i].OnGraphPaused();
				}
			}
		}

		bool IsInstance(BehaviourTree bt){
			return instances.Values.Contains(bt);
		}

		void CheckInstance(){
			
			if (IsInstance(subTree)){
				return;
			}

			BehaviourTree instance = null;
			if (!instances.TryGetValue(subTree, out instance)){
				instance = Graph.Clone<BehaviourTree>(subTree);
				instances[subTree] = instance;
				
				for (var i = 0; i < instance.allNodes.Count; i++){
					instance.allNodes[i].OnGraphStarted();
				}	

			}

            instance.agent = graphAgent;
		    instance.blackboard = graphBlackboard;
		    instance.UpdateReferences();
			subTree = instance;
		}

		////////////////////////////
		//////EDITOR AND GUI////////
		////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeGUI(){

			GUILayout.Label(string.Format("SubTree\n{0}", _subTree) );
			if (subTree == null){
				if (!Application.isPlaying && GUILayout.Button("CREATE NEW"))
					Node.CreateNested<BehaviourTree>(this);
			}
		}

		protected override void OnNodeInspectorGUI(){

		    EditorUtils.BBParameterField("Behaviour SubTree", _subTree);

	    	if (subTree == this.graph){
		    	Debug.LogWarning("You can't have a Graph nested to iteself! Please select another");
		    	subTree = null;
		    }

		    if (subTree != null){

		    	var defParams = subTree.GetDefinedParameters();
		    	if (defParams.Length != 0){

			    	EditorUtils.TitledSeparator("Defined SubTree Parameters");
			    	GUI.color = Color.yellow;
			    	UnityEditor.EditorGUILayout.LabelField("Name", "Type");
					GUI.color = Color.white;
			    	var added = new List<string>();
			    	foreach(var bbVar in defParams){
			    		if (!added.Contains(bbVar.name)){
				    		UnityEditor.EditorGUILayout.LabelField(bbVar.name, bbVar.varType.FriendlyName());
				    		added.Add(bbVar.name);
				    	}
			    	}
			    	if (GUILayout.Button("Check/Create Blackboard Variables")){
			    		subTree.CreateDefinedParameterVariables(graphBlackboard);
			    	}
			    }
		    }
		}

		#endif
	}
}