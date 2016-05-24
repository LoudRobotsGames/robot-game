using System.Linq;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework.Internal{

    ///Missing node types are deserialized into this on deserialization and can load back if type is found
    [DoNotList]
	sealed public class MissingConnection : Connection {

		public string missingType;
		public string recoveryState;

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnConnectionInspectorGUI(){
			var text = missingType.Substring(0, missingType.Contains("[")? missingType.IndexOf("[") : missingType.Length );
			GUILayout.Label(text);
		}

		#endif
	}
}