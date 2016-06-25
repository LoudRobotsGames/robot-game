using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Name("Set Mecanim Float")]
	[Category("Mecanim")]
	public class MecanimSetFloat : ActionTask<Animator> {

		[RequiredField]
		public BBParameter<string> parameter;
		public BBParameter<float> setTo;
		[SliderField(0,1)]
		public float transitTime = 0.25f;

		private float currentValue;

		protected override string info{
			get {return "Mec.SetFloat " + parameter + " to " + setTo.ToString();}
		}

		protected override void OnExecute(){

			if (transitTime <= 0){
				agent.SetFloat(parameter.value, setTo.value);
				EndAction();
				return;
			}

			currentValue = agent.GetFloat(parameter.value);
		}

		protected override void OnUpdate(){

			agent.SetFloat(parameter.value, Mathf.Lerp(currentValue, setTo.value, elapsedTime/transitTime));
			if (elapsedTime >= transitTime)
				EndAction(true);
		}
	}
}