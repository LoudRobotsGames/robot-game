using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Blackboard/Lists")]
	public class PickListElement<T> : ActionTask{
		[RequiredField] [BlackboardOnly]
		public BBParameter<List<T>> targetList;
		public BBParameter<int> index;
		[BlackboardOnly]
        public BBParameter<T> saveAs;

		protected override void OnExecute(){

			if (index.value < 0 || index.value >= targetList.value.Count){
				EndAction(false);
				return;
			}

			saveAs.value = targetList.value[index.value];
			EndAction(true);			
		}
	}
}