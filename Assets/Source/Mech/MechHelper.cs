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

    public static GameObject CreateMech(MechBlueprint blueprint)
    {
        // Root mech object
        GameObject mech = GameObject.Instantiate<GameObject>(blueprint.Locomotion.Model);
        mech.name = blueprint.name;
        Transform mount = mech.transform.FindChild(blueprint.Locomotion.PathToTopMount);

        // Core system, attached to top mount on locomotion
        GameObject core = GameObject.Instantiate<GameObject>(blueprint.Core.Model);
        core.name = blueprint.Core.Model.name;
        core.transform.SetParent(mount, false);

        MechSystem system = core.AddComponent<MechSystem>();
        blueprint.Core.InitializeSystem(system);    

        mount = core.transform.FindChild(blueprint.Core.LeftWeaponMount);
        GameObject weapon = GameObject.Instantiate<GameObject>(blueprint.LeftWeapon.Model);
        weapon.transform.SetParent(mount, false);
        Animator anim = weapon.GetComponent<Animator>();
        anim.Stop();

        mount = core.transform.FindChild(blueprint.Core.RightWeaponMount);
        weapon = GameObject.Instantiate<GameObject>(blueprint.RightWeapon.Model);
        weapon.transform.SetParent(mount, false);
        anim = weapon.GetComponent<Animator>();
        anim.Stop();

        MechComputer computer = mech.AddComponent<MechComputer>();
        computer.m_MechRoot = mech;
        computer.InitComputer();

        return mech;
    }

    public static void SetupExplodeComponent(MechSystem system, GameObject prefab)
    {
        Explode explode = system.gameObject.AddComponent<Explode>();

        explode.m_Prefab = prefab;

        system.m_ExplodeOnDestruction = explode;
    }
}
