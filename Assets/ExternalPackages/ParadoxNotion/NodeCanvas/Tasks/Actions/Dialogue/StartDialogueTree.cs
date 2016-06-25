using UnityEngine;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using NodeCanvas.DialogueTrees;

namespace NodeCanvas.Tasks.Actions{

	[Category("Dialogue")]
	[AgentType(typeof(IDialogueActor))]
	[Description("Starts a Dialogue Tree with specified agent for 'Instigator'\nPlease Drag & Drop the DialogueTree Component rather than the GameObject for assignement")]
	[Icon("Dialogue")]
	public class StartDialogueTree : ActionTask {

		[RequiredField]
		public BBParameter<DialogueTree> dialogueTree;
		public bool waitActionFinish = true;

		protected override string info{
			get {return string.Format("Start Dialogue {0}", dialogueTree.ToString());}
		}

		protected override void OnExecute(){
			
			var actor = (IDialogueActor)agent;
			if (waitActionFinish){
				dialogueTree.value.StartDialogue(actor, (success)=> {EndAction(success);} );
			} else {
				dialogueTree.value.StartDialogue(actor);
				EndAction();
			}
		}
	}
}