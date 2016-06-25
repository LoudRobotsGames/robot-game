using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Category("GameObject")]
	public class InstantiateGameObject : ActionTask<Transform> {

		public BBParameter<Vector3> clonePosition;
		[BlackboardOnly]
		public BBParameter<GameObject> saveCloneAs;

		protected override string info{
			get {return "Instantiate " + agentInfo + " at " + clonePosition + " as " + saveCloneAs;}
		}

		protected override void OnExecute(){

			saveCloneAs.value = (GameObject)Object.Instantiate(agent.gameObject, clonePosition.value, Quaternion.identity);
			EndAction();
		}
	}
}