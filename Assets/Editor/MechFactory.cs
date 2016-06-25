using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MechFactory
{
    [UnityEditor.MenuItem("Assets/Create/Mech Blueprint")]
    public static void Create()
    {
        Debug.Log("Create");
        MechBlueprint blueprint = ScriptableObject.CreateInstance<MechBlueprint>();
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        AssetDatabase.CreateAsset(blueprint, AssetDatabase.GenerateUniqueAssetPath(path + "/New Blueprint.asset"));

    }
}
