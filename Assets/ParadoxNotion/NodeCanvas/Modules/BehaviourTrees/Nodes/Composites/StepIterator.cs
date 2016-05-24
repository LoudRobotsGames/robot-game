using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.BehaviourTrees{

	[Category("Composites")]
	[Description("Executes AND immediately returns children node status ONE-BY-ONE. Step Iterator always moves forward by one and loops it's index")]
	[Icon("StepIterator")]
	public class StepIterator : BTComposite {

		private int current;

		public override string name{
			get{return string.Format("<color=#bf7fff>{0}</color>", base.name.ToUpper());}
		}

		protected override Status OnExecute(Component agent, IBlackboard blackboard){
			current = current % outConnections.Count;
			return outConnections[current].Execute(agent, blackboard);
		}

		protected override void OnReset(){
			current += 1;
		}
	}
}