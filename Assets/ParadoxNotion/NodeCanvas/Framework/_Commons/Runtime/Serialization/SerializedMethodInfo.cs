using System;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace ParadoxNotion.Serialization{

	///Serialized MethodInfo
	[Serializable]
	public class SerializedMethodInfo{

		[SerializeField]
		private string _returnInfo;		
		[SerializeField]
		private string _baseInfo;
		[SerializeField]
		private string _paramsInfo;
		
		private MethodInfo _method;

		//required
		public SerializedMethodInfo(){}
		///Serialize a new MethodInfo
		public SerializedMethodInfo(MethodInfo method){
			SerializeMethod(method);
		}

		void SerializeMethod(MethodInfo method){
			_returnInfo = method.ReturnType.FullName;
			_baseInfo = string.Format("{0}|{1}", method.RTReflectedType().FullName, method.Name);
			_paramsInfo = string.Join("|", method.GetParameters().Select(p => p.ParameterType.FullName).ToArray() );			
		}

		///Deserialize and return target MethodInfo.
		public MethodInfo Get(){
			if (_method == null && _baseInfo != null){
				_method = GetOriginal();
				if (_method == null){
					_method = GetFinal();
					if (_method != null){
						SerializeMethod(_method);
					}
				}
			}

			return _method;
		}

		//original method serialized
		MethodInfo GetOriginal(){

			var type = ReflectionTools.GetType(_baseInfo.Split('|')[0]);
			if (type == null){
				return null;
			}

			var name = _baseInfo.Split('|')[1];
			var paramTypeNames = string.IsNullOrEmpty(_paramsInfo)? null : _paramsInfo.Split('|');
			var parameterTypes = paramTypeNames == null? new Type[0] : paramTypeNames.Select(n => ReflectionTools.GetType(n)).ToArray();

			var method = type.RTGetMethod(name, parameterTypes);

			if (!string.IsNullOrEmpty(_returnInfo)){ //it might be in case of older serialzations
				var returnType = ReflectionTools.GetType(_returnInfo);
				if (method != null && returnType != method.ReturnType){
					method = null;
				}
			}

			return method;
		}

		//resolve to final method. This basicaly means:
		//just get the first possible found method, without taking into account parameter types or return type.
		MethodInfo GetFinal(){

			var type = ReflectionTools.GetType(_baseInfo.Split('|')[0]);
			if (type == null){
				return null;
			}
			var name = _baseInfo.Split('|')[1];
			//to avoid ambiguous
			return type.RTGetMethods().Where(m => m.Name == name).FirstOrDefault();
		}


		//Are the original and finaly resolve methods different?
		public bool HasChanged(){
			#if UNITY_EDITOR //it can only be changed in editor really, and we avoid unessescary reflection.
			return GetOriginal() == null;
			#else
			return false;
			#endif
		}

		///Returns the serialized method information.
		public string GetMethodString(){
			return string.Format("{0} ({1})", _baseInfo.Replace("|", "."), _paramsInfo.Replace("|", ", "));
		}
	}
}