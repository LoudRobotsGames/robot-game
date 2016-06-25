#if UNITY_EDITOR

using System.Linq;
using NodeCanvas.DialogueTrees;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;


namespace NodeCanvas.Editor{

	[CustomEditor(typeof(DialogueTree))]
	public class DialogueTreeInspector : GraphInspector{

		private DialogueTree DLGTree{
			get {return target as DialogueTree;}
		}

		public override void OnInspectorGUI(){

			base.OnInspectorGUI();

            EditorUtils.TitledSeparator("Dialogue Actor Parameters");
			EditorGUILayout.HelpBox("Enter the Key-Value pair for Dialogue Actors involved in this Dialogue Tree.\nReferencing a DialogueActor is optional", MessageType.Info);

			GUILayout.BeginVertical("box");

			if (GUILayout.Button("Add Actor Parameter"))
				DLGTree.actorParameters.Add(new DialogueTree.ActorParameter("actor parameter name"));
			
			EditorGUILayout.LabelField("INSTIGATOR <--Replaced by the Actor starting the Dialogue");

			for (var i = 0; i < DLGTree.actorParameters.Count; i++){
				var reference = DLGTree.actorParameters[i];
				GUILayout.BeginHorizontal();
				if (DLGTree.actorParameters.Where(r => r != reference).Select(r => r.name).Contains(reference.name))
					GUI.backgroundColor = Color.red;
				reference.name = EditorGUILayout.TextField(reference.name);
				GUI.backgroundColor = Color.white;
				reference.actor = (IDialogueActor)EditorGUILayout.ObjectField(reference.actor as Object, typeof(DialogueActor), true);
				if (GUILayout.Button("X", GUILayout.Width(18)))
					DLGTree.actorParameters.Remove(reference);
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();

			EditorUtils.EndOfInspector();

			if (Application.isPlaying && GUILayout.Button("Debug Start Dialogue")){
				DLGTree.StartDialogue();
			}

			if (GUI.changed)
				EditorUtility.SetDirty(DLGTree);
		}
	}
}

#endif