using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DamageMech : MonoBehaviour
{
    public void DamageRandomMechSystem()
    {
        MechSystem[] systems = FindObjectsOfType<MechSystem>();

        MechSystem system = systems[UnityEngine.Random.Range(0, systems.Length)];
        system.TakeDamage(UnityEngine.Random.Range(1, 5), Vector3.zero, Vector3.forward);
    }
}
