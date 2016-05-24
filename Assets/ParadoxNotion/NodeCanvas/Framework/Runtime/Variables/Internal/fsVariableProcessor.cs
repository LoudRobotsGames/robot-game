using System;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;


namespace NodeCanvas.Framework.Internal {

	///Handles missing variable types deserialzation. These are resolved to Variable<object>
	public class fsVariableProcessor : fsObjectProcessor {

		public override bool CanProcess(Type type){
			return typeof(Variable).RTIsAssignableFrom(type);
		}

		public override void OnBeforeSerialize(Type storageType, object instance){}
		public override void OnAfterSerialize(Type storageType, object instance, ref fsData data){}

		public override void OnBeforeDeserialize(Type storageType, ref fsData data){

			if (data.IsNull)
				return;

			var json = data.AsDictionary;

			if (json.ContainsKey("$type")){

				var serializedType = ReflectionTools.GetType( json["$type"].AsString );
				if (serializedType == null){
					json["$type"] = new fsData( typeof(Variable<object>).FullName );
				}
			}
		}

		public override void OnAfterDeserialize(Type storageType, object instance){}
	}
}