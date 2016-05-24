using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions{

	[Name("Move To Target Position")]
	[Category("Movement")]
	public class MoveToPosition : ActionTask<NavMeshAgent> {

		public BBParameter<Vector3> targetPosition;
		public BBParameter<float> speed = 3;
		public float keepDistance = 0.1f;

		private Vector3? lastRequest;

		protected override string info{
			get {return "GoTo " + targetPosition.ToString();}
		}

		protected override void OnExecute(){

			agent.speed = speed.value;
			if ( (agent.transform.position - targetPosition.value).magnitude < agent.stoppingDistance + keepDistance){
				EndAction(true);
				return;
			}

			Go();
		}

		protected override void OnUpdate(){
			Go();
		}

		void Go(){

			if (lastRequest != targetPosition.value){
				if ( !agent.SetDestination( targetPosition.value) ){
					EndAction(false);
					return;
				}
			}

			lastRequest = targetPosition.value;

			if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance)
				EndAction(true);
		}

		protected override void OnStop(){

			lastRequest = null;
			if (agent.gameObject.activeSelf)
				agent.ResetPath();
		}

		protected override void OnPause(){
			OnStop();
		}
	}
}

/*
public static float SumDistances(this Vector3[] list){
    float sum = 0f;

    // Sum for all except the end cap, since that doesn't have a distance to anything else.
    for (int i = 0; i < list.Length - 1; i++ )
    {
        sum += UnityEngine.Vector3.Distance(list[i], list[i + 1]);
    }

    return sum;
}
*/