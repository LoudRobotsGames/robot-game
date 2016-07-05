using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BlueprintFactory
{
    public static void CreateBlueprint<T>() where T : ScriptableObject
    {
        string type = typeof(T).ToString();
        Debug.Log("Create " + type);
        T blueprint = ScriptableObject.CreateInstance<T>();
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        path = path + string.Format("/New{0}.asset", type);
        AssetDatabase.CreateAsset(blueprint, AssetDatabase.GenerateUniqueAssetPath(path));

    }

    [UnityEditor.MenuItem("Assets/Create/Locomotion Blueprint")]
    public static void CreateLocomotion()
    {
        BlueprintFactory.CreateBlueprint<LocomotionBlueprint>();
    }


    [UnityEditor.MenuItem("Assets/Create/Weapon Blueprint")]
    public static void CreateWeapon()
    {
        BlueprintFactory.CreateBlueprint<WeaponBlueprint>();
    }


    [UnityEditor.MenuItem("Assets/Create/Core Blueprint")]
    public static void CreateCore()
    {
        BlueprintFactory.CreateBlueprint<CoreBlueprint>();
    }

    [UnityEditor.MenuItem("Assets/Create/Mech Blueprint")]
    public static void CreateMech()
    {
        BlueprintFactory.CreateBlueprint<MechBlueprint>();
    }
}
