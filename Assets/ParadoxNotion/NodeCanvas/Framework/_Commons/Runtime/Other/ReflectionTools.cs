using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace ParadoxNotion{

	///Helper extension methods to work with NETFX_CORE as well as some other reflection helper extensions and utilities
	public static class ReflectionTools {

		private static List<Assembly> _loadedAssemblies;
		private static List<Assembly> loadedAssemblies{
        	get
        	{
        		if (_loadedAssemblies == null){

	        		#if NETFX_CORE

				    _loadedAssemblies = new List<Assembly>();
		 		    var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
				    var folderFilesAsync = folder.GetFilesAsync();
				    folderFilesAsync.AsTask().Wait();

				    foreach (var file in folderFilesAsync.GetResults()){
				        if (file.FileType == ".dll" || file.FileType == ".exe"){
				            try
				            {
				                var filename = file.Name.Substring(0, file.Name.Length - file.FileType.Length);
				                AssemblyName name = new AssemblyName { Name = filename };
				                Assembly asm = Assembly.Load(name);
				                _loadedAssemblies.Add(asm);
				            }
				            catch (BadImageFormatException)
				            {
				                // Thrown reflecting on C++ executable files for which the C++ compiler stripped the relocation addresses
				                continue;
				            }
				        }
				    }

	        		#else

	        		_loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

	        		#endif
	        	}

	        	return _loadedAssemblies;
        	}
        }

		private static IEnumerable GetBaseTypes(Type type){
			
			yield return type;
			Type baseType;

			#if NETFX_CORE
			baseType = type.GetTypeInfo().BaseType;
			#else
			baseType = type.BaseType;
			#endif

			if (baseType != null){
				foreach (var t in GetBaseTypes(baseType)){
					yield return t;
				}
			}
		}

		//Alternative to Type.GetType to work with FullName instead of AssemblyQualifiedName when looking up a type by string
		public static Type GetType(string typeName){

			Type type = null;
			type = Type.GetType(typeName);
			if (type != null){
				return type;
			}

            foreach (var asm in loadedAssemblies) {
                type = asm.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }

            return null;
		}

		///Get every single type in loaded assemblies
		public static Type[] GetAllTypes(){
			var result = new List<Type>();
			foreach (var asm in loadedAssemblies){
				result.AddRange(asm.GetTypes());
			}
			return result.ToArray();
		}

		///Get a friendly name for the type
		public static string FriendlyName(this Type t, bool trueSignature = false){

			if (t == null){
				return null;
			}

			if (!trueSignature && t == typeof(UnityEngine.Object)){
				return "UnityObject";
			}

			var s = trueSignature? t.FullName : t.Name;
			if (!trueSignature){
				s = s.Replace("Single", "Float");
				s = s.Replace("Int32", "Integer");
			}

			if ( t.RTIsGenericParameter() ){
				s = "T";
			}

			if ( t.RTIsGenericType() ){
				
				s = (trueSignature? t.Namespace + "." : "") + t.Name;

				var args= t.RTGetGenericArguments();
				
				if (args.Length != 0){
				
					s = s.Replace("`" + args.Length.ToString(), "");

					s += "<";
					for (var i= 0; i < args.Length; i++)
						s += (i == 0? "":", ") + args[i].FriendlyName(trueSignature);
					s += ">";
				}
			}

			return s;			
		}

		///Get a full signature string name for a method
		public static string SignatureName(this MethodInfo method){
			var parameters = method.GetParameters();
			var methodName = (method.IsStatic? "static " : "") + method.Name + " (";
			for (var i = 0; i < parameters.Length; i++){
				var p = parameters[i];
				methodName += (p.IsOut? "out " : "") + p.ParameterType.FriendlyName() + (i < parameters.Length-1? ", " : "");
			}
			methodName += ") : " + method.ReturnType.FriendlyName();
			return methodName;
		}


		public static Type RTReflectedType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().DeclaringType;
			#else
			return type.ReflectedType;
			#endif			
		}

		public static Type RTReflectedType(this MemberInfo member){
			#if NETFX_CORE
			return member.DeclaringType; //no way to get ReflectedType here that I know of...
			#else
			return member.ReflectedType;
			#endif						
		}


		public static bool RTIsAssignableFrom(this Type type, Type second){
			#if NETFX_CORE
			return type.GetTypeInfo().IsAssignableFrom(second.GetTypeInfo());
			#else
			return type.IsAssignableFrom(second);
			#endif
		}

		public static bool RTIsAbstract(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsAbstract;
			#else
			return type.IsAbstract;
			#endif			
		}

		public static bool RTIsValueType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsValueType;
			#else
			return type.IsValueType;
			#endif						
		}

		public static bool RTIsArray(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsArray;
			#else
			return type.IsArray;
			#endif			
		}

		public static bool RTIsInterface(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsInterface;
			#else
			return type.IsInterface;
			#endif			
		}

		public static bool RTIsSubclassOf(this Type type, Type other){
			#if NETFX_CORE
			return type.GetTypeInfo().IsSubclassOf(other);
			#else
			return type.IsSubclassOf(other);
			#endif						
		}

		public static bool RTIsGenericParameter(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsGenericParameter;
			#else
			return type.IsGenericParameter;
			#endif									
		}

		public static bool RTIsGenericType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsGenericType;
			#else
			return type.IsGenericType;
			#endif						
		}



		public static MethodInfo RTGetGetMethod(this PropertyInfo prop){
			#if NETFX_CORE
			return prop.GetMethod;
			#else
			return prop.GetGetMethod();
			#endif			
		}

		public static MethodInfo RTGetSetMethod(this PropertyInfo prop){
			#if NETFX_CORE
			return prop.SetMethod;
			#else
			return prop.GetSetMethod();
			#endif
		}

		public static FieldInfo RTGetField(this Type type, string name, bool includePrivate = false){
			#if NETFX_CORE
			var fields = GetBaseTypes(type).OfType<Type>().Select(baseType => baseType.GetTypeInfo().DeclaredFields).ToList();
			foreach (FieldInfo f in fields){
				if (f.Name == name){
					if (f.IsPrivate && includePrivate)
						return f;
					if (f.IsPublic)
						return f;
				}
			}
			return null;
			#else
			if (includePrivate)
				return type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
			#endif
		}

		public static PropertyInfo RTGetProperty(this Type type, string name){
			#if NETFX_CORE
			return GetBaseTypes(type).OfType<Type>().Select(baseType => baseType.GetTypeInfo().GetDeclaredProperty(name)).FirstOrDefault(property => property != null);
			#else
			return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
			#endif
		}

		public static MethodInfo RTGetMethod(this Type type, string name, bool includePrivate = false){

			#if NETFX_CORE
			var methods = GetBaseTypes(type).OfType<Type>().Select(baseType => baseType.GetTypeInfo().DeclaredMethods).ToList();
			foreach (MethodInfo[] m in methods){
				foreach (MethodInfo j in m){
					if (j.Name == name){
						if (j.IsPrivate && includePrivate)
							return j;
						if (j.IsPublic)
							return j;
					}
				}
			}
			return null;

			#else
			if (includePrivate)
				return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
			#endif
		}

		public static MethodInfo RTGetMethod(this Type type, string name, Type[] paramTypes){
			#if NETFX_CORE
			return type.GetTypeInfo().GetMethod(name, paramTypes);
			#else
			return type.GetMethod(name, paramTypes);
			#endif
		}

		public static EventInfo RTGetEvent(this Type type, string name){
			#if NETFX_CORE
			return GetBaseTypes(type).OfType<Type>().Select(baseType => baseType.GetTypeInfo().GetDeclaredEvent(name)).FirstOrDefault(method => method != null);
			#else
			return type.GetEvent(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
			#endif			
		}

		
		//cache the fields since it's used regularely
		private static Dictionary<Type, FieldInfo[]> _typeFields = new Dictionary<Type, FieldInfo[]>();
		public static FieldInfo[] RTGetFields(this Type type){

			FieldInfo[] fields;
			if (!_typeFields.TryGetValue(type, out fields)){

				#if NETFX_CORE
				var fieldsList = new List<FieldInfo>();
				foreach (Type t in GetBaseTypes(type).OfType<Type>())
					fieldsList.AddRange(t.GetTypeInfo().DeclaredFields);
				fields = fieldsList.ToArray();
				#else
				fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				#endif

				_typeFields[type] = fields;
			}

			return fields;
		}

		public static PropertyInfo[] RTGetProperties(this Type type){
			#if NETFX_CORE
			var props = new List<PropertyInfo>();
			foreach (Type t in GetBaseTypes(type).OfType<Type>())
				props.AddRange(t.GetTypeInfo().DeclaredProperties);
			return props.ToArray();
			#else
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			#endif
		}

		public static MethodInfo[] RTGetMethods(this Type type){

			#if NETFX_CORE
			var methods = new List<MethodInfo>();
			foreach (Type t in GetBaseTypes(type).OfType<Type>())
				methods.AddRange(t.GetTypeInfo().DeclaredMethods);
			return methods.ToArray();
			#else
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			#endif
		}



		//
		public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute {
			#if NETFX_CORE
			return (T)type.GetTypeInfo().GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#else
			return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#endif			
		}

		public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute{
			#if NETFX_CORE
			return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#else
			return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#endif			
		}
		//

		public static Type RTMakeGenericType(this Type type, Type[] typeArgs){
			#if NETFX_CORE
            return type.GetTypeInfo().MakeGenericType(typeArgs);
			#else
            return type.MakeGenericType(typeArgs);
			#endif			
		}

        public static Type[] RTGetGenericArguments(this Type type){
			#if NETFX_CORE
            return type.GetTypeInfo().GenericTypeArguments;
			#else
            return type.GetGenericArguments();
			#endif
        }

        public static Type[] RTGetEmptyTypes(){
			#if NETFX_CORE
			return new Type[0];
			#else
            return Type.EmptyTypes;
			#endif
        }


        public static T RTCreateDelegate<T>(this MethodInfo method, object instance){
			return (T)(object)method.RTCreateDelegate(typeof(T), instance);
        }

        public static Delegate RTCreateDelegate(this MethodInfo method, Type type, object instance){
			#if NETFX_CORE
			return method.CreateDelegate(type, instance);
			#else
            return Delegate.CreateDelegate(type, instance, method);
			#endif
        }




/* 	    //NOT USED anymore, but kept cause it might be usefull in the future

	    ///Creates a delegate of T for a MethodInfo with casted method parameters and return type to the specified delegate T types
	    public static T BuildDelegate<T>(MethodInfo method, params object[] missingParamValues) {
	        
	        var queueMissingParams = new Queue<object>(missingParamValues);
	        var dgtMi = typeof(T).RTGetMethod("Invoke");
	        var dgtRet = dgtMi.ReturnType;
	        var dgtParams = dgtMi.GetParameters();

	        var paramsOfDelegate = (dgtParams as IEnumerable<ParameterInfo>).Select(tp => Expression.Parameter(tp.ParameterType, tp.Name)).ToArray();

	        var methodParams = method.GetParameters();

	        if (method.IsStatic)
	        {
	            var paramsToPass = (methodParams as IEnumerable<ParameterInfo>).Select((p, i) => CreateParam(paramsOfDelegate, i, p, queueMissingParams)).ToArray();

	            var call = Expression.Call(method, paramsToPass);
	            var convertCall = Expression.Convert(call, typeof(object));
	            Expression<T> expr = null;
	            if (dgtRet == typeof(void)){
	            	expr = Expression.Lambda<T>(call, paramsOfDelegate);
            	} else {
            		expr = Expression.Lambda<T>(convertCall, paramsOfDelegate);
            	}

	            return expr.Compile();
	        }
	        else
	        {
	            var paramThis = Expression.Convert(paramsOfDelegate[0], method.DeclaringType);
	            var paramsToPass = methodParams.Select((p, i) => CreateParam(paramsOfDelegate, i + 1, p, queueMissingParams)).ToArray();

	            var call = Expression.Call(paramThis, method, paramsToPass);
	            var convertCall = Expression.Convert(call, typeof(object));
	            Expression<T> expr = null;
	            if (dgtRet == typeof(void)){
		            expr = Expression.Lambda<T>(call, paramsOfDelegate);
            	} else {
            		expr = Expression.Lambda<T>(convertCall, paramsOfDelegate);
            	}

	            return expr.Compile();
	        }
	    }

	    private static Expression CreateParam(ParameterExpression[] paramsOfDelegate, int i, ParameterInfo callParamType, Queue<object> queueMissingParams) {
	        if (i < paramsOfDelegate.Length)
	            return Expression.Convert(paramsOfDelegate[i], callParamType.ParameterType);

	        if (queueMissingParams.Count > 0)
	            return Expression.Constant(queueMissingParams.Dequeue());

	        if (callParamType.ParameterType.RTIsValueType() )
	            return Expression.Constant(Activator.CreateInstance(callParamType.ParameterType));

	        return Expression.Constant(null);
	    }
*/

	}
}