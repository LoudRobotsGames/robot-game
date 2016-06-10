using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal);
}
