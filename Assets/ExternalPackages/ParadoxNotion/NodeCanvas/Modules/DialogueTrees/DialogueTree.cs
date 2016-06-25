using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees{

	/// <summary>
	/// Use DialogueTrees to create Dialogues between Actors
	/// </summary>
	[GraphInfo(
		packageName = "NodeCanvas",
		docsURL = "http://nodecanvas.paradoxnotion.com/documentation/",
		resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/",
		forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/"
		)]
	public class DialogueTree : Graph {

		[System.Serializable]
		public class ActorParameter {
			
			[SerializeField]
			private string _keyName;
			[SerializeField]
			private UnityEngine.Object _actorObject;
			private IDialogueActor _actor;
			
			public string name{
				get {return _keyName;}
				set {_keyName = value;}
			}

			public IDialogueActor actor{
				get
				{
					if (_actor == null)
						_actor = _actorObject as IDialogueActor;
					return _actor;
				}
				set
				{
					_actor = value;
					_actorObject = value as UnityEngine.Object;
				}
			}

			public ActorParameter(){}
			public ActorParameter(string name){
				this.name = name;
			}
			public ActorParameter(string name, IDialogueActor actor){
				this.name = name;
				this.actor = actor;
			}
		}

		[SerializeField]
		private List<ActorParameter> _actorParameters = new List<ActorParameter>();
		private DTNode currentNode;


		public static event Action<DialogueTree> OnDialogueStarted;
		public static event Action<DialogueTree> OnDialoguePaused;
		public static event Action<DialogueTree> OnDialogueFinished;
		public static event Action<SubtitlesRequestInfo> OnSubtitlesRequest;
		public static event Action<MultipleChoiceRequestInfo> OnMultipleChoiceRequest;
		public static DialogueTree currentDialogue;


		public override System.Type baseNodeType{ get {return typeof(DTNode);} }
		public override bool requiresAgent{	get {return false;} }
		public override bool requiresPrimeNode { get {return true;} }
		public override bool autoSort{ get {return true;} }
		public override bool useLocalBlackboard{get {return true;}}


		///The dialogue actor parameters
		public List<ActorParameter> actorParameters{
			get {return _actorParameters;}
		}


		//A list of the defined keycodes for the involved actor parameters
		public List<string> definedActorKeys{
			get
			{
				var list = actorParameters.Select(r => r.name).ToList();
				list.Insert(0, "INSTIGATOR");
				return list;
			}
		}

		///Resolves and gets an actor based on the key name
		public IDialogueActor GetActorReference(string keyName){
			
			//Check for INSTIGATOR selection
			if (keyName == "INSTIGATOR"){
				
				//return it directly if it implements IDialogueActor
				if (agent is IDialogueActor)
					return (IDialogueActor)agent;

				//Otherwise use the default actor and set name and transform from agent
				if (agent != null)
					return new ProxyDialogueActor(agent.gameObject.name, agent.transform);
				
				return new ProxyDialogueActor("Null Instigator", null);
			}

			//Check for non "INSTIGATOR" selection. If there IS an actor reference return it
			var refData = actorParameters.Find(r => r.name == keyName);
			if (refData != null && refData.actor != null)
				return refData.actor;

			//Otherwise use the default actor and set the name to the key and null transform
			Debug.Log(string.Format("<b>DialogueTree:</b> An actor entry '{0}' on DialogueTree has no reference. A dummy Actor will be used with the entry Key for name", keyName), this);
			return new ProxyDialogueActor(keyName, null);
		}


		///Set the target IDialogueActor for the provided key parameter
		public void SetActorReference(string keyName, IDialogueActor actor){
			var reference = actorParameters.Find(r => r.name == keyName);
			if (reference == null){
				Debug.LogError(string.Format("There is no defined Actor key name '{0}'", keyName));
				return;
			}
			reference.actor = actor;
		}

		///Set all target IDialogueActors at once by provided dictionary
		public void SetActorReferences(Dictionary<string, IDialogueActor> actors){
			foreach (var pair in actors){
				var reference = actorParameters.Find(r => r.name == pair.Key);
				if (reference == null){
					Debug.LogWarning(string.Format("There is no defined Actor key name '{0}'. Seting actor skiped", pair.Key));
					continue;
				}
				reference.actor = pair.Value;
			}
		}


		///Continues the DialogueTree at provided child connection index of currentNode
		public void Continue(int index = 0){

			if (!isRunning){
				return;
			}

			if (index < 0 || index > currentNode.outConnections.Count-1){
				Stop(true);
				return;
			}
			
			EnterNode( (DTNode)currentNode.outConnections[index].targetNode );
		}

		///Enters the provided node
		public void EnterNode(DTNode node){
			Debug.Log(node.ID);
			currentNode = node;
			currentNode.Reset(false);
			if (currentNode.Execute(agent, blackboard) == Status.Error ){
				Stop(false);
			}
		}




		///Raise the OnSubtitlesRequest event
		public static void RequestSubtitles(SubtitlesRequestInfo info){
			if (OnSubtitlesRequest != null)
				OnSubtitlesRequest(info);
			else Debug.LogWarning("<b>DialogueTree:</b> Subtitle Request event has no subscribers. Make sure to add the default '@DialogueGUI' prefab or create your own GUI.");
		}

		///Raise the OnMultipleChoiceRequest event
		public static void RequestMultipleChoices(MultipleChoiceRequestInfo info){
			if (OnMultipleChoiceRequest != null)
				OnMultipleChoiceRequest(info);
			else Debug.LogWarning("<b>DialogueTree:</b> Multiple Choice Request event has no subscribers. Make sure to add the default '@DialogueGUI' prefab or create your own GUI.");
		}


		///Convenience starting Methods

		///Start the DialogueTree without an Instigator
		public void StartDialogue(){
			StartGraph(null, localBlackboard, null);
		}

		///Start the DialogueTree with provided actor as Instigator
		public void StartDialogue(IDialogueActor instigator){
			StartGraph(instigator is Component? (Component)instigator : instigator.transform, localBlackboard, null);
		}

		///Start the DialogueTree with provded actor as instigator and callback
		public void StartDialogue(IDialogueActor instigator, Action<bool> callback){
			StartGraph(instigator is Component? (Component)instigator : instigator.transform, localBlackboard, callback );
		}

		///Start the DialogueTree with a callback for when its finished
		public void StartDialogue(Action<bool> callback){
			StartGraph(null, localBlackboard, callback);
		}


		////

		protected override void OnGraphStarted(){

			if (currentDialogue != null){
				Debug.LogWarning(string.Format("<b>DialogueTree:</b> Another Dialogue Tree named '{0}' is already running and will be stoped before starting new one '{1}'", currentDialogue.name, this.name ));
				currentDialogue.Stop(true);
			}

			currentDialogue = this;

			Debug.Log(string.Format("<b>DialogueTree:</b> Dialogue Started '{0}'", this.name));
			if (OnDialogueStarted != null)
				OnDialogueStarted(this);

			if ( !(agent is IDialogueActor) ){
				Debug.Log("<b>DialogueTree:</b> INSTIGATOR agent used in DialogueTree does not implement IDialogueActor. A dummy actor will be used.");
			}

			currentNode = currentNode != null? currentNode : (DTNode)primeNode;
			EnterNode( currentNode );
		}

		protected override void OnGraphStoped(){

			currentNode = null;
			currentDialogue = null;

			Debug.Log(string.Format("<b>DialogueTree:</b> Dialogue Finished '{0}'", this.name));
			if (OnDialogueFinished != null)
				OnDialogueFinished(this);
		}

		protected override void OnGraphPaused(){

			Debug.Log(string.Format("<b>DialogueTree:</b> Dialogue Paused '{0}'", this.name));
			if (OnDialoguePaused != null)
				OnDialoguePaused(this);
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		[UnityEditor.MenuItem("Tools/ParadoxNotion/NodeCanvas/Create/Dialogue Tree", false, 0)]
		public static void Editor_CreateGraph(){
			var newGraph = EditorUtils.AddScriptableComponent<DialogueTree>( new GameObject("DialogueTree") );
			UnityEditor.Selection.activeObject = newGraph;
		}

/*		
		[UnityEditor.MenuItem("Window/NodeCanvas/Create/Graph/Dialogue Tree")]
		public static void Editor_CreateGraph(){
			var newGraph = EditorUtils.CreateAsset<DialogueTree>(true);
			UnityEditor.Selection.activeObject = newGraph;
		}

		[UnityEditor.MenuItem("Assets/Create/NodeCanvas/Dialogue Tree")]
		public static void Editor_CreateGraphFix(){
			var path = EditorUtils.GetAssetUniquePath("DialogueTree.asset");
			var newGraph = EditorUtils.CreateAsset<DialogueTree>(path);
			UnityEditor.Selection.activeObject = newGraph;
		}	
*/		
		#endif
	}
}