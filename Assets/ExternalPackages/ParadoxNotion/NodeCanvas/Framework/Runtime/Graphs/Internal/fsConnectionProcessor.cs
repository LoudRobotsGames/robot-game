using System;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Framework.Internal{

	///Handles missing Connection serialization and recovery
	public class fsConnectionProcessor : fsObjectProcessor {

		public override bool CanProcess(Type type){
			return typeof(Connection).RTIsAssignableFrom(type);
		}

		public override void OnBeforeSerialize(Type storageType, object instance){}
		public override void OnAfterSerialize(Type storageType, object instance, ref fsData data){}

		public override void OnBeforeDeserialize(Type storageType, ref fsData data){

			if (data.IsNull){
				return;
			}

			var json = data.AsDictionary;

			fsData typeData;
			if (json.TryGetValue("$type", out typeData)){

				var serializedType = ReflectionTools.GetType( typeData.AsString );

				if (serializedType == null || serializedType == typeof(MissingConnection)){
					//TargetType is either the one missing or the one previously stored as missing in the MissingConnection.
					var targetFullTypeName = serializedType == null? typeData.AsString : json["missingType"].AsString;
					//Try find type with same name in some other namespace that is subclass of Connection
					var typeNameWithoutNS = targetFullTypeName.Split('.').LastOrDefault();
					foreach(var type in ReflectionTools.GetAllTypes()){
						if (type.Name == typeNameWithoutNS && type.IsSubclassOf(typeof(NodeCanvas.Framework.Connection))){
							json["$type"] = new fsData(type.FullName);
							return;
						}
					}
				}

				//Handle missing serialized Connection type
				if (serializedType == null){
					//inject the 'MissingConnection' type and store recovery serialization state.
					//recoveryState and missingType are serializable members of MissingConnection.
					json["recoveryState"] = new fsData( data.ToString() );
					json["missingType"] = new fsData( typeData.AsString );
					json["$type"] = new fsData( typeof(MissingConnection).FullName );
				}

				//Recover possible found serialized type
				if (serializedType == typeof(MissingConnection)){

					//Does the missing type now exists? If so recover
					var missingType = ReflectionTools.GetType( json["missingType"].AsString );
					if (missingType != null){

						var recoveryState = json["recoveryState"].AsString;
						var recoverJson = fsJsonParser.Parse(recoveryState).AsDictionary;

						//merge the recover state *ON TOP* of the current state, thus merging only Declared recovered members
						json = json.Concat( recoverJson.Where( kvp => !json.ContainsKey(kvp.Key) ) ).ToDictionary( c => c.Key, c => c.Value );
						json["$type"] = new fsData( missingType.FullName );
						data = new fsData( json );
					}
				}
			}
		}

		public override void OnAfterDeserialize(Type storageType, object instance){}
	}
}