using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Actions{

	[Name("Control Graph Owner")]
	[Category("✫ Utility")]
	[Description("Start, Resume, Pause, Stop a GraphOwner's behaviour")]
	public class GraphOwnerControl : ActionTask<GraphOwner> {

		public enum Control
		{
			StartBehaviour,
			StopBehaviour,
			PauseBehaviour
		}

		public Control control = Control.StartBehaviour;
		public bool waitActionFinish = true;

		private bool isFinished = false;

		protected override string info{
			get {return agentInfo + "." + control.ToString();}
		}

		protected override void OnExecute(){
			if (control == Control.StartBehaviour){
				isFinished = false;
				if (waitActionFinish){
					agent.StartBehaviour( delegate {isFinished = true;} );
				} else {
					agent.StartBehaviour();
					EndAction();
				}
			}
		}

		protected override void OnStop(){
			if (waitActionFinish && control == Control.StartBehaviour){
				agent.StopBehaviour();
			}
		}

		//These should take place here to be called 1 frame later, in case target is the same agent.
		protected override void OnUpdate(){

			if (control == Control.StartBehaviour){
			
				if (isFinished)
					EndAction();
			
			} else if (control == Control.StopBehaviour){

				agent.StopBehaviour();
				EndAction();

			} else if (control == Control.PauseBehaviour){

				agent.PauseBehaviour();
				EndAction();
			}
		}
	}
}