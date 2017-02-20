using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WeaponBlueprint : BaseBlueprint
{
    public GameObject Model;
    public ShotType ShotType = ShotType.Projectile;
    public float MaxVerticalAngle = 10f;
    public float MaxHorizontalAngle = 5f;
    public float ConvergenceRate = 0.125f;

}
