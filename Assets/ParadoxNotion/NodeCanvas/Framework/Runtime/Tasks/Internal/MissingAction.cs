using System.Linq;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Framework.Internal{

	/// <summary>
	/// Injected when an ActionTask is missing. Recovers back when that ActionTask is found.
	/// </summary>
    [DoNotList]
	[Description("Please resolve the MissingTask issue by either replacing the task or importing the missing task type in the project")]
	public class MissingAction : ActionTask {

		public string missingType;
		public string recoveryState;

		protected override string info{
			get { return string.Format("<color=#ff6457>* {0} *</color>", missingType.Split('.').Last()); }
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnTaskInspectorGUI(){
			GUILayout.Label(missingType);
		}

		#endif
	}
}