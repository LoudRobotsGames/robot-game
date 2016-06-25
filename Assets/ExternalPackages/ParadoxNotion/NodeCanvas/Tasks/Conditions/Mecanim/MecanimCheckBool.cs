using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Name("Check Mecanim Bool")]
	[Category("Mecanim")]
	public class MecanimCheckBool : ConditionTask<Animator> {

		[RequiredField]
		public BBParameter<string> parameter;
		public BBParameter<bool> value;

		protected override string info{
			get{return "Mec.Bool " + parameter.ToString() + " == " + value;}
		}

		protected override bool OnCheck(){

			return agent.GetBool(parameter.value) == value.value;
		}
	}
}