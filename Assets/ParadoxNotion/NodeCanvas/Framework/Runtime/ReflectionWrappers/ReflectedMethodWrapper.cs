using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using UnityEngine;


namespace NodeCanvas.Framework.Internal{

	///Wraps a MethodInfo with the relevant BBVariables to be called within a Reflection based Task
	abstract public class ReflectedWrapper{

		//required
		public ReflectedWrapper(){}

		[SerializeField]
		protected SerializedMethodInfo _targetMethod;

		public static ReflectedWrapper Create(MethodInfo method, IBlackboard bb){
			if (method == null) return null;
			if (method.ReturnType == typeof(void))
				return ReflectedActionWrapper.Create(method, bb);
			return ReflectedFunctionWrapper.Create(method, bb);
		}

		public void SetVariablesBB(IBlackboard bb){foreach (var bbVar in GetVariables()) bbVar.bb = bb;}
		
		public bool HasChanged(){ return _targetMethod != null? _targetMethod.HasChanged() : false; }
		public MethodInfo GetMethod(){ return _targetMethod != null? _targetMethod.Get() : null; }
		public string GetMethodString(){ return _targetMethod != null? _targetMethod.GetMethodString() : null; }

		abstract public BBParameter[] GetVariables();
		abstract public void Init(object instance);
	}

	///Wraps a MethodInfo Action with the relevant BBVariables to be commonly called within a Reflection based Task
	abstract public class ReflectedActionWrapper : ReflectedWrapper{
		
		new public static ReflectedActionWrapper Create(MethodInfo method, IBlackboard bb){
			if (method == null) return null;
			Type type = null;
			Type[] argTypes = null;
			var parameters = method.GetParameters();
			if (parameters.Length == 0) type = typeof(ReflectedAction);
			if (parameters.Length == 1) type = typeof(ReflectedAction<>);
			if (parameters.Length == 2) type = typeof(ReflectedAction<,>);
			if (parameters.Length == 3) type = typeof(ReflectedAction<,,>);
			argTypes = parameters.Select(p => p.ParameterType).ToArray();
			var o = (ReflectedActionWrapper)Activator.CreateInstance( argTypes.Length > 0? type.RTMakeGenericType(argTypes) : type );
			o._targetMethod = new SerializedMethodInfo(method);

			BBParameter.SetBBFields(o, bb);
			
			var bbParams = o.GetVariables();
			for (int i = 0; i < parameters.Length; i++){
				var p = parameters[i];
				if (p.IsOptional){
					bbParams[i].value = p.DefaultValue;
				}
			}

			return o;
		}

		abstract public void Call();
	}
	

	///Wraps a MethodInfo Function with the relevant BBVariables to be commonly called within a Reflection based Task
	abstract public class ReflectedFunctionWrapper : ReflectedWrapper{
		
		new public static ReflectedFunctionWrapper Create(MethodInfo method, IBlackboard bb){
			if (method == null) return null;
			Type type = null;
			var argTypes = new List<Type>{method.ReturnType};
			var parameters = method.GetParameters();
			if (parameters.Length == 0) type = typeof(ReflectedFunction<>);
			if (parameters.Length == 1) type = typeof(ReflectedFunction<,>);
			if (parameters.Length == 2) type = typeof(ReflectedFunction<,,>);
			if (parameters.Length == 3) type = typeof(ReflectedFunction<,,,>);
			argTypes.AddRange( parameters.Select(p => p.ParameterType) );
			var o = (ReflectedFunctionWrapper)Activator.CreateInstance( type.RTMakeGenericType(argTypes.ToArray()) );
			o._targetMethod = new SerializedMethodInfo(method);

			BBParameter.SetBBFields(o, bb);

			var bbParams = o.GetVariables();
			for (int i = 0; i < parameters.Length; i++){
				var p = parameters[i];
				if (p.IsOptional){
					bbParams[i + 1].value = p.DefaultValue; //index 0 is return value
				}
			}

			return o;
		}
		
		abstract public object Call();
	}
}