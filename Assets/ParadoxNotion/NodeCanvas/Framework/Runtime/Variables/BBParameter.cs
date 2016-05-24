using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ParadoxNotion;
using UnityEngine;


namespace NodeCanvas.Framework{

	///Marks the BBParameter possible to only pick values from a blackboard
	[AttributeUsage(AttributeTargets.Field)]
	public class BlackboardOnlyAttribute : Attribute{}

	///Class for Parameter Variables that allow binding to a Blackboard variable or specifying a value directly.
	[Serializable] 
    [ParadoxNotion.Design.SpoofAOT]
	abstract public class BBParameter {

		[SerializeField]
		private string _name; //null means use local _value, empty means |NONE|, anything else means use bb variable.
		[SerializeField]
		private string _targetVariableID;

		[NonSerialized]
		private IBlackboard _bb;
		[NonSerialized]
		private Variable _varRef;


		//required
		public BBParameter(){}


		///Create and return an instance of a generic BBParameter<T> with type argument provided and set to read from the specified blackboard
		public static BBParameter CreateInstance(Type t, IBlackboard bb){
			if (t == null) return null;
			var newBBParam = (BBParameter)Activator.CreateInstance( typeof(BBParameter<>).RTMakeGenericType(new Type[]{t}) );
			newBBParam.bb = bb;
			return newBBParam;
		}

		///Set the blackboard reference provided for all BBParameters and List<BBParameter> fields on the target object provided.
		public static void SetBBFields(object o, IBlackboard bb){
			var bbParams = GetObjectBBParameters(o);
			for (var i = 0; i < bbParams.Count; i++){
				bbParams[i].bb = bb;
			}
		}

		///Returns BBParameters found in target object
		public static List<BBParameter> GetObjectBBParameters(object o){
			var bbParams = new List<BBParameter>();
			var fields = o.GetType().RTGetFields();
			for (var i = 0; i < fields.Length; i++){
				var field = fields[i];

				if (field.FieldType.RTIsSubclassOf(typeof(BBParameter))){
					var value = field.GetValue(o);
					if (value == null){
						value = Activator.CreateInstance(field.FieldType);
						field.SetValue(o, value);
					}
					bbParams.Add( (BBParameter)value );
					continue;
				}


				if (typeof(IList).RTIsAssignableFrom(field.FieldType) && !field.FieldType.IsArray && typeof(BBParameter).RTIsAssignableFrom(field.FieldType.RTGetGenericArguments()[0]) ){
					var list = field.GetValue(o) as IList;
					if (list != null){
						for (var j = 0; j < list.Count; j++){
							var bbParam = (BBParameter)list[j];
							if (bbParam == null){
								bbParam = (BBParameter)Activator.CreateInstance( field.FieldType.RTGetGenericArguments()[0] );
								list[j] = bbParam;
							}
							bbParams.Add( bbParam );
						}
					}
					continue;	
				}

				if (o is ISubParametersContainer){
					var parameters = (o as ISubParametersContainer).GetIncludeParseParameters();
					if (parameters != null){
						bbParams.AddRange( parameters );
					}
				}
			}

			return bbParams;
		}

		private Variable ResolveReference(IBlackboard targetBlackboard, bool useID){
			var targetName = this.name;
			if (targetName != null && targetName.Contains("/")){
				var split = targetName.Split('/');
				targetBlackboard = GlobalBlackboard.Find(split[0]);
				targetName = split[1];
			}

			Variable result = null;
			if (targetBlackboard == null){ return null; }
			if (useID && targetVariableID != null){ result = targetBlackboard.GetVariableByID(targetVariableID); }
			if (result == null && !string.IsNullOrEmpty(targetName)){ result = targetBlackboard.GetVariable(targetName, varType); }
			return result;
		}

		///The target variable ID
		private string targetVariableID{
			get {return _targetVariableID;}
			set {_targetVariableID = value;}
		}

		///The Variable object reference if any.One is set after a get or set as well as well when SetBBFields is called
		///Setting the varRef also binds this parameter with that Variable.
		public Variable varRef{
			get {return _varRef;}
			set
			{
				if (_varRef != value){
					if (_varRef != null){
						_varRef.onNameChanged -= UpdateName; //remove old one
					}
					if (value != null){
						value.onNameChanged += UpdateName; //add new one
						UpdateName(value.name); //update name immediately
					}
					_varRef = value;
					targetVariableID = value != null? value.ID : null;
					Bind(value);
				}
			}
		}

