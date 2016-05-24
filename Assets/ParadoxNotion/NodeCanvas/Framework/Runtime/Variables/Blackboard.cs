﻿using System;
using System.Collections.Generic;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Serialization;
using UnityEngine;
using System.Linq;

namespace NodeCanvas.Framework{

	/// <summary>
	/// A Blackboard component to hold variables
	/// </summary>
    public class Blackboard : MonoBehaviour, ISerializationCallbackReceiver, IBlackboard{

		[SerializeField]
		private string _serializedBlackboard;
		[SerializeField]
		private List<UnityEngine.Object> _objectReferences;

		[NonSerialized]
		private BlackboardSource _blackboard = new BlackboardSource();
		[NonSerialized]
		private bool hasDeserialized = false;


		//serialize blackboard variables to json
		public void OnBeforeSerialize(){
			if (_objectReferences != null && _objectReferences.Any(o => o != null)){
				hasDeserialized = false;
			}
			#if UNITY_EDITOR
			if (JSONSerializer.applicationPlaying) return;
			_objectReferences = new List<UnityEngine.Object>();
			_serializedBlackboard = JSONSerializer.Serialize(typeof(BlackboardSource), _blackboard, false, _objectReferences);
			#endif
		}


		//deserialize blackboard variables from json
		public void OnAfterDeserialize(){
			if (hasDeserialized && JSONSerializer.applicationPlaying) return; //avoid double call that Unity does (bug?)
			hasDeserialized = true;
			_blackboard = JSONSerializer.Deserialize<BlackboardSource>(_serializedBlackboard, _objectReferences);
			if (_blackboard == null) _blackboard = new BlackboardSource();
		}


		void Awake(){
			//Call to bind the variables with respected properties on the target game object
			_blackboard.InitializePropertiesBinding(propertiesBindTarget, false);
		}

		new public string name{
			get {return string.IsNullOrEmpty(_blackboard.name)? gameObject.name + "_BB" : _blackboard.name;}
			set
			{
				if (string.IsNullOrEmpty(value)){
					value = gameObject.name + "_BB";
				}
				_blackboard.name = value;
			}
		}

		///An indexer to access variables on the blackboard. It's recomended to use GetValue<T> instead
		public object this[string varName]{
			get { return _blackboard[varName]; }
			set { SetValue(varName, value); }
		}

		///The raw variables dictionary. It's highly recomended to use the methods available to access it though
		public Dictionary<string, Variable> variables{
			get {return _blackboard.variables;}
			set {_blackboard.variables = value;}
		}

		///The GameObject target to do variable/property binding
		public GameObject propertiesBindTarget{
			get {return gameObject;}
		}

		///Add a new variable of name and type
		public Variable AddVariable(string name, Type type){
			return _blackboard.AddVariable(name, type);
		}

		///Get a Variable of name and optionaly type
		public Variable GetVariable(string name, Type ofType = null){
			return _blackboard.GetVariable(name, ofType);
		}

		///Get a Variable of ID and optionaly type
		public Variable GetVariableByID(string ID){
			return _blackboard.GetVariableByID(ID);
		}

		//Generic version of get variable
		public Variable<T> GetVariable<T>(string name){
			return _blackboard.GetVariable<T>(name);
		}

		///Get the variable value of name
		public T GetValue<T>(string name){
			return _blackboard.GetValue<T>(name);
		}

		///Set the variable value of name
		public Variable SetValue(string name, object value){
			return _blackboard.SetValue(name, value);
		}

		///Get all variable names
		public string[] GetVariableNames(){
			return _blackboard.GetVariableNames();
		}

		///Get all variable names of type
		public string[] GetVariableNames(Type ofType){
			return _blackboard.GetVariableNames(ofType);
		}

		////////////////////
		//SAVING & LOADING//
		////////////////////

		///Saves the blackboard with the blackboard name as saveKey.
		public string Save(){ return Save(this.name); }
		///Saves the Blackboard in PlayerPrefs in the provided saveKey. You can use this for a Save system
		public string Save(string saveKey){
			var json = this.Serialize();
			PlayerPrefs.SetString(saveKey, json);
			return json;
		}

		///Loads a blackboard with this blackboard name as saveKey.
		public bool Load(){	return Load(this.name); }
		///Loads back the Blackboard from PlayerPrefs of the provided saveKey. You can use this for a Save system
		public bool Load(string saveKey){

			var json = PlayerPrefs.GetString(saveKey);
			if (string.IsNullOrEmpty(json)){
				Debug.Log("No data to load blackboard variables from key " + saveKey);
				return false;
			}

			return this.Deserialize(json);
		}


		///Serialize the blackboard to json
		public string Serialize(){
			return JSONSerializer.Serialize(typeof(BlackboardSource), _blackboard, false, _objectReferences);
		}

		///Deserialize the blackboard from json
		//We deserialize ON TOP of existing variables so that outside references to them are stay intact.
		public bool Deserialize(string json){
			var bb = JSONSerializer.Deserialize<BlackboardSource>(json, _objectReferences);
			if (bb == null){
				return false;
			}

			foreach (var pair in bb.variables){
				if (_blackboard.variables.ContainsKey(pair.Key)){
					_blackboard.SetValue(pair.Key, pair.Value.value);
				} else {
					_blackboard.variables[pair.Key] = pair.Value;
				}
			}

			var keys = new List<string>(_blackboard.variables.Keys);
			foreach(string key in keys){
				if (!bb.variables.ContainsKey(key)){
					_blackboard.variables.Remove(key);
				}
			}
			
			_blackboard.InitializePropertiesBinding(propertiesBindTarget, true);
			return true;
		}
	}
}