using System.Linq;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework.Internal{

    ///Missing node types are deserialized into this on deserialization and can load back if type is found
    [DoNotList]
	[Description("Please resolve the MissingNode issue by either replacing the node or importing the missing node type in the project")]
	sealed public class MissingNode : Node {

		public string missingType;
		public string recoveryState;

		public override string name{
			get { return string.Format("<color=#ff6457>{0}</color>", "* Missing Node *"); }
		}

		public override System.Type outConnectionType{ get {return null;} }
		public override int maxInConnections{ get {return 0;} }
		public override int maxOutConnections{ get {return 0;} }
		public override bool allowAsPrime{ get {return false;} }
		public override bool showCommentsBottom{ get {return false;} }
		


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnNodeGUI(){
			//removes assembly stuff in case of generic and show
			GUILayout.Label(string.Format("<color=#ff6457>* {0} *</color>", Strip() ));
		}

		protected override void OnNodeInspectorGUI(){
			GUILayout.Label(Strip());
		}

		string Strip(){
			return missingType.Substring(0, missingType.Contains("[")? missingType.IndexOf("[") : missingType.Length );
		}

		#endif
	}
}