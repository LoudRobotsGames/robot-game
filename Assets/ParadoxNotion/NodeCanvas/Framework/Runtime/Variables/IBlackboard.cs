using System;
using System.Collections.Generic;
using UnityEngine;


namespace NodeCanvas.Framework {

    /// <summary>
    /// An interface for Blackboards, or otherwise for a Variables container.
    /// </summary>
    [ParadoxNotion.Design.SpoofAOT]
    public interface IBlackboard
    {
        string name { get; set; }
        Dictionary<string, Variable> variables { get; set; }
        GameObject propertiesBindTarget { get; }

        Variable AddVariable(string varName, Type type);
        Variable GetVariable(string varName, Type ofType = null);
        Variable GetVariableByID(string ID);
        Variable<T> GetVariable<T>(string varName);
        T GetValue<T>(string varName);
        Variable SetValue(string varName, object value);
        string[] GetVariableNames();
        string[] GetVariableNames(Type ofType);
    }
}