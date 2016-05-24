using UnityEngine;
using UnityEditor;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace NodeCanvas.Editor{

	static class Commands {

		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Create/Scene Global Blackboard")]
		public static void CreateGlobalBlackboard(){
			Selection.activeObject = GlobalBlackboard.Create();
		}

		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Create/Standalone Action List")]
		static void CreateActionListPlayer(){
			Selection.activeObject = ActionListPlayer.Create();
		}

		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Create/New Task")]
		[MenuItem("Assets/Create/ParadoxNotion/NodeCanvas/New Task")]
		public static void ShowTaskWizard(){
			TaskWizardWindow.ShowWindow();
		}


		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Preferred Types Editor")]
		public static void ShowPrefTypes(){
			PreferedTypesEditorWindow.ShowWindow();
		}

	    [MenuItem("Tools/ParadoxNotion/NodeCanvas/External Inspector Panel")]
	    public static void ShowExternalInspector(){
	    	ExternalInspectorWindow.ShowWindow();
	    }

		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Welcome Window")]
		public static void ShowWelcome(){
			WelcomeWindow.ShowWindow(null);
		}

		[MenuItem("Tools/ParadoxNotion/NodeCanvas/Visit Website")]
		public static void VisitWebsite(){
			Help.BrowseURL("http://nodecanvas.paradoxnotion.com");
		}
	}
}