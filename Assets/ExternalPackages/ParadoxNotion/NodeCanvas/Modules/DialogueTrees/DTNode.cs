using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees{

	/// <summary>
	/// Super base class for DialogueTree nodes that can live within a DialogueTree Graph.
	/// </summary>
	abstract public class DTNode : Node {

		[SerializeField]
		private string _actorName = "INSTIGATOR";

		public override string name{
			get{return string.Format("#{0} {1}", ID, actorName);}
		}

		public override int maxInConnections{ get{return -1;} }
		public override int maxOutConnections{ get{return 1;} }
		sealed public override System.Type outConnectionType{ get{return typeof(DTConnection);} }
		sealed public override bool allowAsPrime {get{return true;}}
		sealed public override bool showCommentsBottom{ get{return false;}}

		protected DialogueTree DLGTree{
			get{return (DialogueTree)graph;}
		}

		///The key name actor parameter to be used for this node
		protected string actorName{
			get
			{
				if (!DLGTree.definedActorKeys.Contains(_actorName))
					return "<color=#d63e3e>*" + _actorName + "*</color>";
				return _actorName;
			}
			set
			{
				if (_actorName != value && !string.IsNullOrEmpty(value)){
					_actorName = value;
				}
			}
		}

		///The DialogueActor that will execute the node
		protected IDialogueActor finalActor{
			get	{ return DLGTree.GetActorReference(actorName); }
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeInspectorGUI(){
			GUI.backgroundColor = EditorUtils.lightBlue;
			actorName = EditorUtils.StringPopup(actorName, DLGTree.definedActorKeys, false, false);
			GUI.backgroundColor = Color.white;					
		}
		
		#endif
	}
}