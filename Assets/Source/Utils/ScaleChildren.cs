using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ScaleChildren : MonoBehaviour
{
    public Vector3 overrideScale = Vector3.one;

    public void Awake()
    {
        UpdateScale();
    }

    [ContextMenu("Update")]
    public void OnTransformChildrenChanged()
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        Debug.Log("Ahhh");
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform t = transform.GetChild(i);
            t.localScale = overrideScale;
            Debug.Log("Scale: " + overrideScale.ToString());
        }
    }
}
