using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Conditions{

	[Category("✫ Script Control/Standalone Only")]
	[Description("Check a property on a script and return if it's equal or not to the check value")]
	public class CheckProperty : ConditionTask, ISubParametersContainer {

		[SerializeField] /*[IncludeParseVariables]*/
		protected ReflectedFunctionWrapper functionWrapper;
		[SerializeField]
		protected BBParameter checkValue;
		[SerializeField]
		protected CompareMethod comparison;

		BBParameter[] ISubParametersContainer.GetIncludeParseParameters(){
			return functionWrapper != null? functionWrapper.GetVariables() : null;
		}

		private MethodInfo targetMethod{
			get {return functionWrapper != null? functionWrapper.GetMethod() : null;}
		}

		public override System.Type agentType{
			get {return targetMethod != null? targetMethod.RTReflectedType() : typeof(Transform);}
		}

		protected override string info{
			get
			{
				if (functionWrapper == null)
					return "No Property Selected";
				if (targetMethod == null)
					return string.Format("<color=#ff6457>* {0} *</color>", functionWrapper.GetMethodString() );
				return string.Format("{0}.{1}{2}", agentInfo, targetMethod.Name, OperationTools.GetCompareString(comparison) + checkValue.ToString());
			}
		}

		//store the method info on agent set for performance
		protected override string OnInit(){

			if (targetMethod == null)
				return "CheckProperty Error";

			try
			{
				functionWrapper.Init(agent);
				return null;
			}
			catch {return "CheckProperty Error";}
		}

		//do it by invoking method
		protected override bool OnCheck(){

			if (functionWrapper == null)
				return true;

			if (checkValue.varType == typeof(float))
				return OperationTools.Compare( (float)functionWrapper.Call(), (float)checkValue.value, comparison, 0.05f );
			if (checkValue.varType == typeof(int))
				return OperationTools.Compare( (int)functionWrapper.Call(), (int)checkValue.value, comparison);
			return object.Equals( functionWrapper.Call(), checkValue.value );			
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnTaskInspectorGUI(){

			if (!Application.isPlaying && GUILayout.Button("Select Property")){
				System.Action<MethodInfo> MethodSelected = (method)=> {
					functionWrapper = ReflectedFunctionWrapper.Create(method, blackboard);
					checkValue = BBParameter.CreateInstance( method.ReturnType, blackboard);
					comparison = CompareMethod.EqualTo;
				};

				if (agent != null){
					EditorUtils.ShowGameObjectMethodSelectionMenu(agent.gameObject, typeof(object), typeof(object), MethodSelected, 0, true, true);
				} else {
					var menu = new UnityEditor.GenericMenu();
					foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(Component), true))
						menu = EditorUtils.GetMethodSelectionMenu(t, typeof(object), typeof(object), MethodSelected, 0, true, true, menu);
					menu.ShowAsContext();
					Event.current.Use();
				}				
			}			

			if (targetMethod != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Type", agentType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Property", targetMethod.Name);
				GUILayout.EndVertical();

				GUI.enabled = checkValue.varType == typeof(float) || checkValue.varType == typeof(int);
				comparison = (CompareMethod)UnityEditor.EditorGUILayout.EnumPopup("Comparison", comparison);
				GUI.enabled = true;
				EditorUtils.BBParameterField("Value", checkValue);
			}
		}

		#endif
	}
}