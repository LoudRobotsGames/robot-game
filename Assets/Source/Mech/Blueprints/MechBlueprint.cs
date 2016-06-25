using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MechBlueprint : ScriptableObject
{
    public WeaponBlueprint LeftWeapon;
    public WeaponBlueprint RightWeapon;
    public CoreBlueprint Core;
}
