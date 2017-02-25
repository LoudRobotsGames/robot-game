using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CoreBlueprint : BaseBlueprint
{
    public string LeftWeaponMount;
    public string RightWeaponMount;
    public string TopWeaponMount;
    public string RotatorJoint;
    public string TiltJoint;
    public Vector3 CockpitAimPoint;
    public Vector3 CockpitCameraOffset;
    public GameObject Model;
    public GameObject Explosion;

    public void InitializeSystem(MechSystem system)
    {
        system.SetupCommonState(this);

        system.m_SystemLocation = MechSystem.SystemLocation.CenterTorso;
        if (Explosion != null)
        {
            MechHelper.SetupExplodeComponent(system, Explosion);
            system.m_DestroyType = MechSystem.DestroyType.ExplodeAndNotify;
        }
    }
}
