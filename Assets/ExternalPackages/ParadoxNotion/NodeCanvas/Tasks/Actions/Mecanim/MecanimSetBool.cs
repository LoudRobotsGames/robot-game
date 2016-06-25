using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Name("Set Mecanim Bool")]
	[Category("Mecanim")]
	public class MecanimSetBool : ActionTask<Animator> {

		[RequiredField]
		public BBParameter<string> parameter;
		public BBParameter<bool> setTo;

		protected override string info{
			get{return "Mec.SetBool '" + parameter.ToString() + "' to " + setTo;}
		}

		protected override void OnExecute(){

			agent.SetBool(parameter.value, (bool)setTo.value);
			EndAction(true);
		}
	}
}