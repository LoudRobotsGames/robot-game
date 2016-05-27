using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MechHelper
{
    private static List<MechSystem> s_MechSystemList = new List<MechSystem>(16);

    public static MechSystem GetPart(GameObject go, MechSystem.SystemLocation locationType)
    {
        s_MechSystemList.Clear();
        go.GetComponentsInChildren<MechSystem>(true, s_MechSystemList);

        for (int i = 0; i < s_MechSystemList.Count; ++i)
        {
            if (s_MechSystemList[i].m_SystemLocation == locationType)
            {
                return s_MechSystemList[i];
            }
        }
        return null;
    }
}
