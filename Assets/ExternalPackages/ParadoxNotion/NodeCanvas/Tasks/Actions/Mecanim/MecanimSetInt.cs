using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Name("Set Mecanim Int")]
	[Category("Mecanim")]
	public class MecanimSetInt : ActionTask<Animator> {

		[RequiredField]
		public BBParameter<string> parameter;
		public BBParameter<int> setTo;

		protected override string info{
			get {return "Mec.SetInt '" + parameter + "' to " + setTo;}
		}

		protected override void OnExecute(){
			agent.SetInteger(parameter.value, setTo.value);
			EndAction();
		}
	}
}