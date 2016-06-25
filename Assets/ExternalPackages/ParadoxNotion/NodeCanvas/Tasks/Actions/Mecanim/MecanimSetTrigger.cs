using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Name("Set Mecanim Trigger")]
	[Category("Mecanim")]
	public class MecanimSetTrigger : ActionTask<Animator> {

		[RequiredField]
		public BBParameter<string> parameter;

		protected override string info{
			get{return "Mec.SetTrigger " + parameter;}
		}

		protected override void OnExecute(){

			agent.SetTrigger(parameter.value);
			EndAction();
		}
	}
}