		void UpdateName(string newName){
			if (_name.Contains("/")){ //is global
				var bbName = _name.Split('/')[0];
				newName = bbName + "/" + newName;
			}
			_name = newName;
		}

		///The blackboard to read/write from. Setting this also sets the variable reference if found
		public IBlackboard bb{
			get {return _bb;}
			set
			{
				if (_bb != value){
					_bb = value;
					varRef = value != null? ResolveReference(_bb, true) : null;
				}
			}
		}


		///The name of the Variable to read/write from. Null if not, Empty if |NONE|.
		public string name{
			get { return _name; }
			set
			{
				if (_name != value){
					_name = value;
					varRef = value != null? ResolveReference(bb, false) : null;
				}
			}
		}


		///Should the variable read from a blackboard variable?
		public bool useBlackboard{
			get { return name != null; }
			set
			{
				if (value == false){
					name = null;
				}
				if (value == true && name == null){
					name = string.Empty;
				}
			}
		}


		///Has the user selected |NONE| in the dropdown?
		public bool isNone{
			get {return name == string.Empty;}
		}

		///Is the final value null?
		public bool isNull{
			get	{ return object.Equals(objectValue, null); }
		}

		///The type of the Variable reference or null if there is no Variable referenced. The returned type is for most cases the same as 'VarType'
		public Type refType{
			get {return varRef != null? varRef.varType : null;}
		}

		///The value as object type when accessing from base class. Don't do this regularely.
		public object value{
			get {return objectValue;}
			set {objectValue = value;}
		}

		///The raw object value
		abstract protected object objectValue{get;set;}
		///The type of the value that this BBParameter holds
		abstract public Type varType{get;}
		///Bind the BBParameter to target. Null unbinds.
		abstract protected void Bind(Variable data);

		public override string ToString(){
			if (isNone)
				return "<b>NONE</b>";
			if (useBlackboard)
				return string.Format("<b>${0}</b>", name);
			if (isNull)
				return "<b>NULL</b>";
			if (objectValue is IList)
				return string.Format("<b>{0}</b>", varType.FriendlyName());
			if (objectValue is IDictionary)
				return string.Format("<b>{0}</b>", varType.FriendlyName());
			return string.Format("<b>{0}</b>", objectValue.ToStringAdvanced() );
		}
	}


	///Use BBParameter to create a parameter possible to be linked to a blackboard Variable
	[Serializable]
	public class BBParameter<T> : BBParameter{

	    public BBParameter() {}
        public BBParameter(T value) { _value = value; }

	    //delegates for Variable binding
		private Func<T> getter;
		private Action<T> setter;
		//

		[SerializeField]
		protected T _value;
		new public T value{
			get
			{
				if (getter != null){
					return getter();
				}

				//Dynamic?
				if (name != null && bb != null){
					//setting the varRef property also binds it.
					//this will not create a new var but get one if exists already.
					varRef = bb.GetVariable(name, varType);
					return getter != null? getter() : default(T);
				}

				return _value;
			}
			set
			{
				if (setter != null){
					setter(value);
					return;
				}
				
				if (isNone){
					return;
				}

				//Dynamic?
				if (name != null && bb != null){
					//setting the varRef property also binds it
					//this will create a new var if one does not exists
					varRef = bb.SetValue(name, value);
					return;
				}

				_value = value;
			}
		}
		
		protected override object objectValue{
			get {return value;}
			set {this.value = (T)value;}
		}
		
		public override Type varType{
			get {return typeof(T);}
		}

		///Binds this BBParameter to a Variable. Null unbinds
		protected override void Bind(Variable data){
			if (data == null){
				getter = null;
				setter = null;
				_value = default(T);
				return;
			}

			BindGetter(data);
			BindSetter(data);
		}

		//Bind the Getter
		bool BindGetter(Variable data){
			if (data is Variable<T>){
				getter = (data as Variable<T>).GetValue;
				return true;
			}

			if (data.CanConvertTo(varType)){
				var func = data.GetGetConverter(varType);
				getter = ()=>{ return (T)func(); };
				return true;
			}

			return false;
		}

		//Bind the Setter
		bool BindSetter(Variable data){
			if (data is Variable<T>){
				setter = (data as Variable<T>).SetValue;
				return true;
			}

			if (data.CanConvertFrom(varType)){
				var func = data.GetSetConverter(varType);
				setter = (T value)=>{ func(value); };
				return true;
			}

			return false;
		}


	    public static implicit operator BBParameter<T>(T value) {
	        return new BBParameter<T>{value = value};
	    }
/*
	    public static implicit operator T(BBParameter<T> param) {
	        return param.value;
	    }
*/
	}
}