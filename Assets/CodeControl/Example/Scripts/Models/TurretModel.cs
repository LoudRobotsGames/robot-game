using UnityEngine;
using System.Collections;
using CodeControl;

namespace CodeControl.Example
{
    public class TurretModel : Model
    {

        public Color Color;
        public Vector3 Position;
        public ModelRef<TurretModel> TargetTurret;
        public float TimeSinceLastShot;

    }
}