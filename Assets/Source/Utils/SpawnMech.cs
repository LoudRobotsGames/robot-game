using UnityEngine;
using System.Collections;

public class SpawnMech : MonoBehaviour
{
    public MechBlueprint m_Blueprint;

    public void Spawn()
    {
        MechHelper.CreateMech(m_Blueprint);
    }

}
