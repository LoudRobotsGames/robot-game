using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Blackboard/Lists")]
	public class AddElementToList<T> : ActionTask{
		[RequiredField] [BlackboardOnly]
		public BBParameter<List<T>> targetList;
		public BBParameter<T> targetElement;

		protected override void OnExecute(){
			targetList.value.Add(targetElement.value);
			EndAction();
		}
	}
}